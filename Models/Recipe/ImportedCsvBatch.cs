namespace otomasyon.Models.Recipe;

/// <summary>Dışarıdan yüklenen CSV satırları; satırlar tek tek kaldırılabilir.</summary>
public sealed class ImportedCsvBatch
{
    public required string SourceFilePath { get; init; }
    public List<ImportedCsvRow> Rows { get; } = new();

    public int LineCount => Rows.Count;

    public string DisplayName => Path.GetFileName(SourceFilePath);

    public IReadOnlyList<string> Lines => Rows.Select(r => r.RawLine).ToList();
}
