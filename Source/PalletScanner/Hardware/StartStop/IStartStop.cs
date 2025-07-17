using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletScanner.Hardware.StartStop
{
    public interface IStartStop : IDisposable
    {
        delegate void Trigger(IStartStop sender);

        event Trigger StartTriggered;
        event Trigger StopTriggered;
        void StartScanning();
        void StopScanning();
    }
    public abstract class AbstractStartStop : IStartStop
    {
        private IStartStop.Trigger? _StartTriggered = null;
        private IStartStop.Trigger? _StopTriggered = null;
        public event IStartStop.Trigger StartTriggered
        {
            add => _StartTriggered += value;
            remove => _StartTriggered -= value;
        }
        public event IStartStop.Trigger StopTriggered
        {
            add => _StopTriggered += value;
            remove => _StopTriggered -= value;
        }
        protected void OnStartTrigger() => _StartTriggered?.Invoke(this);
        protected void OnStopTrigger() => _StopTriggered?.Invoke(this);

        public abstract void StartScanning();
        public abstract void StopScanning();

        protected virtual void Dispose(bool disposing) { }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
