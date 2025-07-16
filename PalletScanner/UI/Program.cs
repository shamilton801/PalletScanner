using PalletScanner.CustomerInterface;
using PalletScanner.Customers.Tyson;
using PalletScanner.HardwareInterface.Cameras;
using System.Net;


ICamera[] cameras = [
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

ICustomer tyson = new Tyson();
IValidator validator = tyson.CreateValidationSession();

// TODO: Session management. 
foreach (var barcode in cameras.ReadAllBarcodes().ToEnumerable())
{
    validator.AddBarcodeRead(barcode);
    Console.WriteLine($"{barcode.BarcodeContent} -> {validator.Status.Last().Message}");
}
