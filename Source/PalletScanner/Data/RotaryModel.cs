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

        public event IStartStop.Trigger StartTriggered
        {
            add => startStop.StartTriggered += value;
            remove => startStop.StartTriggered -= value;
        }
        public event IStartStop.Trigger StopTriggered
        {
            add => startStop.StopTriggered += value;
            remove => startStop.StopTriggered -= value;
        }
        public void StartScan() => StartScanInner(null);
        private CancellationTokenSource? StartScanInner(Action? onStop)
        {
            if (_threadData != null) return null;
            var cts = new CancellationTokenSource();
            _threadData = new ThreadData()
            {
                Cts = cts,
                Thd = new Thread(() => ValidationThread(cts.Token, onStop))
            };
            _threadData.Value.Thd.Start();
            startStop.StartScanning();
            return cts;
        }

        public void StopScan()
        {
            var data = _threadData;
            if (data == null) return;
            data.Value.Cts.Cancel();
            data.Value.Thd.Join();
        }

        private static readonly TimeSpan ScanTime = TimeSpan.FromSeconds(5);
        public void StartTimedScan(Action? onStop = null) => StartScanInner(onStop)?.CancelAfter(ScanTime);
        private void ValidationThread(CancellationToken token, Action? onStop)
        {
            try
            {
                var validator = customer.CreateValidationSession();
                validator.StatusChanged += (sender, args) => _StatusUpdated?.Invoke(args);
                _StatusUpdated?.Invoke(validator.Status);
                ReadBarcodesToValidatorAsync(validator, token).WaitForCancel();
                _StatusUpdated?.Invoke(validator.Status);
            }
            finally
            {
                startStop.StopScanning();
                _threadData = null;
                onStop?.Invoke();
            }
        }

        private async Task ReadBarcodesToValidatorAsync(IValidator validator, CancellationToken token)
        {
            await foreach (var barcode in cameras.ReadAllBarcodes().WithCancellation(token))
                validator.AddBarcodeRead(barcode);
        }
    }
}
