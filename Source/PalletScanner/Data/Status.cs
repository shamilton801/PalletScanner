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
        public IEnumerable<BarcodeRead> AssociatedBarcodeReads = [];
        public IEnumerable<Status> ChildStatus = [];
    }
}