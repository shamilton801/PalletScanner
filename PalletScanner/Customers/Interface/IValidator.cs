using PalletScanner.Data;

namespace PalletScanner.Customers.Interface
{
    public interface IValidator
    {
        IList<Status> Status { get; }
        void AddBarcodeRead(BarcodeRead barcodeRead);
    }
}