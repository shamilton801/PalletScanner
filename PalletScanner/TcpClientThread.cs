using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PalletScanner
{
    public class TcpClientThread
    {
        public delegate void MsgProcessor(ReadOnlySpan<byte> data);
        private MsgProcessor? _MessageReceived = null;
        public event MsgProcessor MessageReceived
        {
            add => _MessageReceived += value;
            remove => _MessageReceived -= value;
        }

        private IPEndPoint _ipEndPoint;
        private Socket _client;

        private readonly object _lock = new();
        private Thread? _recvThread = null;
        private bool _shouldContinue = false;
        private SocketAsyncEventArgs _e = new();

        // Example: 1-1-DM3812-371E32,1392527977<0x0A>3629,613,87,Code 128,1.31640625,0190027182545238320100010011250602210642488165<0x0D>
        public TcpClientThread(string ipaddr, int port)
        {
            _ipEndPoint = IPEndPoint.Parse($"{ipaddr}:{port}");
            _client = new(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            _client.Connect(_ipEndPoint);
            _shouldContinue = true;
            _e.Completed += DataReceived;
            _recvThread = new Thread(RecvLoop);
            _recvThread.Start();
        }

        private void RecvLoop()
        {
            while (true)
            {
                lock (_lock)
                {
                    if (!_client.ReceiveAsync(_e))
                    {
                        throw new IOException();
                    }
                    Monitor.Wait(_lock);
                }
                if (!_shouldContinue) return;
            }
        }

        private void DataReceived(Object sender, SocketAsyncEventArgs e)
        {
            _MessageReceived?.Invoke(e.Buffer);
            Monitor.PulseAll(_lock);
        }

        public void Stop()
        {
            _shouldContinue = false;
            if (_recvThread == null) return;
            Monitor.PulseAll(_lock);
            _recvThread.Join();
        }
    }
}
