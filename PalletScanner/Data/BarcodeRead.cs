using PalletScanner.HardwareInterface.Cameras;
using System.Drawing;

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
    }
}