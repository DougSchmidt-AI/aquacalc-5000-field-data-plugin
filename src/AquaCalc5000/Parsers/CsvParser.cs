﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AquaCalc5000.Parsers
{
    public class CsvParser
    {
        private readonly List<CsvLine> _csvLines;

        private readonly char _delimiterChar;

        public CsvParser(string csvText, char delimiterChar = ',')
        {
            _delimiterChar = delimiterChar;
            _csvLines = ParseToCsvLines(csvText);
        }

        private List<CsvLine> ParseToCsvLines(string csvText)
        {
            var csvLines = new List<CsvLine>();

            if (csvText == null)
            {
                return csvLines;
            }

            using (var reader = new StringReader(csvText))
            {
                string line;
                var lineNumber = 1;
                while ((line = reader.ReadLine()) != null)
                {
                    csvLines.Add(new CsvLine
                    {
                        LineNumber = lineNumber++,
                        OriginalLine = line,
                        Parts = SplitToParts(line)
                    });
                }
            }

            return csvLines;
        }

        private List<string> SplitToParts(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return new List<string>();
            }

            return line.Split(_delimiterChar).Select(s => s.Trim()).ToList();
        }

        public CsvLine GetFirstNonEmptyLineOrNull()
        {
            return _csvLines.FirstOrDefault(l =>
                !string.IsNullOrWhiteSpace(l.OriginalLine));
        }

        public string GetRequiredStringByLabel(string label)
        {
            var foundLine = GetRequiredLineByLabel(label);

            if (foundLine == null)
            {
                throw new ArgumentException($"No line starting with '{label}' is found.", nameof(label));
            }

            var parts = foundLine.Parts;
            if (parts.Count < 2)
            {
                throw new ArgumentException($"Required value is not found for '{label}'");
            }

            var combinedString = string.Join(_delimiterChar.ToString(),
                parts.Skip(1).Where(p => !string.IsNullOrWhiteSpace(p)));

            if (string.IsNullOrWhiteSpace(combinedString))
            {
                throw new ArgumentException($"Required value is empty for '{label}'");
            }

            return combinedString;
        }

        public CsvLine GetLineByLabelOrNull(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentNullException(nameof(label));
            }

            return _csvLines.FirstOrDefault(l =>
                l.Parts.Any() &&
                string.Equals(l.Parts.First(), label, StringComparison.InvariantCultureIgnoreCase));
        }

        private CsvLine GetRequiredLineByLabel(string label)
        {
            var foundLine = GetLineByLabelOrNull(label);

            if (foundLine == null)
            {
                throw new ArgumentException($"No line starting with '{label}' is found.", nameof(label));
            }

            return foundLine;
        }

        public CsvLine GetFirstLineByFilterOrNull(Func<CsvLine, bool> filterFunc)
        {
            return _csvLines.FirstOrDefault(filterFunc);
        }

        public int GetRequiredIntByLabel(string label)
        {
            var parts = GetRequiredValueLine(label).Parts;

            return int.Parse(parts[1]);
        }

        private CsvLine GetRequiredValueLine(string label)
        {
            var foundLine = GetRequiredLineByLabel(label);

            if (foundLine.Parts.Count < 2)
            {
                throw new ArgumentException($"Required value is not found for '{label}'");
            }

            return foundLine;
        }

        public List<CsvLine> GetAllLinesByFilter(Func<CsvLine, bool> filterFunc)
        {
            return _csvLines.Where(filterFunc).ToList();
        }

        public int GetLastLineNum()
        {
            return _csvLines.Any() ? _csvLines.Last().LineNumber : 0;
        }

        public double GetRequiredDoubleByLabel(string label)
        {
            var parts = GetRequiredValueLine(label).Parts;

            return double.Parse(parts[1]);
        }

        public bool GetRequiredBooleanByLabelOrDefault(string label)
        {
            var parts = GetRequiredValueLine(label).Parts;

            if (bool.TryParse(parts[1], out bool bValue))
            {
                return bValue;
            }

            return true;
        }
    }
}
