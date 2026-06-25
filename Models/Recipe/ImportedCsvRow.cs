using otomasyon.Localization;

namespace otomasyon.Models.Recipe;

/// <summary>İçe aktarılmış CSV'deki tek şekil satırı.</summary>
public sealed class ImportedCsvRow
{
    public string RawLine { get; set; } = string.Empty;
    public int RowIndex { get; set; }
    public int ShapeCode { get; set; }
    public double CamKalinlikMm { get; set; }
    public int Adet { get; set; }
    public int EdgeCount { get; set; }
    public required string SourceFileName { get; init; }

    public string GetDisplayName() => L.F("Import.RowLabel", SourceFileName, RowIndex);
}
