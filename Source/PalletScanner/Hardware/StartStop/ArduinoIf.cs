using System.IO.Ports;
using PalletScanner.Customers.Interface;
using PalletScanner.Data;
using System.Threading;
using System.Runtime.CompilerServices;

namespace PalletScanner.Hardware.StartStop
{
    public class ArduinoIf : AbstractStartStop
    {
        private const byte START_BYTE = 0xD0;
        private const byte STOP_BYTE = 0xD1;
        private const int BAUD = 9600;

        private readonly SerialPort port;
        private readonly object _lock = new();

        private bool _disposed = false;

        public ArduinoIf(string PORT)
        {
            port = new(PORT, BAUD, Parity.None, 8, StopBits.One);
            port.DataReceived += Port_DataReceived;
            port.DtrEnable = true;
            port.Open();
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buf;
            lock (_lock)
            {
                SerialPort spL = (SerialPort)sender;
                buf = new byte[spL.BytesToRead];
                Console.WriteLine(System.Text.Encoding.UTF8.GetString(buf));

                if (spL.BytesToRead > 1)
                {
                    //Console.WriteLine("More than 1 bytes received from arduino. This indicates an issue with the arduino logic");
                }

                spL.Read(buf, 0, buf.Length);
            }
            
            // We only care about the last byte received
            switch (buf.Last())
            {
                case START_BYTE: OnStartTrigger(); break;
                case STOP_BYTE: OnStopTrigger(); break;
            }
        }

        public override void StartScanning()
        {
            lock (_lock)
            {
                if (_disposed) return;
                port.Write([START_BYTE], 0, 1);
            }
        }

        public override void StopScanning()
        {
            lock (_lock)
            {
                if (_disposed) return;
                port.Write([STOP_BYTE], 0, 1);
            }
        }

        public void timer_Elapsed(object state)
        {
            StopScanning();
        }

        protected override void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (_disposed) return;
                port.Close();
                _disposed = true;
            }
        }

        ~ArduinoIf()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }
    }
}
