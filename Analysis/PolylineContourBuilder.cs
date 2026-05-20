using netDxf.Entities;
using otomasyon.Models;
using static otomasyon.Geometry.GeometryHelper;

namespace otomasyon.Analysis;

/// <summary>
/// Kapalı Polyline2D: her tepe noktasından sonrakine bir kenar (bulge = yay).
/// (0,0) yakınından CCW.
/// </summary>
public static class PolylineContourBuilder
{
    private const double Eps = 1e-9;

    public static bool TryBuild(DxfScene scene, out List<ContourPathOrderer.OrderedSegment> segments)
    {
        segments = new List<ContourPathOrderer.OrderedSegment>();
        double tol = ContourTolerance.FromScene(scene);
        double bestArea = 0;
        Polyline2D? best = null;

        foreach (var entity in scene.Entities)
        {
            if (entity is not Polyline2D candidate)
                continue;

            if (!IsClosedContour(candidate, tol))
                continue;

            double area = Math.Abs(SignedArea(candidate, tol));
            if (area <= bestArea)
                continue;

            bestArea = area;
            best = candidate;
        }

        if (best is null)
            return false;

        BuildSegments(best, tol, segments);
        return segments.Count >= 2;
    }

    private static void BuildSegments(Polyline2D poly, double tol, List<ContourPathOrderer.OrderedSegment> segments)
    {
        var verts = poly.Vertexes;
        int n = verts.Count;
        if (n < 2)
            return;

        var loop = new List<(double X, double Y, double Bulge)>(n);
        for (int i = 0; i < n; i++)
        {
            var p = verts[i].Position;
            loop.Add((p.X, p.Y, verts[i].Bulge));
        }

        if (loop.Count > 1 && PointsNear(loop[0].X, loop[0].Y, loop[^1].X, loop[^1].Y, tol))
            loop.RemoveAt(loop.Count - 1);

        ContourPathOrderer.AppendOrderedSegmentsFromChain(loop, segments);
    }

    private static bool IsClosedContour(Polyline2D poly, double tol)
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

    private static double SignedArea(Polyline2D poly, double tol)
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
}
