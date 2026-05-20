using otomasyon.Models;

namespace otomasyon.Geometry;

/// <summary>
/// Çizim ve metin için tüm köşe noktalarını toplar; CCW köşe numarasını eşleştirir.
/// </summary>
public static class CornerDisplayCollector
{
    public sealed class CornerLabel
    {
        public double X { get; init; }
        public double Y { get; init; }
        public int? CornerIndex { get; init; }
    }

    public static List<CornerLabel> Collect(DxfScene scene)
    {
        if (!scene.Bounds.HasBounds)
            return new List<CornerLabel>();

        var raw = new List<(double X, double Y)>(64);
        foreach (var entity in scene.Entities)
            WorldCornerCollector.CollectForDisplay(entity, raw);

        foreach (var rf in scene.RadiusFeatures)
        {
            raw.Add((rf.StartX, rf.StartY));
            raw.Add((rf.EndX, rf.EndY));
        }

        if (raw.Count == 0)
            return new List<CornerLabel>();

        double span = Math.Max(scene.Bounds.Width, scene.Bounds.Height);
        var unique = WorldCornerCollector.MergeClosePointsForDisplay(raw, span);
        double matchEps = Math.Max(1e-6, span * 1e-8);

        var contourCorners = new List<(double X, double Y, int CornerIndex)>(scene.ContourEdges.Count);
        foreach (var edge in scene.ContourEdges)
            contourCorners.Add((edge.StartX, edge.StartY, edge.CornerIndex));

        var result = new List<CornerLabel>(unique.Count);
        foreach (var p in unique)
        {
            int? k = TryMatchCornerIndex(p.X, p.Y, contourCorners, matchEps);
            result.Add(new CornerLabel { X = p.X, Y = p.Y, CornerIndex = k });
        }

        result.Sort(CompareLabels);
        return result;
    }

    private static int? TryMatchCornerIndex(
        double x, double y,
        IReadOnlyList<(double X, double Y, int CornerIndex)> contourCorners,
        double eps)
    {
        int? best = null;
        double bestDist2 = double.MaxValue;

        foreach (var (cx, cy, ki) in contourCorners)
        {
            double dx = x - cx;
            double dy = y - cy;
            double d2 = dx * dx + dy * dy;
            if (d2 > eps * eps || d2 >= bestDist2)
                continue;

            bestDist2 = d2;
            best = ki;
        }

        return best;
    }

    private static int CompareLabels(CornerLabel a, CornerLabel b)
    {
        if (a.CornerIndex is int ka && b.CornerIndex is int kb)
            return ka.CompareTo(kb);

        if (a.CornerIndex.HasValue)
            return -1;

        if (b.CornerIndex.HasValue)
            return 1;

        double angA = Math.Atan2(a.Y, a.X);
        double angB = Math.Atan2(b.Y, b.X);
        return angA.CompareTo(angB);
    }
}
