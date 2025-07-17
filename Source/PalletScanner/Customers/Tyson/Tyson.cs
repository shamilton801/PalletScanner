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
        // valid item number -> serial number -> status
        private readonly Dictionary<string, Dictionary<string, LeafStatus>> _itemNumberTracking = [];
        public override IEnumerable<IStatus> Status
        {
            get
            {
                foreach (var pair in _itemNumberTracking)
                {
                    int counted = pair.Value.Count;
                    int expected = TysonCsvData.ExpectedCounts[pair.Key];
                    string msg = $"{counted}/{expected} {TysonCsvData.ItemDescriptions[pair.Key]} ({pair.Key})";
                    ParentStatus status = new(counted == expected ? StatusType.Info : StatusType.Error, msg);
                    status.ChildStatus.AddRange(_itemNumberTracking[pair.Key].Select(x => x.Value));
                    yield return status;
                }
            }
        }

        public override void AddBarcodeRead(BarcodeRead barcodeRead)
        {
            var tysonBarcodeRead = AsValidTysonBarcode(barcodeRead);
            if (tysonBarcodeRead == null) return;

            bool statusChanged = false;

            var itemNum = tysonBarcodeRead.ItemNumber;
            if (!_itemNumberTracking.TryGetValue(itemNum, out var serialTracking)) 
            {
                serialTracking = [];
                _itemNumberTracking[itemNum] = serialTracking;
                statusChanged = true;
            }

            var serialNum = tysonBarcodeRead.SerialNumber;
            if (!serialTracking.TryGetValue(serialNum, out var status))
            {
                status = new LeafStatus(StatusType.Info, $"{serialNum}");
                serialTracking[serialNum] = status;
                statusChanged = true;
            }
            status.AssociatedBarcodeReads.Add(barcodeRead);

            if (statusChanged) NotfifyStatusUpdated();
        }

        private static TysonBarcode? AsValidTysonBarcode(BarcodeRead barcodeRead)
        {
            if (barcodeRead.BarcodeContent.Length != 46) return null;
            if (!barcodeRead.BarcodeContent.All(char.IsAsciiDigit)) return null;
            TysonBarcode result = new(barcodeRead);
            if (!TysonCsvData.ExpectedCounts.ContainsKey(result.ItemNumber)) return null;
            return result;
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