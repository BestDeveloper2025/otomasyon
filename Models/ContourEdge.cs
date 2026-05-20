namespace otomasyon.Models;

/// <summary>
/// (0,0) referansından saat yönünün tersine (CCW) numaralandırılmış tek kontur kenarı.
/// </summary>
public sealed class ContourEdge
{
    public int Index { get; init; }
    public int CornerIndex { get; init; }

    public double StartX { get; init; }
    public double StartY { get; init; }
    public double EndX { get; init; }
    public double EndY { get; init; }

    public bool IsRadiusSegment { get; init; }
    public int? RadiusIndex { get; init; }
    public double Bulge { get; init; }
    public double LengthMm { get; init; }
}
