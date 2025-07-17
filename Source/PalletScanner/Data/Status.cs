namespace PalletScanner.Data
{
    public enum StatusType
    {
        Info, Warning, Error
    }

    public interface IStatus
    {
        StatusType Type { get; }
        string Message { get; }
        IEnumerable<BarcodeRead> AssociatedBarcodeReads { get; }
        IEnumerable<IStatus> ChildStatus { get; }
    }

    public class LeafStatus(StatusType type, string message = "") : IStatus
    {
        public StatusType Type => type;
        public string Message => message;
        public List<BarcodeRead> AssociatedBarcodeReads { get; } = [];
        IEnumerable<BarcodeRead> IStatus.AssociatedBarcodeReads => AssociatedBarcodeReads;
        public IEnumerable<IStatus> ChildStatus => [];
    }

    public class ParentStatus(StatusType type, string message = "") : IStatus
    {
        public StatusType Type => type;
        public string Message => message;
        public IEnumerable<BarcodeRead> AssociatedBarcodeReads => [];
        public List<IStatus> ChildStatus { get; } = [];
        IEnumerable<IStatus> IStatus.ChildStatus => ChildStatus;
    }
}