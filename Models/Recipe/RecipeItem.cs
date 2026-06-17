using otomasyon.Models.Simulation;
using otomasyon.Simulation;

namespace otomasyon.Models.Recipe;

/// <summary>Reçetedeki tek şekil: DXF + işleme parametreleri + CSV çıktı ayarları.</summary>
public sealed class RecipeItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string DisplayName { get; init; }
    public required string SourceFilePath { get; init; }
    public required SimulationJob Job { get; init; }
    public required CsvFileExporter.ExportOptions ExportOptions { get; init; }
    public DateTime AddedAt { get; init; } = DateTime.Now;

    public int EdgeCount => Job.Path.Segments.Count;
}