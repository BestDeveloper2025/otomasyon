using System.Text;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

/// <summary>
/// Makine .dat çıktısı (noktalı virgülle ayrılmış satırlar, tırnak/BOM yok).
/// </summary>
public static class DatFileExporter
{
    private const string FieldSeparator = ";";

    public static CsvFileExporter.ExportOptions CreateDefaultOptions() => CsvFileExporter.CreateDefaultOptions();

    public static bool TryWrite(SimulationJob job, string filePath, out string? error)
        => TryWrite(job, CreateDefaultOptions(), filePath, out error);

    public static bool TryWrite(
        SimulationJob job,
        CsvFileExporter.ExportOptions options,
        string filePath,
        out string? error)
    {
        if (!TryBuildLine(job, 1, options, out string? line, out error))
            return false;

        try
        {
            File.WriteAllText(filePath, line + Environment.NewLine, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static bool TryWriteBatch(
        IReadOnlyList<SimulationJob> jobs,
        CsvFileExporter.ExportOptions options,
        string filePath,
        out string? error)
    {
        var entries = new (SimulationJob Job, CsvFileExporter.ExportOptions Options)[jobs.Count];
        for (int i = 0; i < jobs.Count; i++)
            entries[i] = (jobs[i], options);
        return TryWriteBatch(entries, filePath, out error);
    }

    public static bool TryWriteBatch(
        IReadOnlyList<(SimulationJob Job, CsvFileExporter.ExportOptions Options)> entries,
        string filePath,
        out string? error)
    {
        error = null;
        if (entries.Count == 0)
        {
            error = "Reçetede kayıtlı şekil yok.";
            return false;
        }

        var lines = new List<string>(entries.Count);
        for (int i = 0; i < entries.Count; i++)
        {
            var (job, options) = entries[i];
            if (!TryBuildLine(job, i + 1, options, out string? line, out string? itemError))
            {
                error = $"Şekil {i + 1}: {itemError}";
                return false;
            }

            lines.Add(line);
        }

        try
        {
            File.WriteAllText(filePath, string.Join("\n", lines) + Environment.NewLine, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static bool TryBuildLine(
        SimulationJob job,
        int rowIndex,
        CsvFileExporter.ExportOptions options,
        out string line,
        out string? error)
    {
        line = string.Empty;
        if (!CsvFileExporter.TryBuildFields(job, rowIndex, options, out IReadOnlyList<string>? fields, out error))
            return false;

        line = string.Join(FieldSeparator, fields);
        return true;
    }
}
