using PalletScanner.Customers.Interface;
using PalletScanner.Hardware.Cameras;
using PalletScanner.Hardware.StartStop;
using PalletScanner.Utils;

namespace PalletScanner.Data
{
    public class RotaryModel
    {
        private Action<IEnumerable<IStatus>>? _StatusUpdated = null;
        public event Action<IEnumerable<IStatus>> StatusUpdated
        {
            add => _StatusUpdated += value;
            remove => _StatusUpdated -= value;
        }

        private struct ThreadData
        {
            public CancellationTokenSource Cts;
            public Thread Thd;
        }

        private ThreadData? _threadData = null;
        private readonly ICamera[] cameras;
        private readonly IStartStop startStop;
        private readonly ICustomer customer;

        public RotaryModel(ICamera[] cameras, IStartStop startStop, ICustomer customer)
        {
            this.cameras = cameras;
            this.startStop = startStop;
            this.customer = customer;
            startStop.StartTriggered += _ => StartScan();
            startStop.StopTriggered += _ => StopScan();
        }

        public void StartScan()
        {
            if (_threadData != null) return;
            var cts = new CancellationTokenSource();
            _threadData = new ThreadData()
            {
                Cts = cts,
                Thd = new Thread(() => ValidationThread(cts.Token))
            };
            _threadData.Value.Thd.Start();
            startStop.StartScanning();
        }

        public void StopScan()
        {
            if (_threadData == null) return;
            var data = _threadData.Value;
            _threadData = null;
            startStop.StopScanning();
            data.Cts.Cancel();
            data.Thd.Join();
        }

        private void ValidationThread(CancellationToken token)
        {
            var validator = customer.CreateValidationSession();
            validator.StatusChanged += (sender, args) => _StatusUpdated?.Invoke(args);
            _StatusUpdated?.Invoke(validator.Status);
            ReadBarcodesToValidatorAsync(validator, token).WaitForCancel();
            _StatusUpdated?.Invoke(validator.Status);
        }

        private async Task ReadBarcodesToValidatorAsync(IValidator validator, CancellationToken token)
        {
            await foreach (var barcode in cameras.ReadAllBarcodes().WithCancellation(token))
                validator.AddBarcodeRead(barcode);
        }
    }
}
