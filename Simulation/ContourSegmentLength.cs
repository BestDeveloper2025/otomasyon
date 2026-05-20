using otomasyon.Geometry;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

internal static class ContourSegmentLength
{
    public static double ComputeMm(in ContourPathSegment segment)
        => SegmentLength.FromBulgeMm(
            segment.StartX, segment.StartY,
            segment.EndX, segment.EndY,
            segment.Bulge);
}
