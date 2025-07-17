using PalletScanner.Hardware.Cameras;
using System.Drawing;
using System.Numerics;

namespace PalletScanner.Data
{
    public struct SpatialProperties(Vector3 location, SizeF size)
    {
        public readonly Vector3 Location => location;
        public readonly SizeF Size => size;

        public static SpatialProperties operator *(Pose txfm, SpatialProperties val) =>
            new(txfm * val.Location, val.Size);
    }

    public class BarcodeTracker3D
    {
        public ICamera AssociatedCamera { get; }
        private readonly float InvFocalLength;
        private readonly Vector2 PrincipalPoint;
        private readonly CameraParameters? CameraParams;
        private readonly Pose? CameraPose;

        public BarcodeTracker3D(ICamera camera)
        {
            AssociatedCamera = camera;
            CameraParams = camera.CameraParams;
            CameraPose = camera.CameraPose;
            PrincipalPoint = CameraParams?.PrincipalPoint.ToVector2() ?? default;
            InvFocalLength = (1 / CameraParams?.FocalLengthPixels) ?? default;
        }

        public SpatialProperties? ComputeWorld(BarcodeRead barcode, SizeF physicalModuleSize) =>
            CameraPose * ComputeLocal(barcode, physicalModuleSize);
        public SpatialProperties? ComputeLocal(BarcodeRead barcode, SizeF physicalModuleSize)
        {
            if (!CameraParams.HasValue) return null;
            var corners = barcode.BarcodeCorners;
            if (corners == null || corners.Length != 4) return null;
            return Estimate(
                corners.Select(c => c.ToVector2()).ToArray(),
                barcode.ModuleSizePixels,
                barcode.BaselineAngle,
                physicalModuleSize);
        }


        private SpatialProperties? Estimate(
            Vector2[] corners,      // In pixels (reported from barcode scanner)
            float moduleSize,       // In pixels (reported from barcode scanner)
            float orientationAngle, // In degrees (reported from barcode scanner, positively oriented)
            SizeF PhysicalBarSize   // In real-world units
        )
        {
            orientationAngle *= MathF.PI / 180;
            var quad = new Vector2[4];
            Array.Copy(corners, quad, quad.Length);
            if (!PermuteCorners(corners, orientationAngle)) return null;
            var c00 = Pt2dir(corners[0]);
            var cx0 = Pt2dir(corners[1]);
            var cxy = Pt2dir(corners[2]);
            var c0y = Pt2dir(corners[3]);
            Vector2 viewX = (cxy + cx0 - c0y - c00) / 2;
            Vector2 viewY = (cxy + c0y - cx0 - c00) / 2;
            var estSize = new SizeF(
                PhysicalBarSize.Width * viewX.Length() / (moduleSize * InvFocalLength),
                PhysicalBarSize.Height);
            viewX /= estSize.Width;
            viewY /= estSize.Height;
            var depth = 1 / MathF.Sqrt(MaxEigenvalue(viewX, viewY));
            var estLocation = new Vector3((c00 + cx0 + c0y + cxy) * depth / 4, depth);
            return new SpatialProperties(estLocation, estSize);
        }

        /// <summary>
        /// Given a 2x2 matrix <c>A</c>, computes the maximum eigenvalue of <c>A * A^T</c>
        /// </summary>
        /// <param name="c1">First column of <c>A</c></param>
        /// <param name="c2">Second column of <c>A</c></param>
        /// <returns>The maximum eigenvalue of <c>A</c></returns>
        private static float MaxEigenvalue(Vector2 c1, Vector2 c2)
        {
            var mean = (c1.LengthSquared() + c2.LengthSquared()) / 2;
            var det = Cross(c1, c2);
            var sqrPart = mean * mean + det * det;
            return sqrPart >= 0 ? mean + MathF.Sqrt(sqrPart) : mean;
        }
        /// <summary>
        /// Permutes the corners of the given quad so that its corners are in the order
        /// <list type="number">
        ///   <item>Origin</item>
        ///   <item>Opposite origin along x-axis</item>
        ///   <item>Diagnoally opposite origin</item>
        ///   <item>Opposite origin along y-axis</item>
        /// </list>
        /// </summary>
        /// <param name="quad">The quad to modify</param>
        /// <param name="orientationAngle">The angle of the quad-local x-axis with the world x-axis</param>
        /// <returns><c>true</c> if and only if the corners have valid geometry and are permuted.</returns>
        private static bool PermuteCorners(Vector2[] quad, float orientationAngle)
        {
            /* Put corners in polygon order */ {
                if (Intersect(quad[0], quad[1], quad[2], quad[3]))
                    (quad[1], quad[2]) = (quad[2], quad[1]);
                else if (Intersect(quad[0], quad[2], quad[1], quad[3]))
                { } // Do nothing
                else if (Intersect(quad[0], quad[3], quad[1], quad[2]))
                    (quad[2], quad[3]) = (quad[3], quad[2]);
                else return false;
            }
            /* Put corners in counterclockwise order */ {
                var a = quad[1] - quad[0];
                var b = quad[3] - quad[0];
                if (Cross(a, b) < 0)
                    (quad[1], quad[3]) = (quad[3], quad[1]);
            }
            /* Rotate to match orientationAngle */ {
                var (s, c) = MathF.SinCos(orientationAngle);
                Vector2 orientationDir = new(c, s);
                float bestFit = float.NegativeInfinity;
                int bestIdx = 0;
                for (int i = 0; i < 4; ++i)
                {
                    Vector2 dir = quad[(i + 1) % 4] - quad[i];
                    float fit = Vector2.Dot(orientationDir, dir) / dir.Length();
                    if (fit > bestFit)
                    {
                        bestFit = fit;
                        bestIdx = i;
                    }
                }
                switch (bestIdx & 3)
                {
                    case 0:
                        break;
                    case 1:
                        (quad[0], quad[1], quad[2], quad[3]) = (quad[1], quad[2], quad[3], quad[0]);
                        break;
                    case 2:
                        (quad[0], quad[1], quad[2], quad[3]) = (quad[2], quad[3], quad[0], quad[1]);
                        break;
                    case 3:
                        (quad[0], quad[1], quad[2], quad[3]) = (quad[3], quad[0], quad[1], quad[2]);
                        break;
                }
            }
            return true;
        }

        private static bool Intersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            Vector2 aDir = a2 - a1;
            Vector2 bDir = b2 - b1;
            Vector2 deltaStart = b1 - a1;
            var aParam = Cross(deltaStart, bDir);
            var bParam = Cross(deltaStart, aDir);
            var paramDen = Cross(aDir, bDir);
            if (paramDen < 0)
            {
                aParam = -aParam;
                bParam = -bParam;
                paramDen = -paramDen;
            }
            return aParam > 0 && aParam < paramDen && bParam > 0 && bParam < paramDen;
        }
        private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
        private Vector2 Pt2dir(Vector2 pt) => (pt - PrincipalPoint) * InvFocalLength;
    }
}