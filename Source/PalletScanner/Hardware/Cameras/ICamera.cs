using PalletScanner.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PalletScanner.Hardware.Cameras
{
    public interface ICamera
    {
        string Name { get; }
        IAsyncEnumerable<BarcodeRead> ReadBarcodes();
    }

    public static class CameraEx
    {
        public static IAsyncEnumerable<BarcodeRead> ReadAllBarcodes(this IEnumerable<ICamera> cameras) =>
            AsyncEnumerableEx.Merge(cameras.Select(cam => cam.ReadBarcodes()).ToArray());
    }
}