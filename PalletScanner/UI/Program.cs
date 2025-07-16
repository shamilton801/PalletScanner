using PalletScanner.HardwareInterface.Cameras;
using System.Net;

ICamera[] cameras = [];// [ CreateJordansCamera() ];
foreach (var barcode in cameras.ReadAllBarcodes().ToEnumerable())
    Console.WriteLine(barcode.BarcodeContent);
ICamera CreateJordansCamera() => new DatamanNetworkCamera(IPAddress.Parse("192.168.1.42"), "DM262-852514");