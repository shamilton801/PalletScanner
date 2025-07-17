using PalletScanner.Data;

namespace PalletScanner.Customers.Interface
{
    public interface IValidator
    {
        delegate void StatusUpdateHandler(IValidator sender, IEnumerable<IStatus> newStatus);

        IEnumerable<IStatus> Status { get; }
        event StatusUpdateHandler StatusChanged;
        void AddBarcodeRead(BarcodeRead barcodeRead);
    }

    public abstract class AbstractValidator : IValidator
    {
        public abstract IEnumerable<IStatus> Status { get; }
        private IValidator.StatusUpdateHandler? _statusUpdateHandlers = null;
        public event IValidator.StatusUpdateHandler StatusChanged
        {
            add => _statusUpdateHandlers += value;
            remove => _statusUpdateHandlers -= value;
        }
        protected void NotfifyStatusUpdated() => _statusUpdateHandlers?.Invoke(this, Status);

        public abstract void AddBarcodeRead(BarcodeRead barcodeRead);
    }
}