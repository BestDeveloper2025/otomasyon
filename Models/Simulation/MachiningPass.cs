namespace otomasyon.Models.Simulation;

/// <summary>Konturdan içeri tek bir taş geçişi (derinlik mm).</summary>
public sealed class MachiningPass
{
    public int PassNumber { get; init; }
    public double DepthFromContourMm { get; init; }
}
