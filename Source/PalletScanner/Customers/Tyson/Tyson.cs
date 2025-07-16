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
        private class ValidationData
        {
            public int Count = 0;
            public List<string> SerialNums = [];
        }

        public override IEnumerable<Status> Status => _status;
        private readonly List<Status> _status = [];
        
        private readonly Dictionary<string, ValidationData> _currentScanData = [];

        public override void AddBarcodeRead(BarcodeRead barcodeRead)
        {
            TysonBarcode b = new(barcodeRead);
            //Status s = new();
            //s.AssociatedBarcodeRead = barcodeRead;
            
            if (!TysonCsvData.ExpectedCounts.ContainsKey(b.ItemNumber))
            {
                //s.Type = StatusType.Error;
                //s.Message = $"{b.ItemNumber} was not found in the Tyson Validation CSV";
            } else
            {
                if (!_currentScanData.ContainsKey(b.ItemNumber)) _currentScanData[b.ItemNumber] = new();

                _currentScanData[b.ItemNumber].Count++;
                _currentScanData[b.ItemNumber].SerialNums.Add(b.SerialNumber);

                bool rightAmount = _currentScanData[b.ItemNumber].Count == TysonCsvData.ExpectedCounts[b.ItemNumber];

                //s.Type = rightAmount ? StatusType.Error : StatusType.Info;
                //s.Message = $"{_currentScanData[b.ItemNumber]}/{TysonCsvData.ExpectedCounts[b.ItemNumber]} {TysonCsvData.ItemDescriptions[b.ItemNumber]} ({b.ItemNumber})";
            }

            //_status.Add(s);
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