namespace otomasyon.Models.Simulation;

public sealed class EdgeMachiningPlan
{
    public int EdgeIndex { get; init; }
    public double TargetThicknessMm { get; init; }

    /// <summary>Kenarda kenardan ne kadar içeriden başlanacağı (mm).</summary>
    public double OffsetMm { get; init; }

    public IReadOnlyList<MachiningPass> Passes { get; init; } = Array.Empty<MachiningPass>();
}
