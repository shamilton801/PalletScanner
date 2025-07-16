using PalletScanner.CustomerInterface;
using PalletScanner.Customers.Tyson;
using PalletScanner.HardwareInterface.Cameras;
using PalletScanner.Utils;
using System.Net;

const bool isJordan = false;

var cameras = isJordan ? CreateJordansCameras() : CreateScannerCameras();
RunToCancel(tok => Run<Tyson>(cameras, tok));

static async Task Run<Customer>(ICamera[] cameras, CancellationToken token = default)
    where Customer : ICustomer, new()
{
    Customer customer = new();
    Console.WriteLine("Using customer: " + customer.Name);
    var validation = customer.CreateValidationSession();
    await foreach (var barcode in cameras.ReadAllBarcodes().WithCancellation(token))
    {
        Console.WriteLine("Barcode scanned: " + barcode.BarcodeContent);
        validation.AddBarcodeRead(barcode);
    }
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

ICamera[] CreateJordansCameras() => [ new DatamanNetworkCamera(IPAddress.Parse("192.168.1.42"), "DM262-852514") ];
ICamera[] CreateScannerCameras() => [
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.109"), "DM3812-1-1"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.108"), "DM3812-2-1"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.107"), "DM3812-3-1"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.106"), "DM3812-4-1"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.105"), "DM3812-5-1"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.104"), "DM3812-6-1"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.103"), "DM3812-7-1"),

    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.119"), "DM3812-1-2"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.118"), "DM3812-2-2"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.117"), "DM3812-3-2"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.116"), "DM3812-4-2"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.115"), "DM3812-5-2"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.114"), "DM3812-6-2"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.113"), "DM3812-7-2"),
];