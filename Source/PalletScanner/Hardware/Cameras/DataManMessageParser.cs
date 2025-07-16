using PalletScanner.Data;
using System.Buffers.Text;
using System.Buffers;
using System.Drawing;
using System.Text;

namespace PalletScanner.Hardware.Cameras
{
    internal static class DataManMessageParser
    {
        private static IEnumerable<BarcodeRead> ParseMessageLegacy(ICamera cam, string[][] messageData)
        {
            if (messageData.Length < 2 || messageData[0].Length < 2) yield break;
            var cameraDetails = messageData[0];
            if (cameraDetails[0] != cam.Name) yield break;

            var isTimeInt = int.TryParse(cameraDetails[1], out var triggerTime);

            foreach (var barcodeData in messageData)
            {
                if (barcodeData.Length < 6) continue;

                var isXInt = int.TryParse(barcodeData[0], out var xPoint);
                var isYInt = int.TryParse(barcodeData[1], out var yPoint);
                var isAngleInt = int.TryParse(barcodeData[2], out var anglePoint);
                var isPpmFloat = float.TryParse(barcodeData[4], out var ppm);

                if (!isXInt || !isYInt || !isAngleInt || !isTimeInt || !isPpmFloat) continue;

                yield return new(
                    cam,
                    TimeSpan.FromMilliseconds(triggerTime),
                    barcodeData[5],
                    barcodeData[3],
                    new PointF(xPoint, yPoint),
                    anglePoint,
                    ppm);
            }
        }
        private static IEnumerable<BarcodeRead> ParseMessageVersioned(ICamera cam, string[] header, IEnumerable<string[]> body, string version)
        {
            switch (version)
            {
                case "2.0":
                    if (header.Length != 2) break;
                    if (cam.Name != header[0]) break;
                    if (!int.TryParse(header[1], out var triggerTime)) break;
                    foreach (var line in body)
                    {
                        if (line.Length != 7) continue;
                        if (!int.TryParse(line[0], out var centerCoordX)) continue;
                        if (!int.TryParse(line[1], out var centerCoordY)) continue;
                        if (!float.TryParse(line[2], out var angle)) continue;
                        var symbologyType = line[3];
                        if (!float.TryParse(line[4], out var ppm)) continue;
                        var corners = ParsePointList(DecodeBase64(line[5]));
                        var content = DecodeBase64(line[6]);
                        var centerCoord = new PointF(centerCoordX, centerCoordY);
                        if (content == null || corners == null) continue;

                        yield return new(
                            cam,
                            TimeSpan.FromMilliseconds(triggerTime),
                            content,
                            symbologyType,
                            centerCoord,
                            angle,
                            ppm,
                            corners);
                    }
                    break;
            }
        }

        private static string? DecodeBase64(string? v)
        {
            if (v == null) return null;
            var bytes = new byte[v.Length];
            return Base64.DecodeFromUtf8(Encoding.UTF8.GetBytes(v), bytes, out _, out var bytesWritten) switch
            {
                OperationStatus.Done => Encoding.UTF8.GetString(bytes, 0, bytesWritten),
                _ => null,
            };
        }
        private static PointF[]? ParsePointList(string? v)
        {
            if (v == null) return null;
            var data = ParseLines(v);
            var result = new PointF[data.Length];
            for (int i = 0; i < data.Length; ++i)
            {
                if (data[i].Length != 2) return null;
                if (!int.TryParse(data[i][0], out var x)) return null;
                if (!int.TryParse(data[i][1], out var y)) return null;
                result[i] = new PointF(x, y);
            }
            return result;
        }
        private static string[][] ParseLines(string message)
            => [.. message.Split('\n').Select(line => line.Split(","))];

        public static IEnumerable<BarcodeRead> ParseMessage(ICamera cam, string message)
        {
            const string magicString = "GateKeeper Reader Format ";
            var messageData = ParseLines(message);
            if (messageData.Length == 0 || messageData[0].Length == 0) return [];
            if (messageData[0].Length != 1) return ParseMessageLegacy(cam, messageData);
            var firstLine = messageData[0][0];
            if (!firstLine.StartsWith(magicString)) return [];
            if (messageData.Length < 2 || messageData[1].Length != 2) return [];
            return ParseMessageVersioned(cam, messageData[1], messageData.Skip(2), firstLine[magicString.Length..]);
        }
    }
}
