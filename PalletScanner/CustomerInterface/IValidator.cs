using PalletScanner.Data;

namespace PalletScanner.CustomerInterface
{
    public interface IValidator
    {
        IList<Status> Status { get; }
        void AddBarcodeRead(BarcodeRead barcodeRead);
    }
}