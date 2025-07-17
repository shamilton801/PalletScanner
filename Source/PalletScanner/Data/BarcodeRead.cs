using PalletScanner.Hardware.Cameras;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace PalletScanner.Data
{
    public class BarcodeRead(
        ICamera source,
        TimeSpan readTimeOffset,
        string barcodeContent,
        string symbologyType,
        PointF centerPoint,
        float baselineAngle,
        float moduleSizePixels,
        PointF[]? barcodeCorners = null)
    {
        public ICamera Source => source;
        public TimeSpan ReadTimeOffset => readTimeOffset;
        public string BarcodeContent => barcodeContent;
        public string SymbologyType => symbologyType;
        public PointF CenterPoint => centerPoint;
        public float BaselineAngle => baselineAngle;
        public float ModuleSizePixels => moduleSizePixels;
        public PointF[]? BarcodeCorners => barcodeCorners;

        public SpatialProperties? Compute3D(SizeF physicalModuleSize) =>
            GetTracker3D().ComputeWorld(this, physicalModuleSize);
        public SpatialProperties? Compute3DLocal(SizeF physicalModuleSize) =>
            GetTracker3D().ComputeLocal(this, physicalModuleSize);

        private static readonly ConditionalWeakTable<ICamera, BarcodeTracker3D> Tracker3DByCamera = [];
        private BarcodeTracker3D GetTracker3D()
        {
            if (Tracker3DByCamera.TryGetValue(source, out var result)) return result;
            result = new(source);
            Tracker3DByCamera.Add(source, result);
            return result;
        }
    }
}