namespace otomasyon.Models.Recipe;

/// <summary>İçe aktarılmış CSV satırında düzenlenebilir kullanıcı parametreleri.</summary>
public sealed class CsvRowEditModel
{
    public double KalinlikMm { get; set; }
    public int Adet { get; set; }
    public List<int> ActiveEdgeIndices { get; } = new();
    public List<int> ActiveVentIndices { get; } = new();
    public Dictionary<int, double> SaByEdge { get; } = new();
    public Dictionary<int, double> OffsetByEdge { get; } = new();
    public Dictionary<int, double> VentSaByIndex { get; } = new();
}
