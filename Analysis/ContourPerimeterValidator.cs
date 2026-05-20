using otomasyon.Geometry;

namespace otomasyon.Analysis;

/// <summary>Üçgen kapanış (diyagonal) gibi hatalı konturları düzeltir.</summary>
internal static class ContourPerimeterValidator
{
    public static List<ContourPathOrderer.OrderedSegment> EnsureValidPerimeter(
        List<ContourPathOrderer.OrderedSegment> segments,
        double tol)
    {
        if (segments.Count == 0)
            return segments;

        if (!HasSuspiciousDiagonal(segments, tol) && segments.Count >= 4)
            return segments;

        var corners = CollectUniqueCorners(segments, tol);
        if (corners.Count < 3)
            return segments;

        if (corners.Count == 3)
            ContourCornerRepair.InsertMissingAxisAlignedCorners(corners, tol);

        if (corners.Count < 3)
            return segments;

        var rebuilt = new List<ContourPathOrderer.OrderedSegment>();
        ContourPathOrderer.AppendOrderedSegmentsFromChain(corners, rebuilt);
        if (rebuilt.Count >= 3 && !HasSuspiciousDiagonal(rebuilt, tol))
            return rebuilt;

        return segments;
    }

    private static bool HasSuspiciousDiagonal(
        IReadOnlyList<ContourPathOrderer.OrderedSegment> segments,
        double tol)
    {
        if (segments.Count != 3)
            return false;

        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;
        foreach (var s in segments)
        {
            minX = Math.Min(minX, Math.Min(s.StartX, s.EndX));
            maxX = Math.Max(maxX, Math.Max(s.StartX, s.EndX));
            minY = Math.Min(minY, Math.Min(s.StartY, s.EndY));
            maxY = Math.Max(maxY, Math.Max(s.StartY, s.EndY));
        }

        double w = maxX - minX;
        double h = maxY - minY;
        if (w < tol || h < tol)
            return false;

        double diag = Math.Sqrt(w * w + h * h);
        foreach (var s in segments)
        {
            double len = SegmentLength.FromBulgeMm(s.StartX, s.StartY, s.EndX, s.EndY, s.Bulge);
            if (len > diag * 0.92)
                return true;
        }

        return false;
    }

    private static List<(double X, double Y, double Bulge)> CollectUniqueCorners(
        IReadOnlyList<ContourPathOrderer.OrderedSegment> segments,
        double tol)
    {
        var list = new List<(double X, double Y, double Bulge)>();
        foreach (var s in segments)
        {
            TryAddCorner(list, s.StartX, s.StartY, tol);
            TryAddCorner(list, s.EndX, s.EndY, tol);
        }

        return list;
    }

    private static void TryAddCorner(
        List<(double X, double Y, double Bulge)> list,
        double x, double y,
        double tol)
    {
        foreach (var c in list)
        {
            if (Math.Abs(c.X - x) <= tol && Math.Abs(c.Y - y) <= tol)
                return;
        }

        list.Add((x, y, 0));
    }
}
