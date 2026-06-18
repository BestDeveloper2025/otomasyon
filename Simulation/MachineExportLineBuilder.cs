using System.Globalization;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

internal static class MachineExportLineBuilder
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static bool TryBuildCombinedLines(
        IReadOnlyList<string>? prefixLines,
        IReadOnlyList<(SimulationJob Job, CsvFileExporter.ExportOptions Options)> entries,
        bool csvFormat,
        out IReadOnlyList<string> lines,
        out string? error)
    {
        lines = Array.Empty<string>();
        error = null;

        int prefixCount = prefixLines?.Count ?? 0;
        if (prefixCount == 0 && entries.Count == 0)
        {
            error = "Kaydedilecek veri yok.";
            return false;
        }

        var result = new List<string>(prefixCount + entries.Count);
        if (prefixLines is not null)
            result.AddRange(prefixLines);

        int startRow = GetNextRowIndex(prefixLines);
        for (int i = 0; i < entries.Count; i++)
        {
            var (job, options) = entries[i];
            int rowIndex = startRow + i;

            if (csvFormat)
            {
                if (!CsvFileExporter.TryBuildLine(job, rowIndex, options, out string? line, out string? itemError))
                {
                    error = $"Yeni şekil {i + 1}: {itemError}";
                    return false;
                }

                result.Add(line);
            }
            else
            {
                if (!DatFileExporter.TryBuildLine(job, rowIndex, options, out string? line, out string? itemError))
                {
                    error = $"Yeni şekil {i + 1}: {itemError}";
                    return false;
                }

                result.Add(line);
            }
        }

        for (int i = 0; i < result.Count; i++)
            result[i] = CsvLineParser.ReplaceFirstField(result[i], i + 1);

        lines = result;
        return true;
    }

    public static int GetNextRowIndex(IReadOnlyList<string>? prefixLines)
    {
        if (prefixLines is null || prefixLines.Count == 0)
            return 1;

        int max = 0;
        foreach (string line in prefixLines)
        {
            if (!TryParseRowIndex(line, out int rowIndex))
                continue;

            if (rowIndex > max)
                max = rowIndex;
        }

        return max > 0 ? max + 1 : prefixLines.Count + 1;
    }

    private static bool TryParseRowIndex(string line, out int rowIndex)
    {
        rowIndex = 0;
        if (string.IsNullOrWhiteSpace(line))
            return false;

        ReadOnlySpan<char> span = line.AsSpan().Trim();
        if (span.Length == 0)
            return false;

        if (span[0] == '"')
        {
            int end = span.Slice(1).IndexOf('"');
            if (end < 0)
                return false;
            span = span.Slice(1, end);
        }
        else
        {
            int sep = span.IndexOf(';');
            if (sep >= 0)
                span = span[..sep];
        }

        return int.TryParse(span.Trim(), NumberStyles.Integer, Inv, out rowIndex);
    }
}
