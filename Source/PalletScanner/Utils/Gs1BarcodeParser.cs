using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;

namespace GS1BarcodeParser
{
    /// <summary>
    /// Represents a parsed Application Identifier and its data
    /// </summary>
    public class ParsedAi
    {
        public string Ai { get; set; }
        public string RawData { get; set; }
        public string FormattedData { get; set; }
        public string Description { get; set; }
        public string DataTitle { get; set; }
        public string Unit { get; set; }
        public int DecimalPlaces { get; set; }
        public bool IsValid { get; set; }
        public string ValidationError { get; set; }
    }

    /// <summary>
    /// Represents the result of parsing a complete barcode
    /// </summary>
    public class BarcodeParseResult
    {
        public bool IsValid { get; set; }
        public List<ParsedAi> ParsedAis { get; set; } = new List<ParsedAi>();
        public List<string> Errors { get; set; } = new List<string>();
        public string RawBarcode { get; set; }
    }

    /// <summary>
    /// Represents an Ai definition from the XML schema
    /// </summary>
    internal class AiDefinition
    {
        public string Ai { get; set; }
        public string Description { get; set; }
        public string DataTitle { get; set; }
        public string Format { get; set; }
        public string DataType { get; set; }
        public bool FixedLength { get; set; }
        public int? TotalLength { get; set; }
        public int AiLength { get; set; }
        public int? DataLength { get; set; }
        public int? MinDataLength { get; set; }
        public int? MaxDataLength { get; set; }
        public bool FNC1Required { get; set; }
        public string Usage { get; set; }
        public string Unit { get; set; }
        public string CharacterSet { get; set; }
        public bool CanStartBarcode { get; set; }
        public string DateFormat { get; set; }
        public bool IsPattern { get; set; }
        public string PatternRegex { get; set; }
        public bool HasDecimalIndicator { get; set; }
        public int[] ValidDecimalPositions { get; set; }
    }

    /// <summary>
    /// GS1 Barcode Parser using XML schema definitions
    /// </summary>
    public class GS1BarcodeParser
    {
        private readonly Dictionary<string, AiDefinition> _aiDefinitions = new Dictionary<string, AiDefinition>();
        private readonly List<AiDefinition> _patternDefinitions = new List<AiDefinition>();
        private readonly HashSet<string> _mandatoryStartAis = new HashSet<string>();
        private const char FNC1 = '\u001D'; // ASCII Group Separator (GS)
        private const string FNC1_STRING = "\u001D";

        // Character set definitions
        private readonly Regex _gs1Ai82Regex = new Regex(@"^[A-Za-z0-9!""#%&'()*+,\-\./:;<=>?@\[\]^_`{|}~\s]*$");
        private readonly Regex _gs1Ai39Regex = new Regex(@"^[A-Z0-9\-\.]*$");
        private readonly Regex _gs1Ai64Regex = new Regex(@"^[A-Za-z0-9\-_]*$");

        public GS1BarcodeParser(string xmlSchemaPath)
        {
            LoadSchema(xmlSchemaPath);
        }

        public GS1BarcodeParser(Stream xmlSchemaStream)
        {
            LoadSchema(xmlSchemaStream);
        }

        private void LoadSchema(string xmlPath)
        {
            using (var stream = File.OpenRead(xmlPath))
            {
                LoadSchema(stream);
            }
        }

        private void LoadSchema(Stream xmlStream)
        {
            var doc = XDocument.Load(xmlStream);
            var root = doc.Root;

            // Load parsing rules
            var mandatoryStartRule = root.Descendants("Rule")
                .FirstOrDefault(r => r.Element("RuleID")?.Value == "MANDATORY_START");

            if (mandatoryStartRule != null)
            {
                foreach (var ai in mandatoryStartRule.Descendants("Ai"))
                {
                    _mandatoryStartAis.Add(ai.Value);
                }
            }

            // Load direct Ai definitions
            foreach (var aiElement in root.Descendants("ApplicationIdentifier"))
            {
                var def = ParseAiDefinition(aiElement);
                if (def != null)
                {
                    _aiDefinitions[def.Ai] = def;
                }
            }

            // Load pattern Ai definitions
            foreach (var patternElement in root.Descendants("ApplicationIdentifierPattern"))
            {
                var def = ParsePatternDefinition(patternElement);
                if (def != null)
                {
                    _patternDefinitions.Add(def);
                }
            }
        }

        private AiDefinition ParseAiDefinition(XElement element)
        {
            var def = new AiDefinition
            {
                Ai = element.Element("Ai")?.Value,
                Description = element.Element("Description")?.Value,
                DataTitle = element.Element("DataTitle")?.Value,
                Format = element.Element("Format")?.Value,
                DataType = element.Element("DataType")?.Value,
                FixedLength = bool.Parse(element.Element("FixedLength")?.Value ?? "false"),
                AiLength = int.Parse(element.Element("AiLength")?.Value ?? "0"),
                FNC1Required = bool.Parse(element.Element("FNC1Required")?.Value ?? "false"),
                Usage = element.Element("Usage")?.Value,
                Unit = element.Element("Unit")?.Value,
                CharacterSet = element.Element("CharacterSet")?.Value,
                CanStartBarcode = bool.Parse(element.Element("CanStartBarcode")?.Value ?? "false"),
                DateFormat = element.Element("DateFormat")?.Value,
                IsPattern = false
            };

            if (element.Element("TotalLength") != null)
                def.TotalLength = int.Parse(element.Element("TotalLength").Value);
            if (element.Element("DataLength") != null)
                def.DataLength = int.Parse(element.Element("DataLength").Value);
            if (element.Element("MinDataLength") != null)
                def.MinDataLength = int.Parse(element.Element("MinDataLength").Value);
            if (element.Element("MaxDataLength") != null)
                def.MaxDataLength = int.Parse(element.Element("MaxDataLength").Value);

            return def;
        }

        private AiDefinition ParsePatternDefinition(XElement element)
        {
            var pattern = element.Element("AiPattern")?.Value;
            if (string.IsNullOrEmpty(pattern)) return null;

            var def = ParseAiDefinition(element);
            def.IsPattern = true;
            def.Ai = pattern;

            // Convert pattern to regex
            def.PatternRegex = pattern.Replace("[", "").Replace("]", "");

            // Check for decimal indicator
            if (element.Element("DecimalIndicator")?.Value == "LastDigitOfAi")
            {
                def.HasDecimalIndicator = true;
                var validPositions = element.Element("ValidDecimalPositions")?.Value;
                if (!string.IsNullOrEmpty(validPositions))
                {
                    def.ValidDecimalPositions = validPositions.Split(',')
                        .Select(p => int.Parse(p.Trim()))
                        .ToArray();
                }
            }

            return def;
        }

        /// <summary>
        /// Parse a complete GS1 barcode
        /// </summary>
        public BarcodeParseResult ParseBarcode(string barcode)
        {
            var result = new BarcodeParseResult
            {
                RawBarcode = barcode,
                IsValid = true
            };

            if (string.IsNullOrEmpty(barcode))
            {
                result.IsValid = false;
                result.Errors.Add("Barcode is empty");
                return result;
            }

            // Check mandatory start Ai
            bool hasValidStart = false;
            foreach (var startAi in _mandatoryStartAis)
            {
                if (barcode.StartsWith(startAi))
                {
                    hasValidStart = true;
                    break;
                }
            }

            if (!hasValidStart)
            {
                result.IsValid = false;
                result.Errors.Add($"Barcode must start with one of these Ais: {string.Join(", ", _mandatoryStartAis)}");
            }

            // Track used Ais to enforce no repetition rule
            var usedAis = new HashSet<string>();

            int position = 0;
            while (position < barcode.Length)
            {
                var aiResult = ExtractNextAi(barcode, position);

                if (aiResult == null)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Unable to parse Ai at position {position}");
                    break;
                }

                // Check for Ai repetition
                if (usedAis.Contains(aiResult.Ai))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Ai {aiResult.Ai} appears more than once in the barcode");
                    aiResult.IsValid = false;
                    aiResult.ValidationError = "Duplicate Ai";
                }
                else
                {
                    usedAis.Add(aiResult.Ai);
                }

                result.ParsedAis.Add(aiResult);
                position = aiResult.NextPosition;
            }

            return result;
        }

        /// <summary>
        /// Extract the next Ai and its data from the barcode
        /// </summary>
        private class AiExtractionResult : ParsedAi
        {
            public int NextPosition { get; set; }
        }

        private AiExtractionResult ExtractNextAi(string barcode, int startPosition)
        {
            if (startPosition >= barcode.Length) return null;

            // Determine Ai length
            int aiLength = DetermineAiLength(barcode, startPosition);
            if (aiLength == 0 || startPosition + aiLength > barcode.Length) return null;

            string ai = barcode.Substring(startPosition, aiLength);
            var definition = GetAiDefinition(ai);

            if (definition == null)
            {
                return new AiExtractionResult
                {
                    Ai = ai,
                    IsValid = false,
                    ValidationError = $"Unknown Ai: {ai}",
                    NextPosition = startPosition + aiLength
                };
            }

            var result = new AiExtractionResult
            {
                Ai = ai,
                Description = definition.Description,
                DataTitle = definition.DataTitle,
                Unit = definition.Unit,
                IsValid = true
            };

            // Extract data
            int dataStart = startPosition + aiLength;
            string data;
            int nextPosition;

            if (definition.FixedLength)
            {
                int dataLength = definition.DataLength ?? 0;
                if (dataStart + dataLength > barcode.Length)
                {
                    result.IsValid = false;
                    result.ValidationError = "Insufficient data for fixed-length Ai";
                    result.NextPosition = barcode.Length;
                    return result;
                }

                data = barcode.Substring(dataStart, dataLength);
                nextPosition = dataStart + dataLength;
            }
            else
            {
                // Variable length - look for FNC1 or end of string
                int fnc1Pos = barcode.IndexOf(FNC1, dataStart);
                if (fnc1Pos == -1)
                {
                    // No FNC1 found - must be last Ai
                    data = barcode.Substring(dataStart);
                    nextPosition = barcode.Length;
                }
                else
                {
                    data = barcode.Substring(dataStart, fnc1Pos - dataStart);
                    nextPosition = fnc1Pos + 1; // Skip FNC1
                }

                // Validate length
                if (definition.MinDataLength.HasValue && data.Length < definition.MinDataLength.Value)
                {
                    result.IsValid = false;
                    result.ValidationError = $"Data too short. Minimum length: {definition.MinDataLength}";
                }
                if (definition.MaxDataLength.HasValue && data.Length > definition.MaxDataLength.Value)
                {
                    result.IsValid = false;
                    result.ValidationError = $"Data too long. Maximum length: {definition.MaxDataLength}";
                }
            }

            result.RawData = data;
            result.NextPosition = nextPosition;

            // Validate and format data
            ValidateAndFormatData(result, definition);

            return result;
        }

        private int DetermineAiLength(string barcode, int position)
        {
            if (position + 2 > barcode.Length) return 0;

            string first2 = barcode.Substring(position, 2);
            int firstTwo = int.Parse(first2);

            // Check 4-digit Ais first
            if (position + 4 <= barcode.Length)
            {
                if (firstTwo >= 70 && firstTwo <= 79) return 4;
                if (firstTwo >= 80 && firstTwo <= 89) return 4;

                // Check specific 4-digit patterns
                string first4 = barcode.Substring(position, 4);
                if (_aiDefinitions.ContainsKey(first4)) return 4;

                // Check pattern definitions
                foreach (var pattern in _patternDefinitions)
                {
                    if (Regex.IsMatch(first4, pattern.PatternRegex))
                        return 4;
                }
            }

            // Check 3-digit Ais
            if (position + 3 <= barcode.Length)
            {
                if (firstTwo >= 23 && firstTwo <= 23) return 3; // Special case for 23x
                if (firstTwo >= 40 && firstTwo <= 49) return 3;

                string first3 = barcode.Substring(position, 3);
                if (_aiDefinitions.ContainsKey(first3)) return 3;
            }

            // Default to 2-digit
            return 2;
        }

        private AiDefinition GetAiDefinition(string ai)
        {
            // Check direct definitions first
            if (_aiDefinitions.ContainsKey(ai))
                return _aiDefinitions[ai];

            // Check patterns
            foreach (var pattern in _patternDefinitions)
            {
                if (Regex.IsMatch(ai, pattern.PatternRegex))
                {
                    // Create a specific instance for this Ai
                    var specific = new AiDefinition
                    {
                        Ai = ai,
                        Description = pattern.Description,
                        DataTitle = pattern.DataTitle,
                        Format = pattern.Format,
                        DataType = pattern.DataType,
                        FixedLength = pattern.FixedLength,
                        TotalLength = pattern.TotalLength,
                        AiLength = pattern.AiLength,
                        DataLength = pattern.DataLength,
                        MinDataLength = pattern.MinDataLength,
                        MaxDataLength = pattern.MaxDataLength,
                        FNC1Required = pattern.FNC1Required,
                        Usage = pattern.Usage,
                        Unit = pattern.Unit,
                        CharacterSet = pattern.CharacterSet,
                        HasDecimalIndicator = pattern.HasDecimalIndicator,
                        ValidDecimalPositions = pattern.ValidDecimalPositions
                    };

                    // Set decimal places if applicable
                    if (pattern.HasDecimalIndicator && ai.Length > 0)
                    {
                        int lastDigit = int.Parse(ai.Substring(ai.Length - 1, 1));
                        if (pattern.ValidDecimalPositions == null ||
                            pattern.ValidDecimalPositions.Contains(lastDigit))
                        {
                            specific.Ai = ai;
                            return specific;
                        }
                        else
                        {
                            return null; // Invalid decimal position
                        }
                    }

                    return specific;
                }
            }

            return null;
        }

        private void ValidateAndFormatData(ParsedAi result, AiDefinition definition)
        {
            // Validate data type
            if (definition.DataType == "Numeric")
            {
                if (!Regex.IsMatch(result.RawData, @"^\d+$"))
                {
                    result.IsValid = false;
                    result.ValidationError = "Data must be numeric";
                    return;
                }
            }

            // Validate character set
            if (!string.IsNullOrEmpty(definition.CharacterSet))
            {
                bool charSetValid = definition.CharacterSet switch
                {
                    "GS1Ai82" => _gs1Ai82Regex.IsMatch(result.RawData),
                    "GS1Ai39" => _gs1Ai39Regex.IsMatch(result.RawData),
                    "GS1Ai64" => _gs1Ai64Regex.IsMatch(result.RawData),
                    _ => true
                };

                if (!charSetValid)
                {
                    result.IsValid = false;
                    result.ValidationError = $"Invalid characters for character set {definition.CharacterSet}";
                    return;
                }
            }

            // Format data based on type
            result.FormattedData = result.RawData;

            // Handle decimal places for variable measure Ais
            if (definition.HasDecimalIndicator && result.Ai.Length > 0)
            {
                int decimalPlaces = int.Parse(result.Ai.Substring(result.Ai.Length - 1, 1));
                result.DecimalPlaces = decimalPlaces;

                if (decimalPlaces > 0 && result.RawData.Length > 0)
                {
                    try
                    {
                        decimal value = decimal.Parse(result.RawData);
                        decimal divisor = (decimal)Math.Pow(10, decimalPlaces);
                        decimal formattedValue = value / divisor;
                        result.FormattedData = formattedValue.ToString($"F{decimalPlaces}");
                    }
                    catch
                    {
                        // Keep raw data if formatting fails
                    }
                }
            }

            // Format dates
            if (!string.IsNullOrEmpty(definition.DateFormat))
            {
                try
                {
                    result.FormattedData = FormatDate(result.RawData, definition.DateFormat);
                }
                catch
                {
                    // Keep raw data if date formatting fails
                }
            }
        }

        private string FormatDate(string rawDate, string format)
        {
            switch (format)
            {
                case "YYMMDD":
                    if (rawDate.Length == 6)
                    {
                        string yy = rawDate.Substring(0, 2);
                        string mm = rawDate.Substring(2, 2);
                        string dd = rawDate.Substring(4, 2);

                        // Handle century
                        int year = int.Parse(yy);
                        year += (year >= 50) ? 1900 : 2000;

                        if (dd == "00")
                            return $"{year}-{mm}";
                        else
                            return $"{year}-{mm}-{dd}";
                    }
                    break;

                case "YYYYMMDD":
                    if (rawDate.Length == 8)
                    {
                        return $"{rawDate.Substring(0, 4)}-{rawDate.Substring(4, 2)}-{rawDate.Substring(6, 2)}";
                    }
                    break;

                case "YYMMDDHHMM":
                    if (rawDate.Length == 10)
                    {
                        string date = FormatDate(rawDate.Substring(0, 6), "YYMMDD");
                        string time = $"{rawDate.Substring(6, 2)}:{rawDate.Substring(8, 2)}";
                        return $"{date} {time}";
                    }
                    break;

                case "YYYYMMDDHHMM":
                    if (rawDate.Length == 12)
                    {
                        string date = FormatDate(rawDate.Substring(0, 8), "YYYYMMDD");
                        string time = $"{rawDate.Substring(8, 2)}:{rawDate.Substring(10, 2)}";
                        return $"{date} {time}";
                    }
                    break;
            }

            return rawDate;
        }

        /// <summary>
        /// Get human-readable description of the parsed barcode
        /// </summary>
        public string GetBarcodeDescription(BarcodeParseResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Barcode: {result.RawBarcode}");
            sb.AppendLine($"Valid: {result.IsValid}");

            if (result.Errors.Any())
            {
                sb.AppendLine("Errors:");
                foreach (var error in result.Errors)
                {
                    sb.AppendLine($"  - {error}");
                }
            }

            sb.AppendLine("\nParsed Data:");
            foreach (var ai in result.ParsedAis)
            {
                sb.AppendLine($"  Ai {ai.Ai}: {ai.Description}");
                sb.AppendLine($"    Data: {ai.FormattedData}");
                if (!string.IsNullOrEmpty(ai.Unit))
                {
                    sb.AppendLine($"    Unit: {ai.Unit}");
                }
                if (!ai.IsValid)
                {
                    sb.AppendLine($"    Error: {ai.ValidationError}");
                }
            }

            return sb.ToString();
        }
    }
    }
//    /// <summary>
//    /// Example usage
//    /// </summary>
//    public class Example
//    {
//        public static void Main()
//        {
//            // Load parser with XML schema
//            var parser = new GS1BarcodeParser("gs1-schema.xml");

//            // Example 1: Fixed-length Ais
//            string barcode1 = "01123456789012341721123110ABC123";
//            var result1 = parser.ParseBarcode(barcode1);
//            Console.WriteLine(parser.GetBarcodeDescription(result1));

//            // Example 2: Variable-length with FNC1
//            string barcode2 = "01123456789012341721123110ABC123" + (char)29 + "21SERIAL123";
//            var result2 = parser.ParseBarcode(barcode2);
//            Console.WriteLine(parser.GetBarcodeDescription(result2));

//            // Example 3: Variable measure with decimal
//            string barcode3 = "01123456789012343202001234"; // 3202 = weight in pounds with 2 decimals = 12.34 lbs
//            var result3 = parser.ParseBarcode(barcode3);
//            Console.WriteLine(parser.GetBarcodeDescription(result3));
//        }
//    }
//}

