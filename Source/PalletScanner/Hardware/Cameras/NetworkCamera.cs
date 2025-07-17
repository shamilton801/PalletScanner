using PalletScanner.Data;
using PalletScanner.Utils;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PalletScanner.Hardware.Cameras
{
    public abstract class NetworkCamera(IPEndPoint endPoint, char seperator = '\r') : AbstractCamera
    {
        private readonly TcpClient _client = CreateConnected(endPoint);

        public override IAsyncEnumerable<BarcodeRead> ReadBarcodes() =>
            Encoding.Default.DecodeAsync(_client.Client.ReceiveAllAsync())
            .SplitIntoStrings(seperator).SelectMany(str => ParseMessage(str).ToAsyncEnumerable());

        protected abstract IEnumerable<BarcodeRead> ParseMessage(string message);

        private static TcpClient CreateConnected(IPEndPoint endpoint)
        {
            TcpClient result = new();
            result.Connect(endpoint);
            return result;
        }
    }

    public class DatamanNetworkCamera(IPAddress address, string name) : NetworkCamera(new(address, DatamanPort))
    {
        private const int DatamanPort = 23;

        public override string Name => name;

        protected override IEnumerable<BarcodeRead> ParseMessage(string message) =>
            DataManMessageParser.ParseMessage(this, message);
    }
}