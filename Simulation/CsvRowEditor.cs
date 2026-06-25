using System.Globalization;
using otomasyon.Localization;
using otomasyon.Models.Recipe;

namespace otomasyon.Simulation;

public static class CsvRowEditor
{
    private const int SlotCount = 12;
    private const int KalinlikIndex = 2;
    private const int AdetIndex = 4;
    private const int SaStart = 6;
    private const int LStart = 18;
    private const int OStart = 54;
    private const int VentSaStart = 66;
    private const int VentXStart = 78;
    private const int VentYStart = 90;
    private const int VentRStart = 102;
    private const int TotalFields = 114;

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static bool TryLoad(ImportedCsvRow row, out CsvRowEditModel model, out string? error)
    {
        model = new CsvRowEditModel();
        error = null;

        var fields = CsvLineParser.SplitFields(row.RawLine).ToList();
        if (fields.Count < 6)
        {
            error = L.Get("Error.CsvNotEnoughFields");
            return false;
        }

        model.KalinlikMm = ParseDouble(fields, KalinlikIndex);
        model.Adet = (int)Math.Max(1, ParseDouble(fields, AdetIndex));

        for (int i = 1; i <= SlotCount; i++)
        {
            double sa = ParseDouble(fields, SaStart + i - 1);
            double len = ParseDouble(fields, LStart + i - 1);
            if (Math.Abs(sa) > 1e-6 || Math.Abs(len) > 1e-6)
            {
                model.ActiveEdgeIndices.Add(i);
                model.SaByEdge[i] = sa;
                model.OffsetByEdge[i] = ParseDouble(fields, OStart + i - 1);
            }
        }

        for (int i = 1; i <= SlotCount; i++)
        {
            double ventSa = ParseDouble(fields, VentSaStart + i - 1);
            double ventX = ParseDouble(fields, VentXStart + i - 1);
            double ventY = ParseDouble(fields, VentYStart + i - 1);
            double ventR = ParseDouble(fields, VentRStart + i - 1);
            if (Math.Abs(ventSa) > 1e-6 || Math.Abs(ventX) > 1e-6 || Math.Abs(ventY) > 1e-6 || Math.Abs(ventR) > 1e-6)
            {
                model.ActiveVentIndices.Add(i);
                model.VentSaByIndex[i] = ventSa;
            }
        }

        return true;
    }

    public static string BuildLine(string originalLine, CsvRowEditModel model)
    {
        var fields = CsvLineParser.SplitFields(originalLine).ToList();
        while (fields.Count < TotalFields)
            fields.Add("0");

        fields[KalinlikIndex] = FormatKalinlik(model.KalinlikMm);
        fields[AdetIndex] = model.Adet.ToString(Inv);

        for (int i = 1; i <= SlotCount; i++)
        {
            if (model.SaByEdge.TryGetValue(i, out double sa))
                fields[SaStart + i - 1] = FormatSa(sa);
            if (model.OffsetByEdge.TryGetValue(i, out double offset))
                fields[OStart + i - 1] = FormatOffset(offset);
            if (model.VentSaByIndex.TryGetValue(i, out double ventSa))
                fields[VentSaStart + i - 1] = FormatSa(ventSa);
        }

        return string.Join(";", fields);
    }

    public static void ApplyToRow(ImportedCsvRow row, CsvRowEditModel model)
    {
        row.RawLine = BuildLine(row.RawLine, model);
        row.CamKalinlikMm = model.KalinlikMm;
        row.Adet = model.Adet;
        UpdateMetadata(row);
    }

    private static void UpdateMetadata(ImportedCsvRow row)
    {
        if (!CsvLineParser.TryParse(row.RawLine, row.SourceFileName, out ImportedCsvRow parsed, out _))
            return;

        row.RowIndex = parsed.RowIndex;
        row.ShapeCode = parsed.ShapeCode;
        row.CamKalinlikMm = parsed.CamKalinlikMm;
        row.Adet = parsed.Adet;
        row.EdgeCount = parsed.EdgeCount;
    }

    private static double ParseDouble(IReadOnlyList<string> fields, int index)
    {
        if (index < 0 || index >= fields.Count)
            return 0;

        return double.TryParse(fields[index], NumberStyles.Float, Inv, out double value) ? value : 0;
    }

    private static string FormatKalinlik(double v)
    {
        if (Math.Abs(v - Math.Round(v)) < 1e-6)
            return ((int)Math.Round(v)).ToString(Inv);
        return v.ToString("0.0", Inv);
    }

    private static string FormatSa(double v)
    {
        if (Math.Abs(v) < 1e-9)
            return "0.0";
        if (Math.Abs(v - Math.Round(v)) < 1e-6)
            return ((int)Math.Round(v)).ToString(Inv);
        return v.ToString("0.0", Inv);
    }

    private static string FormatOffset(double v)
    {
        if (Math.Abs(v) < 1e-9)
            return "0.00";
        return v.ToString("0.00", Inv);
    }
}
