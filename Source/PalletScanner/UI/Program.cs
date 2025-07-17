using PalletScanner.Customers.Interface;
using PalletScanner.Customers.Tyson;
using PalletScanner.Data;
using PalletScanner.Hardware.StartStop;
using PalletScanner.Hardware.Cameras;
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
    validation.StatusChanged += Validation_StatusChanged;
    Validation_StatusChanged(validation, validation.Status);
    try
    {
        await foreach (var barcode in cameras.ReadAllBarcodes().WithCancellation(token))
        {
            Console.WriteLine("Barcode scanned: " + barcode.BarcodeContent);
            Console.WriteLine("Barcode location/size: " + (barcode.Compute3D(new(0.25f, 18.0f))?.ToString() ?? "<Invalid>"));
            validation.AddBarcodeRead(barcode);
        }
    }
    finally
    {
        Validation_StatusChanged(validation, validation.Status);
    }
}

static void Validation_StatusChanged(IValidator sender, IEnumerable<Status> newStatus)
{
    foreach (var status in newStatus)
    {
        Console.WriteLine(status.Type switch
        {
            StatusType.Info     => "   Info: ",
            StatusType.Warning  => "WARNING: ",
            StatusType.Error    => "  ERROR: ",
            _                   => "Unknown: "
        } + status.Message);
    }
}

static void RunToCancel(Func<CancellationToken, Task> task)
{
    CancellationTokenSource cts = new();
    Task.WhenAll(Task.Run(() =>
    {
        using IStartStop startStop = isJordan ? new TestStartStop() : new ArduinoIf("COM3");
        startStop.StartTriggered += s => s.StartScanning();
        startStop.StopTriggered += s => s.StopScanning();

        bool running = true;
        while (running)
        {
            Console.WriteLine("s: start | p: stop | q: quit");
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.S:
                    Console.WriteLine("Starting Scan");
                    startStop.StartScanning();
                    break;
                case ConsoleKey.P:
                    Console.WriteLine("Stopping Scan");
                    startStop.StopScanning();
                    break;
                case ConsoleKey.Q:
                    Console.WriteLine("Quitting");
                    running = false;
                    break;
            }
        }
        startStop.StopScanning();
        cts.Cancel();
    }), task(cts.Token)).WaitForCancel();
}

ICamera[] CreateJordansCameras() => [
    new TestCamera("Test"),
    new DatamanNetworkCamera(IPAddress.Parse("192.168.1.42"), "DM262-852514") {
        CameraPose = Pose.Identity,
        CameraParams = new() {
            CameraSize = new(1280, 960),
            FocalLengthPixels = 1000
        }
    }
];
ICamera[] CreateScannerCameras() => [
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.103"), "7-1-DM3812-371BE6"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.104"), "6-1-DM3812-371068"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.105"), "5-1-DM3812-371D96"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.106"), "4-1-DM3812-36A236"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.107"), "3-1-DM3812-371E12"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.108"), "2-1-DM3812-371DA6"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.109"), "1-1-DM3812-371E32"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.113"), "7-2-DM3812-371D9A"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.114"), "6-2-DM3812-371CAA"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.115"), "5-2-DM3812-370EE4"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.116"), "4-2-DM3812-371DAE"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.117"), "3-2-DM3812-371DB6"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.118"), "2-2-DM3812-371CA6"),
    new DatamanNetworkCamera(IPAddress.Parse("10.191.0.119"), "1-2-DM3812-36E25C"),
];