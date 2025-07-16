using PalletScanner.Customers.Interface;
using PalletScanner.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletScanner.Customers.Tyson
{
    public class Tyson : ICustomer
    {
        public string Name => "Tyson";
        public IValidator CreateValidationSession() => new TysonValidator();
    }

    public class TysonBarcode(BarcodeRead raw)
    {
        public BarcodeRead Raw => raw;
        public string SCC => Raw.BarcodeContent.Substring(2, 14);
        public string PackagingIndicator => Raw.BarcodeContent.Substring(2, 1);
        public string NumberSystemCharacter => Raw.BarcodeContent.Substring(3, 2);
        public string ManufacturerId => Raw.BarcodeContent.Substring(5, 5);
        public string ItemNumber => Raw.BarcodeContent.Substring(10, 5);
        public string CheckDigit => Raw.BarcodeContent.Substring(15, 1);
        public string NetWeight => Raw.BarcodeContent.Substring(20, 6);
        public string Date => Raw.BarcodeContent.Substring(28, 6);
        public string PlantNumber => Raw.BarcodeContent.Substring(36, 3);
        public string SerialNumber => Raw.BarcodeContent.Substring(39, 7);
    }

    public class TysonValidator : AbstractValidator
    {
        private class ValidationPerPallet(string itemNumber)
        {
            public class ValidationPerItem(string serialNumber)
            {
                public readonly List<BarcodeRead> Reads = [];

                public bool AddBarcodeRead(TysonBarcode barcodeRead)
                {
                    Reads.Add(barcodeRead.Raw);
                    return false;
                }

                public IEnumerable<Status> GetStatus()
                {
                    yield return new(StatusType.Info, $"Serial Number: {serialNumber}");
                }
            }

            public readonly Dictionary<string, ValidationPerItem> ItemsBySerialNumber = [];

            public bool AddBarcodeRead(TysonBarcode barcodeRead)
            {
                bool statusChanged = false;
                bool hasCorrectNumber = ItemsBySerialNumber.Count == ExpectedNumberOfBarcodes;
                var sn = barcodeRead.SerialNumber;
                if (!ItemsBySerialNumber.TryGetValue(sn, out var itemData))
                {
                    itemData = new(sn);
                    ItemsBySerialNumber[sn] = itemData;
                    statusChanged = true;
                }
                statusChanged |= hasCorrectNumber != (ItemsBySerialNumber.Count == ExpectedNumberOfBarcodes);
                statusChanged |= itemData.AddBarcodeRead(barcodeRead);
                return statusChanged;
            }

            public IEnumerable<Status> GetStatus()
            {
                if (!TysonCsvData.ItemDescriptions.TryGetValue(itemNumber, out var type)) type = itemNumber;
                yield return new(StatusType.Info, $"Pallet of type: {type}");
                var expectedCount = ExpectedNumberOfBarcodes;
                var actualCount = ItemsBySerialNumber.Count;
                if (expectedCount != actualCount)
                {
                    string msg;
                    if (actualCount > expectedCount) msg = $"Too many barcodes ({actualCount} > {expectedCount})";
                    else if (actualCount < expectedCount) msg = $"Not enough barcodes ({actualCount} < {expectedCount})";
                    else msg = $"{itemNumber} was not found in the Tyson Validation CSV";
                    yield return new(StatusType.Error, msg);
                }
                foreach (var kv in ItemsBySerialNumber)
                    foreach (var status in kv.Value.GetStatus())
                        yield return status;
            }

            private int? ExpectedNumberOfBarcodes
            {
                get
                {
                    if (TysonCsvData.ExpectedCounts.TryGetValue(itemNumber, out int result))
                        return result;
                    else
                        return null;
                }
            }
        }

        public override IEnumerable<Status> Status
        {
            get
            {
                if (PalletsByItemNumber.Count == 0 && FailedReads.Count == 0)
                    yield return new(StatusType.Error, "Empty scan");
                //foreach (var failedRead in FailedReads)
                //    yield return new BarcodeReadStatus(StatusType.Error, failedRead, "Failed barcode read");
                foreach (var kv in PalletsByItemNumber)
                    foreach (var status in kv.Value.GetStatus())
                        yield return status;
            }
        }

        private readonly Dictionary<string, ValidationPerPallet> PalletsByItemNumber = [];
        public readonly List<BarcodeRead> FailedReads = [];

        public override void AddBarcodeRead(BarcodeRead barcodeRead)
        {
            if (IsValidTysonBarcode(barcodeRead))
            {
                bool statusChanged = PalletsByItemNumber.Count == 0 && FailedReads.Count == 0;
                TysonBarcode tysonBarcodeRead = new(barcodeRead);
                var itemNum = tysonBarcodeRead.ItemNumber;
                if (!PalletsByItemNumber.TryGetValue(itemNum, out var palletData))
                {
                    palletData = new(itemNum);
                    PalletsByItemNumber[itemNum] = palletData;
                    statusChanged = true;
                }
                statusChanged |= palletData.AddBarcodeRead(tysonBarcodeRead);
                if (statusChanged) NotfifyStatusUpdated();
            }
            else
            {
                //FailedReads.Add(barcodeRead);
                //NotfifyStatusUpdated();
            }
        }

        private static bool IsValidTysonBarcode(BarcodeRead barcodeRead)
        {
            if (barcodeRead.BarcodeContent.Length != 46) return false;
            return barcodeRead.BarcodeContent.All(char.IsAsciiDigit);
        }
    }

    internal class TysonCsvData
    {
        private class TysonCSVPalletRow
        {
            public string? ItemNum { get; set; }
            public string? UCC { get; set; }
            public string? Config { get; set; }
            public string? UOM { get; set; }
            public string? Descr { get; set; }
            public string? Repack { get; set; }
            public int ConvFactor { get; set; }
            public float Height { get; set; }
            public float Width { get; set; }
            public float Len { get; set; }
            public float MinWgt { get; set; }
            public float MaxWgt { get; set; }
            public float TareWgt { get; set; }
        }

        private static readonly Dictionary<string, int> _expectedCounts = [];
        private static readonly Dictionary<string, string> _itemDescriptions = [];
        private static bool _csvIsParsed = false;

        public static IReadOnlyDictionary<string, int> ExpectedCounts 
        { 
            get
            {
                if (!_csvIsParsed) ParseTysonCsv();
                return _expectedCounts; 
            }
        }
        public static IReadOnlyDictionary<string, string> ItemDescriptions
        {
            get
            {
                if (!_csvIsParsed) ParseTysonCsv();
                return _itemDescriptions;
            }
        }

        public static void ParseTysonCsv()
        {
            List<string> lines = [];
            using (StreamReader f = new StreamReader("tyson_ex_data.csv"))
            {
                string? header = f.ReadLine();
                string? line = f.ReadLine();
                while (line != null)
                {
                    lines.Add(line);
                    line = f.ReadLine();
                }
            }

            foreach (string line in lines)
            {
                string[] values = line.Split(',');
                TysonCSVPalletRow row = new TysonCSVPalletRow();
                row.UCC = values[1];
                row.ConvFactor = int.Parse(values[6]);
                row.UOM = values[3];

                // ************** Unused **************
                row.ItemNum = values[0];
                row.Config = values[2];
                row.Descr = values[4];
                row.Repack = values[5];
                row.Height = float.Parse(values[7]);
                row.Width = float.Parse(values[8]);
                row.Len = float.Parse(values[9]);
                row.MinWgt = float.Parse(values[10]);
                row.MaxWgt = float.Parse(values[11]);
                row.TareWgt = float.Parse(values[12]);
                // ************************************

                if (row.UOM == "PAL")
                {
                    string barcodeItemNum = new(row.UCC.Substring(8, 5));
                    _expectedCounts[barcodeItemNum] = row.ConvFactor;
                    _itemDescriptions[barcodeItemNum] = row.Descr;
                }
            }
            _csvIsParsed = true;
        }
    }
}