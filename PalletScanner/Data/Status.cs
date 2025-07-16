namespace PalletScanner.Data
{
    public enum StatusType
    {
        Info, Warning, Error
    }

    public struct Status
    {
        public StatusType Type { get; set; }
        public string Message { get; set; }
        public BarcodeRead? AssociatedBarcodeRead { get; set; }
    }
}