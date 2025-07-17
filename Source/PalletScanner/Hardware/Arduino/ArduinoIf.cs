using System.IO.Ports;
using PalletScanner.Customers.Interface;
using PalletScanner.Data;

namespace PalletScanner.Hardware.Arduino
{
    public class ArduinoIf
    {
        private const byte START_BYTE = 0xD0;
        private const byte STOP_BYTE = 0xD1;
        private const string PORT = "COM3";
        private const int BAUD = 9600;

        private static SerialPort port = new SerialPort(PORT, BAUD, Parity.None, 8, StopBits.One);

        static ArduinoIf()
        {
            port.DataReceived += Port_DataReceived;
            port.Open();
        }

        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort spL = (SerialPort)sender;
            byte[] buf = new byte[spL.BytesToRead];

            if (spL.BytesToRead > 1)
            {
                //Console.WriteLine("More than 1 bytes received from arduino. This indicates an issue with the arduino logic");
                Console.WriteLine(System.Text.Encoding.UTF8.GetString(buf));
            }

            spL.Read(buf, 0, buf.Length);
            
            // We only care about the last byte received
            if (buf.Last() ==  START_BYTE)
            {
                // bla bla bla trigger an event to start a scan
                // I am tired and want to go to bed
            }
        }

        public static void StartScanning()
        {
            port.Write([START_BYTE], 0, 1);
        }

        public static void StopScanning()
        {
            port.Write([STOP_BYTE], 0, 1);
        }

        public static void Close()
        {
            port.Close();
        }
    }
}
