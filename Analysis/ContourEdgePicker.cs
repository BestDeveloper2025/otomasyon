using otomasyon.Models;

namespace otomasyon.Analysis;

public static class ContourEdgePicker
{
    public static bool TryPick(
        DxfScene scene,
        double worldX,
        double worldY,
        out ContourPathOrderer.OrderedSegment segment,
        out int edgeIndex)
    {
        segment = default;
        edgeIndex = -1;

        if (!ContourPathOrderer.TryBuildOrderedSegments(scene, out var ordered) || ordered.Count == 0)
            return false;

        double tol = ContourTolerance.FromScene(scene);
        double maxDist = Math.Max(tol * 8, Math.Max(scene.Bounds.Width, scene.Bounds.Height) * 0.08);

        double bestDist = double.PositiveInfinity;
        int bestIdx = -1;

        for (int i = 0; i < ordered.Count; i++)
        {
            var seg = ordered[i];
            double d = DistanceToSegment(worldX, worldY, seg.StartX, seg.StartY, seg.EndX, seg.EndY);
            if (d < bestDist)
            {
                bestDist = d;
                bestIdx = i;
            }
        }

        if (bestIdx < 0 || bestDist > maxDist)
            return false;

        segment = ordered[bestIdx];
        edgeIndex = segment.EdgeIndex;
        return true;
    }

    public static bool TryFindMatchingSegment(
        DxfScene scene,
        BaseEdgeSelection selection,
        out ContourPathOrderer.OrderedSegment segment)
    {
        segment = default;
        if (!ContourPathOrderer.TryBuildOrderedSegments(scene, out var ordered))
            return false;

        double eps = ContourTolerance.FromScene(scene);
        foreach (var seg in ordered)
        {
            if (selection.Matches(seg, eps))
            {
                segment = seg;
                return true;
            }
        }

        return false;
    }

    private static double DistanceToSegment(double px, double py, double x0, double y0, double x1, double y1)
    {
        double dx = x1 - x0;
        double dy = y1 - y0;
        double lenSq = dx * dx + dy * dy;
        if (lenSq < 1e-18)
            return Math.Sqrt((px - x0) * (px - x0) + (py - y0) * (py - y0));

        double t = Math.Clamp(((px - x0) * dx + (py - y0) * dy) / lenSq, 0, 1);
        double nx = x0 + t * dx;
        double ny = y0 + t * dy;
        double ddx = px - nx;
        double ddy = py - ny;
        return Math.Sqrt(ddx * ddx + ddy * ddy);
    }
}
