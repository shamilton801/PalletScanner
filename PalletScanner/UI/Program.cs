using PalletScanner.HardwareInterface.Cameras;
using PalletScanner.Utils;
using System.Net;

ICamera[] cameras = [];// [ CreateJordansCamera() ];
RunToCancel(tok => Run(cameras, tok));
ICamera CreateJordansCamera() => new DatamanNetworkCamera(IPAddress.Parse("192.168.1.42"), "DM262-852514");

static async Task Run(ICamera[] cameras, CancellationToken token = default)
{
    await foreach (var barcode in cameras.ReadAllBarcodes().WithCancellation(token))
        Console.WriteLine("Barcode scanned: " + barcode.BarcodeContent);
}
static void RunToCancel(Func<CancellationToken, Task> task)
{
    CancellationTokenSource cts = new();
    Task.WhenAll(Task.Run(() =>
    {
        Console.WriteLine("Press any key to cancel...");
        Console.ReadKey();
        cts.Cancel();
    }), task(cts.Token)).WaitForCancel();
    Console.WriteLine("Operation cancelled");
}