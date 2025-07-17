using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletScanner.Hardware.StartStop
{
    public sealed class TestStartStop : AbstractStartStop
    {
        public void TriggerStart() => OnStartTrigger();
        public void TriggerStop() => OnStopTrigger();
        public override void StartScanning() => Console.WriteLine("Starting Scanning.");
        public override void StopScanning() => Console.WriteLine("Stopping Scanning.");
        protected override void Dispose(bool disposing) => Console.WriteLine("Closing.");
    }
}
