namespace otomasyon.Simulation;

public sealed class SimulationSnapshot
{
    public bool IsFinished { get; init; }
    public int SegmentIndex { get; init; }
    public int EdgeIndex { get; init; }
    public int CornerIndex { get; init; }
    public int TourIndex { get; init; }
    public int TourCount { get; init; }
    public int PassIndex { get; init; }
    public int PassCountOnEdge { get; init; }
    public double PassDepthMm { get; init; }
    public bool ToolIsEngaged { get; init; }
    public double DistanceOnEdgeMm { get; init; }
    public double EdgeLengthMm { get; init; }
    public double TotalTraversedMm { get; init; }
    public double TotalCuttingMm { get; init; }
    public double ToolX { get; init; }
    public double ToolY { get; init; }
    public double InwardNormalDeg { get; init; }
    public string StatusText { get; init; } = string.Empty;
}
