namespace otomasyon.Models.Simulation;

public sealed class ContourPath
{
    public static ContourPath Empty { get; } = new(Array.Empty<ContourPathSegment>(), 0);

    public IReadOnlyList<ContourPathSegment> Segments { get; }
    public double TotalLengthMm { get; }

    public ContourPath(IReadOnlyList<ContourPathSegment> segments, double totalLengthMm)
    {
        Segments = segments;
        TotalLengthMm = totalLengthMm;
    }
}
