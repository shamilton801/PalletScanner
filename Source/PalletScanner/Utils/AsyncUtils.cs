using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace PalletScanner.Utils
{
    public static class AsyncUtils
    {
        public static IAsyncEnumerable<byte> ReceiveAllAsync(this Socket sock)
        {
            return Core(sock, default);

            static async IAsyncEnumerable<byte> Core(Socket sock,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                var inputBuffer = new byte[Environment.SystemPageSize];

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var numBytesReceived = await sock.ReceiveAsync(inputBuffer, cancellationToken);
                    if (numBytesReceived == 0) break;
                    for (int i = 0; i < numBytesReceived; i++)
                        yield return inputBuffer[i];
                }
            }
        }

        public static IAsyncEnumerable<char> DecodeAsync(this Encoding encoding, IAsyncEnumerable<byte> bytes)
        {
            return Core(encoding, bytes, default);

            static async IAsyncEnumerable<char> Core(Encoding encoding, IAsyncEnumerable<byte> bytes,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                Decoder decoder = encoding.GetDecoder();
                await foreach (var b in bytes.WithCancellation(cancellationToken))
                {
                    char c = '\0';
                    int numWritten = decoder.GetChars(new ReadOnlySpan<byte>(in b), new Span<char>(ref c), false);
                    if (numWritten > 0) yield return c;
                }
            }
        }
        public static IAsyncEnumerable<byte> EncodeAsync(this Encoding encoding, IAsyncEnumerable<char> chars)
        {
            return Core(encoding, chars, default);

            static async IAsyncEnumerable<byte> Core(Encoding encoding, IAsyncEnumerable<char> chars,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                Encoder encoder = encoding.GetEncoder();
                await foreach (var c in chars.WithCancellation(cancellationToken))
                {
                    byte b = 0;
                    int numWritten = encoder.GetBytes(new ReadOnlySpan<char>(in c), new Span<byte>(ref b), false);
                    if (numWritten > 0) yield return b;
                }
            }
        }

        public static IAsyncEnumerable<string> SplitIntoStrings(this IAsyncEnumerable<char> stream, char seperator = '\0')
        {
            return Core(stream, seperator, default);

            static async IAsyncEnumerable<string> Core(IAsyncEnumerable<char> stream, char seperator,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                StringBuilder partialResult = new();
                await foreach (var c in stream.WithCancellation(cancellationToken))
                {
                    if (c == seperator)
                    {
                        yield return partialResult.ToString();
                        partialResult.Clear();
                    }
                    else
                    {
                        partialResult.Append(c);
                    }
                }
                if (!cancellationToken.IsCancellationRequested)
                    yield return partialResult.ToString();
            }
        }

        public static void WaitForCancel(this Task task)
        {
            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                if (task.IsCanceled) return;
                if (ex.IsCancellationException()) return;
                else throw;
            }
        }
        public static bool IsCancellationException(this Exception ex)
        {
            if (ex is OperationCanceledException) return true;
            if (ex is AggregateException ae)
                return ae.InnerExceptions.All(e => e.IsCancellationException());
            return false;
        }
    }
}
