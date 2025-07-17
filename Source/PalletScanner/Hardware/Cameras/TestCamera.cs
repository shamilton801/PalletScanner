using PalletScanner.Data;
using System.Runtime.CompilerServices;

namespace PalletScanner.Hardware.Cameras
{
    public class TestCamera(string name) : AbstractCamera
    {
        public override string Name => name;

        private class Node(BarcodeRead barcode)
        {
            public readonly TaskCompletionSource<Node> Next = new();
            public BarcodeRead Barcode => barcode;
        }
        private TaskCompletionSource<Node> Next = new();

        public void ReadBarcode(BarcodeRead barcodeRead)
        {
            Node n = new(barcodeRead);
            Next.SetResult(n);
            Next = n.Next;
        }

        public override IAsyncEnumerable<BarcodeRead> ReadBarcodes()
        {
            return Core(Next, default);
            static async IAsyncEnumerable<BarcodeRead> Core(TaskCompletionSource<Node> next,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var node = await next.Task.WaitAsync(cancellationToken);
                    yield return node.Barcode;
                    next = node.Next;
                }
            }
        }
    }
}
