namespace otomasyon.Models.Simulation;

/// <summary>CCW sıralı tek kontur kenarı (düz veya yay).</summary>
public sealed class ContourPathSegment
{
    public int EdgeIndex { get; init; }
    public int CornerIndex { get; init; }
    public bool IsArc { get; init; }
    public double Bulge { get; init; }

    public double StartX { get; init; }
    public double StartY { get; init; }
    public double EndX { get; init; }
    public double EndY { get; init; }

    public double LengthMm { get; init; }

    public double? CenterX { get; init; }
    public double? CenterY { get; init; }
    public double? Radius { get; init; }
    public double? StartAngleDeg { get; init; }
    public double? EndAngleDeg { get; init; }
}
