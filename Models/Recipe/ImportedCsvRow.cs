using System.Globalization;

namespace otomasyon.Models.Recipe;

/// <summary>İçe aktarılmış CSV'deki tek şekil satırı.</summary>
public sealed class ImportedCsvRow
{
    public required string RawLine { get; init; }
    public int RowIndex { get; init; }
    public int ShapeCode { get; init; }
    public double CamKalinlikMm { get; init; }
    public int Adet { get; init; }
    public int EdgeCount { get; init; }
    public required string SourceFileName { get; init; }

    public string DisplayName => $"{SourceFileName} · satır {RowIndex}";
}
