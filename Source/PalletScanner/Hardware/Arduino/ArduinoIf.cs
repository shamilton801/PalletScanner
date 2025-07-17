using System.IO.Ports;

namespace PalletScanner.Hardware.Arduino
{
    public class ArduinoIf
    {
        private const byte START_BYTE = 0xD0;
        private const byte STOP_BYTE = 0xD1;
        private const string PORT = "COM4";
        private const int BAUD = 9600;

        private static SerialPort port = new SerialPort(PORT, BAUD, Parity.None, 8, StopBits.One);
        
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
