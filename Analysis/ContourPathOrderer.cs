using netDxf.Entities;
using otomasyon.Models;

namespace otomasyon.Analysis;

/// <summary>
/// Kapalı profili (0,0) yakınından başlayarak CCW (saat yönü tersi) sıraya dizer.
/// </summary>
public static class ContourPathOrderer
{
    private const double Eps = 1e-9;

    public readonly struct OrderedSegment
    {
        public int EdgeIndex { get; init; }
        public int CornerIndex { get; init; }
        public double StartX { get; init; }
        public double StartY { get; init; }
        public double EndX { get; init; }
        public double EndY { get; init; }
        public double Bulge { get; init; }
        public bool IsArc => Math.Abs(Bulge) > Eps;
    }

    public static bool TryBuildOrderedSegments(DxfScene scene, out List<OrderedSegment> segments)
    {
        segments = new List<OrderedSegment>();
        if (!TryGetDominantClosedPolyline(scene, out Polyline2D? poly))
            return false;

        var verts = poly.Vertexes;
        int n = verts.Count;
        if (n < 3)
            return false;

        bool ccwInFile = SignedArea(poly) > 0;
        int startIdx = FindOriginStartVertex(verts);

        int edgeNum = 0;
        for (int k = 0; k < n; k++)
        {
            int i = ccwInFile
                ? (startIdx + k) % n
                : (startIdx - k + n) % n;
            int j = ccwInFile
                ? (i + 1) % n
                : (i - 1 + n) % n;

            double bulge = ccwInFile
                ? verts[i].Bulge
                : -verts[j].Bulge;

            var p0 = verts[i].Position;
            var p1 = verts[j].Position;

            segments.Add(new OrderedSegment
            {
                EdgeIndex = ++edgeNum,
                CornerIndex = k + 1,
                StartX = p0.X,
                StartY = p0.Y,
                EndX = p1.X,
                EndY = p1.Y,
                Bulge = bulge
            });
        }

        return segments.Count > 0;
    }

    public static bool TryGetDominantClosedPolyline(DxfScene scene, out Polyline2D poly)
    {
        poly = null!;
        double bestArea = 0;
        Polyline2D? best = null;

        foreach (var entity in scene.Entities)
        {
            if (entity is not Polyline2D candidate || !candidate.IsClosed || candidate.Vertexes.Count < 3)
                continue;

            double abs = Math.Abs(SignedArea(candidate));
            if (abs <= bestArea)
                continue;

            bestArea = abs;
            best = candidate;
        }

        if (best is null)
            return false;

        poly = best;
        return true;
    }

    private static int FindOriginStartVertex(IReadOnlyList<Polyline2DVertex> verts)
    {
        int best = 0;
        for (int i = 1; i < verts.Count; i++)
        {
            if (CompareOriginStart(verts[i].Position, verts[best].Position) < 0)
                best = i;
        }

        return best;
    }

    /// <summary>
    /// (0,0) yakın, sağa (+X) öncelikli, altta (-Y küçük) köşe.
    /// </summary>
    private static int CompareOriginStart(netDxf.Vector2 a, netDxf.Vector2 b)
    {
        double da = a.X * a.X + a.Y * a.Y;
        double db = b.X * b.X + b.Y * b.Y;
        if (Math.Abs(da - db) > Eps)
            return da < db ? -1 : 1;

        if (Math.Abs(a.X - b.X) > Eps)
            return a.X > b.X ? -1 : 1;

        if (Math.Abs(a.Y - b.Y) > Eps)
            return a.Y < b.Y ? -1 : 1;

        return 0;
    }

    private static double SignedArea(Polyline2D poly)
    {
        var v = poly.Vertexes;
        int n = v.Count;
        double sum = 0;
        for (int i = 0; i < n; i++)
        {
            var a = v[i].Position;
            var b = v[(i + 1) % n].Position;
            sum += a.X * b.Y - b.X * a.Y;
        }

        return sum * 0.5;
    }
}
