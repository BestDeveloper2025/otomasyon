using System.Text;
using otomasyon.Localization;
using otomasyon.Models.Recipe;

namespace otomasyon.Simulation;

public static class ImportedCsvBatchWriter
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    public static bool TryWrite(ImportedCsvBatch batch, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(batch.SourceFilePath))
        {
            error = L.Get("Error.ImportedCsvSourceMissing");
            return false;
        }

        try
        {
            string content = string.Join(Environment.NewLine, batch.Lines) + Environment.NewLine;
            File.WriteAllText(batch.SourceFilePath, content, Utf8WithBom);
            return true;
        }
        catch (Exception ex)
        {
            error = L.F("Error.ImportedCsvSaveFailed", ex.Message);
            return false;
        }
    }
}
