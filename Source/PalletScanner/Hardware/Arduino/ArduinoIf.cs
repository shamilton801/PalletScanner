using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

namespace PalletScanner.Hardware.Arduino
{
    public class ArduinoIf
    {
        private const byte START_BYTE = 0xD0;
        private const byte STOP_BYTE = 0xD1;

        private static SerialPort port = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
        
        public static void StartScanning()
        {
            port.Write([START_BYTE], 0, 1);
        }

        public static void StopScanning()
        {
            port.Write([STOP_BYTE], 0, 1);
        }
    }
}
