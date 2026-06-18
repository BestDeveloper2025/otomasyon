using System.Globalization;
using otomasyon.Localization;
using otomasyon.Models.Recipe;

namespace otomasyon.Simulation;

public static class CsvLineParser
{
    private const int SlotCount = 12;
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static bool TryParse(string line, string sourceFileName, out ImportedCsvRow row, out string? error)
    {
        row = null!;
        error = null;

        if (string.IsNullOrWhiteSpace(line))
        {
            error = L.Get("Error.CsvEmptyLine");
            return false;
        }

        IReadOnlyList<string> fields = SplitFields(line);
        if (fields.Count < 6)
        {
            error = L.Get("Error.CsvNotEnoughFields");
            return false;
        }

        if (!int.TryParse(fields[0], NumberStyles.Integer, Inv, out int rowIndex))
        {
            error = L.Get("Error.CsvRowIndex");
            return false;
        }

        _ = int.TryParse(fields[1], NumberStyles.Integer, Inv, out int shapeCode);
        _ = double.TryParse(fields[2], NumberStyles.Float, Inv, out double camKalinlik);
        _ = int.TryParse(fields[4], NumberStyles.Integer, Inv, out int adet);

        int edgeCount = CountActiveEdges(fields);

        row = new ImportedCsvRow
        {
            RawLine = line.Trim(),
            RowIndex = rowIndex,
            ShapeCode = shapeCode,
            CamKalinlikMm = camKalinlik,
            Adet = adet > 0 ? adet : 1,
            EdgeCount = edgeCount,
            SourceFileName = sourceFileName
        };
        return true;
    }

    private static int CountActiveEdges(IReadOnlyList<string> fields)
    {
        int saStart = 6;
        int lStart = saStart + SlotCount;
        int count = 0;

        for (int i = 0; i < SlotCount; i++)
        {
            bool hasSa = i + saStart < fields.Count &&
                         TryParseDouble(fields[i + saStart], out double sa) &&
                         Math.Abs(sa) > 1e-6;
            bool hasL = i + lStart < fields.Count &&
                        TryParseDouble(fields[i + lStart], out double len) &&
                        Math.Abs(len) > 1e-6;

            if (hasSa || hasL)
                count++;
        }

        return count;
    }

    private static bool TryParseDouble(string value, out double result)
        => double.TryParse(value, NumberStyles.Float, Inv, out result);

    public static IReadOnlyList<string> SplitFields(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (c == ';' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        fields.Add(current.ToString());
        return fields;
    }

    public static string ReplaceFirstField(string line, int newRowIndex)
    {
        var fields = SplitFields(line).ToList();
        if (fields.Count == 0)
            return line;

        fields[0] = newRowIndex.ToString(Inv);
        return string.Join(";", fields);
    }
}
