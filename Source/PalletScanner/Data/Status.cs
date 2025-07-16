namespace PalletScanner.Data
{
    public enum StatusType
    {
        Info, Warning, Error
    }

    public class Status(StatusType type, string message = "")
    {
        public StatusType Type => type;
        public string Message => message;
        public virtual IEnumerable<BarcodeRead> AssociatedBarcodeReads => [];
    }

    public class BarcodeReadStatus(StatusType type, BarcodeRead barcode, string message = "") : Status(type, message)
    {
        public override IEnumerable<BarcodeRead> AssociatedBarcodeReads => [ barcode ];
    }

    public class BarcodeReadListStatus(StatusType type, string message = "") : Status(type, message)
    {
        public override IEnumerable<BarcodeRead> AssociatedBarcodeReads => BarcodeReads;
        public readonly List<BarcodeRead> BarcodeReads = [];
    }
}