namespace otomasyon.Models;

/// <summary>
/// Ana kontur içindeki kapalı menfez (iç boşluk) şekli.
/// </summary>
public sealed class VentFeature
{
    public int Index { get; init; }
    public double CenterX { get; init; }
    public double CenterY { get; init; }
    public double DistanceFromOriginMm { get; init; }
    public double AreaMm2 { get; init; }
}
