using PalletScanner.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PalletScanner.Hardware.Cameras
{
    /// <summary>
    /// Intrinsic camera parameters
    /// </summary>
    public struct CameraParameters
    {
        private PointF? PrincipalPointOverride;

        /// <summary>
        /// The size of each output frame.
        /// </summary>
        public Size CameraSize { readonly get; set; }
        /// <summary>
        /// Number of pixels away from the PrincipalPoint at 45 degrees from the optical axis.
        /// </summary>
        public float FocalLengthPixels { readonly get; set; }
        /// <summary>
        /// The pixel location of the optical axis.
        /// </summary>
        public PointF PrincipalPoint
        {
            readonly get => PrincipalPointOverride ??
                new(CameraSize.Width / 2.0f, CameraSize.Height / 2.0f);
            set => PrincipalPointOverride = value;
        }
        /// <summary>
        /// Internally reset the PrincipalPoint, tying it to the center of CameraSize.
        /// </summary>
        public void UseDefaultPrincipalPoint() => PrincipalPointOverride = null;
    }

    public interface ICamera
    {
        string Name { get; }
        IAsyncEnumerable<BarcodeRead> ReadBarcodes();

        /// <summary>
        /// Local axes:
        /// <list type="table">
        /// <listheader>
        ///     <term>Axis</term>
        ///     <description>Local Direction</description>
        /// </listheader>
        /// <item>
        ///     <term>X</term>
        ///     <description>Right</description>
        /// </item>
        /// <item>
        ///     <term>Y</term>
        ///     <description>Down</description>
        /// </item>
        /// <item>
        ///     <term>Z</term>
        ///     <description>Backward</description>
        /// </item>
        /// </list>
        /// </summary>
        Pose? CameraPose { get; }
        CameraParameters? CameraParams { get; }
    }

    public abstract class AbstractCamera : ICamera
    {
        public abstract string Name { get; }
        public Pose? CameraPose { get; set; } = null;
        public CameraParameters? CameraParams { get; set; } = null;
        public abstract IAsyncEnumerable<BarcodeRead> ReadBarcodes();
    }

    public static class CameraEx
    {
        public static IAsyncEnumerable<BarcodeRead> ReadAllBarcodes(this IEnumerable<ICamera> cameras) =>
            AsyncEnumerableEx.Merge(cameras.Select(cam => cam.ReadBarcodes()).ToArray());
    }
}