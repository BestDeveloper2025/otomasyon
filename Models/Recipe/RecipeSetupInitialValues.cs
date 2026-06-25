using otomasyon.Models.Simulation;
using otomasyon.Simulation;

namespace otomasyon.Models.Recipe;

/// <summary>Reçete düzenleme diyaloğunda önceden doldurulacak kullanıcı parametreleri.</summary>
public sealed class RecipeSetupInitialValues
{
    public required IReadOnlyDictionary<int, double> ThicknessByEdge { get; init; }
    public required IReadOnlyDictionary<int, double> OffsetByEdge { get; init; }
    public required IReadOnlyDictionary<int, double> VentStrippingByIndex { get; init; }
    public required StoneToolSettings Tool { get; init; }
    public required CsvFileExporter.ExportOptions ExportOptions { get; init; }

    public static RecipeSetupInitialValues FromRecipeItem(RecipeItem item)
    {
        var thickness = new Dictionary<int, double>();
        var offsets = new Dictionary<int, double>();
        foreach (var edge in item.Job.Plan.Edges)
        {
            thickness[edge.EdgeIndex] = edge.TargetThicknessMm;
            offsets[edge.EdgeIndex] = edge.OffsetMm;
        }

        var ventStripping = item.Job.VentStrippingByIndex.Count > 0
            ? item.Job.VentStrippingByIndex.ToDictionary(kv => kv.Key, kv => kv.Value)
            : item.ExportOptions.VentStrippingByIndex.ToDictionary(kv => kv.Key, kv => kv.Value);

        return new RecipeSetupInitialValues
        {
            ThicknessByEdge = thickness,
            OffsetByEdge = offsets,
            VentStrippingByIndex = ventStripping,
            Tool = item.Job.Tool,
            ExportOptions = item.ExportOptions
        };
    }
}
