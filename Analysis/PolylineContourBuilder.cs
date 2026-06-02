using netDxf.Entities;
using otomasyon.Models;
using static otomasyon.Geometry.GeometryHelper;

namespace otomasyon.Analysis;

/// <summary>
/// Kapalı Polyline2D / Polyline3D: her tepe noktasından sonrakine bir kenar (bulge = yay).
/// (0,0) yakınından CCW.
/// </summary>
public static class PolylineContourBuilder
{
    public static bool TryBuild(DxfScene scene, out List<ContourPathOrderer.OrderedSegment> segments)
    {
        segments = new List<ContourPathOrderer.OrderedSegment>();
        double tol = ContourTolerance.FromScene(scene);
        double bestArea = 0;
        List<(double X, double Y, double Bulge)>? best = null;

        foreach (var entity in scene.Entities)
        {
            if (!TryExtractClosedLoop(entity, tol, out var loop))
                continue;

            double area = Math.Abs(SignedAreaOfBulgeLoop(loop));
            if (area <= bestArea)
                continue;

            bestArea = area;
            best = loop;
        }

        if (best is null)
            return false;

        ContourPathOrderer.AppendOrderedSegmentsFromChain(best, segments);
        return segments.Count >= 2;
    }

    private static bool TryExtractClosedLoop(
        object entity,
        double tol,
        out List<(double X, double Y, double Bulge)> loop)
    {
        loop = new List<(double X, double Y, double Bulge)>();

        switch (entity)
        {
            case Polyline2D p2 when IsClosedContour(p2.Vertexes.Select(v => (v.Position.X, v.Position.Y)).ToList(), p2.IsClosed, tol):
                foreach (var v in p2.Vertexes)
                    loop.Add((v.Position.X, v.Position.Y, v.Bulge));
                break;

            case Polyline3D p3 when IsClosedContour(p3.Vertexes.Select(v => (v.X, v.Y)).ToList(), p3.IsClosed, tol):
                foreach (var v in p3.Vertexes)
                    loop.Add((v.X, v.Y, 0));
                break;

            default:
                return false;
        }

        if (loop.Count > 1 && PointsNear(loop[0].X, loop[0].Y, loop[^1].X, loop[^1].Y, tol))
            loop.RemoveAt(loop.Count - 1);

        return loop.Count >= 3;
    }

    private static bool IsClosedContour(IReadOnlyList<(double X, double Y)> verts, bool isClosedFlag, double tol)
    {
        if (isClosedFlag)
            return verts.Count >= 3;

        if (verts.Count < 3)
            return false;

        var a = verts[0];
        var b = verts[^1];
        double dx = a.X - b.X, dy = a.Y - b.Y;
        return dx * dx + dy * dy <= tol * tol;
    }
}
