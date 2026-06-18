using System.Text;
using otomasyon.Models.Recipe;

namespace otomasyon.Simulation;

public static class CsvFileImporter
{
    public static bool TryImport(string filePath, out ImportedCsvBatch batch, out string? error)
    {
        batch = null!;
        error = null;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            error = "Dosya yolu boş.";
            return false;
        }

        if (!File.Exists(filePath))
        {
            error = "Dosya bulunamadı.";
            return false;
        }

        try
        {
            string text = File.ReadAllText(filePath, Encoding.UTF8);
            string sourceName = Path.GetFileName(filePath);
            var batchBuilder = new ImportedCsvBatch { SourceFilePath = filePath };
            int lineNo = 0;

            foreach (string raw in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                lineNo++;
                string line = raw.Trim();
                if (line.Length == 0)
                    continue;

                if (!line.Contains(';'))
                {
                    error = $"Satır {lineNo}: alan ayırıcı (;) bulunamadı.";
                    return false;
                }

                if (!CsvLineParser.TryParse(line, sourceName, out ImportedCsvRow row, out string? rowError))
                {
                    error = $"Satır {lineNo}: {rowError}";
                    return false;
                }

                batchBuilder.Rows.Add(row);
            }

            if (batchBuilder.Rows.Count == 0)
            {
                error = "CSV dosyasında veri satırı yok.";
                return false;
            }

            batch = batchBuilder;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static ImportedCsvBatch CreateBatchFromLines(string filePath, IReadOnlyList<string> lines)
    {
        var batch = new ImportedCsvBatch { SourceFilePath = filePath };
        string sourceName = Path.GetFileName(filePath);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (CsvLineParser.TryParse(line.Trim(), sourceName, out ImportedCsvRow row, out _))
                batch.Rows.Add(row);
        }

        return batch;
    }
}
