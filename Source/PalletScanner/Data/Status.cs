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
        public StatusType Type { get; set; } = type;
        public string Message { get; set; } = message;
        public List<BarcodeRead> AssociatedBarcodeReads { get; set; } = [];
        IEnumerable<BarcodeRead> IStatus.AssociatedBarcodeReads => AssociatedBarcodeReads;
        public IEnumerable<IStatus> ChildStatus => [];
    }

    public class ParentStatus(StatusType type, string message = "") : IStatus
    {
        public StatusType Type { get; set; } = type;
        public string Message { get; set; } = message;
        public IEnumerable<BarcodeRead> AssociatedBarcodeReads => [];
        public List<IStatus> ChildStatus { get; set; } = [];
        IEnumerable<IStatus> IStatus.ChildStatus => ChildStatus;
    }
}