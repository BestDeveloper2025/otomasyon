using netDxf.Entities;
using otomasyon.Geometry;
using otomasyon.Models;
using static otomasyon.Geometry.GeometryHelper;

namespace otomasyon.Analysis;

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
        double tol = ContourTolerance.FromScene(scene);

        if (PolylineContourBuilder.TryBuild(scene, out var fromPoly))
        {
            segments = ContourPerimeterValidator.EnsureValidPerimeter(fromPoly, tol);
            return segments.Count > 0;
        }

        bool hasArcEntities = scene.Entities.Any(e => e is Arc);

        if (!hasArcEntities && LineEndpointContourBuilder.TryBuild(scene, out var fromRect))
        {
            segments = ContourPerimeterValidator.EnsureValidPerimeter(fromRect, tol);
            return segments.Count > 0;
        }

        List<OrderedSegment>? fromLines = null;
        if (LineLoopContourBuilder.TryBuild(scene, out var lineSegs))
            fromLines = lineSegs;

        if (fromLines is not null && fromLines.Count > 0)
        {
            segments = ContourPerimeterValidator.EnsureValidPerimeter(fromLines, tol);
            return segments.Count > 0;
        }

        return false;
    }

    public static bool HasSimulatableContour(DxfScene scene)
        => TryBuildOrderedSegments(scene, out var segs) && segs.Count > 0;

    public static bool TryGetDominantClosedPolyline(DxfScene scene, out Polyline2D poly)
    {
        poly = null!;
        if (!PolylineContourBuilder.TryBuild(scene, out _))
            return false;

        double bestArea = 0;
        double tol = ContourTolerance.FromScene(scene);
        foreach (var entity in scene.Entities)
        {
            if (entity is not Polyline2D candidate || !IsClosedPolyline(candidate, tol))
                continue;

            double area = Math.Abs(SignedAreaPolyline(candidate, tol));
            if (area <= bestArea)
                continue;

            bestArea = area;
            poly = candidate;
        }

        return poly is not null;
    }

    internal static void AppendOrderedSegmentsFromChain(
        List<(double X, double Y, double Bulge)> loop,
        List<OrderedSegment> segments)
    {
        if (loop.Count < 3)
            return;

        var verts = NormalizeLoopCcwFromOrigin(loop);
        int n = verts.Count;
        int edgeNum = 0;

        for (int k = 0; k < n; k++)
        {
            var p0 = verts[k];
            var p1 = verts[(k + 1) % n];
            segments.Add(new OrderedSegment
            {
                EdgeIndex = ++edgeNum,
                CornerIndex = k + 1,
                StartX = p0.X,
                StartY = p0.Y,
                EndX = p1.X,
                EndY = p1.Y,
                Bulge = p0.Bulge
            });
        }
    }

    private static bool IsClosedPolyline(Polyline2D poly, double tol)
    {
        if (poly.IsClosed)
            return poly.Vertexes.Count >= 3;
        if (poly.Vertexes.Count < 3)
            return false;
        var a = poly.Vertexes[0].Position;
        var b = poly.Vertexes[^1].Position;
        double dx = a.X - b.X, dy = a.Y - b.Y;
        return dx * dx + dy * dy <= tol * tol;
    }

    private static double SignedAreaPolyline(Polyline2D poly, double tol)
    {
        var v = poly.Vertexes;
        int n = v.Count;
        if (n < 3)
            return 0;
        double sum = 0;
        for (int i = 0; i < n; i++)
        {
            var a = v[i].Position;
            var b = v[(i + 1) % n].Position;
            sum += a.X * b.Y - b.X * a.Y;
        }

        return sum * 0.5;
    }

    private static List<(double X, double Y, double Bulge)> NormalizeLoopCcwFromOrigin(
        List<(double X, double Y, double Bulge)> loop)
    {
        if (SignedAreaOfBulgeLoop(loop) < 0)
            loop = ReverseLoop(loop);

        int start = 0;
        for (int i = 1; i < loop.Count; i++)
        {
            if (CompareOriginStart(loop[i], loop[start]) < 0)
                start = i;
        }

        if (start == 0)
            return loop;

        var rotated = new List<(double X, double Y, double Bulge)>(loop.Count);
        for (int k = 0; k < loop.Count; k++)
            rotated.Add(loop[(start + k) % loop.Count]);
        return rotated;
    }

    /// <summary>
    /// CCW yürüyüşü CW depolamaya çevirirken her tepe noktasındaki bulge işaretini ters çevirir.
    /// </summary>
    private static List<(double X, double Y, double Bulge)> ReverseLoop(List<(double X, double Y, double Bulge)> loop)
    {
        int n = loop.Count;
        var rev = new List<(double X, double Y, double Bulge)>(n);
        for (int i = 0; i < n; i++)
        {
            int vi = (n - 1 - i + n) % n;
            int prevEdge = (vi - 1 + n) % n;
            rev.Add((loop[vi].X, loop[vi].Y, -loop[prevEdge].Bulge));
        }

        return rev;
    }

    private static int CompareOriginStart((double X, double Y, double Bulge) a, (double X, double Y, double Bulge) b)
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
}
