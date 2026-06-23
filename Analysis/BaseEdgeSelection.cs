namespace otomasyon.Analysis;

public readonly record struct BaseEdgeSelection(double StartX, double StartY, double EndX, double EndY)
{
    public static BaseEdgeSelection FromSegment(ContourPathOrderer.OrderedSegment seg)
        => new(seg.StartX, seg.StartY, seg.EndX, seg.EndY);

    public bool Matches(ContourPathOrderer.OrderedSegment seg, double eps)
    {
        bool forward =
            PointsNear(seg.StartX, seg.StartY, StartX, StartY, eps)
            && PointsNear(seg.EndX, seg.EndY, EndX, EndY, eps);
        bool reverse =
            PointsNear(seg.StartX, seg.StartY, EndX, EndY, eps)
            && PointsNear(seg.EndX, seg.EndY, StartX, StartY, eps);
        return forward || reverse;
    }

    private static bool PointsNear(double ax, double ay, double bx, double by, double eps)
        => Math.Abs(ax - bx) <= eps && Math.Abs(ay - by) <= eps;
}
