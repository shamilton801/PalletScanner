using PalletScanner.Data;
using PalletScanner.Utils;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PalletScanner.HardwareInterface.Cameras
{
    public abstract class NetworkCamera : ICamera
    {
        public abstract string Name { get; }

        private readonly Socket _client;
        private readonly char seperator;

        public NetworkCamera(IPEndPoint endPoint, char seperator = '\r')
        {
            this.seperator = seperator;
            _client = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _client.Connect(endPoint);
        }

        public IAsyncEnumerable<BarcodeRead> ReadBarcodes() =>
            Encoding.Default.DecodeAsync(_client.ReceiveAllAsync())
            .SplitIntoStrings(seperator).SelectMany(str => ParseMessage(str).ToAsyncEnumerable());

        protected abstract IEnumerable<BarcodeRead> ParseMessage(string message);
    }

    public class DatamanNetworkCamera(IPAddress address, string name) : NetworkCamera(new(address, DatamanPort))
    {
        private const int DatamanPort = 23;

        public override string Name => name;

        protected override IEnumerable<BarcodeRead> ParseMessage(string message) =>
            DataManMessageParser.ParseMessage(this, message);
    }
}