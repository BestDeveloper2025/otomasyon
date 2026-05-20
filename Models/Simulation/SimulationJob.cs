using otomasyon.Models;

namespace otomasyon.Models.Simulation;

/// <summary>Simülasyon için DXF sahnesi + kullanıcı parametreleri.</summary>
public sealed class SimulationJob
{
    public required DxfScene Scene { get; init; }
    public required ContourPath Path { get; init; }
    public required MachiningPlan Plan { get; init; }
    public required StoneToolSettings Tool { get; init; }
    public required string SourceFilePath { get; init; }
}
