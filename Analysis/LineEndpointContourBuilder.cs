using netDxf.Entities;
using otomasyon.Models;

namespace otomasyon.Analysis;

/// <summary>
/// LINE uç noktalarından dikdörtgen kontur (0,0)→(+X)→… CCW.
/// </summary>
public static class LineEndpointContourBuilder
{
    public static bool TryBuild(DxfScene scene, out List<ContourPathOrderer.OrderedSegment> segments)
    {
        segments = new List<ContourPathOrderer.OrderedSegment>();
        double tol = ContourTolerance.FromScene(scene);
        var corners = ClusterLineEndpoints(scene, tol);

        if (corners.Count < 4)
            return false;

        if (corners.Count > 4)
            corners = SelectRectangleCorners(corners, tol);

        if (corners.Count != 4)
            return false;

        if (!IsAxisAlignedRectangle(corners, tol))
            return false;

        var ordered = OrderRectangleCcwFromOrigin(corners);
        ContourPathOrderer.AppendOrderedSegmentsFromChain(ordered, segments);
        return segments.Count == 4;
    }

    private static List<(double X, double Y, double Bulge)> ClusterLineEndpoints(DxfScene scene, double tol)
    {
        var raw = new List<(double X, double Y)>();
        foreach (var entity in scene.Entities)
        {
            if (entity is not Line line)
                continue;

            raw.Add((line.StartPoint.X, line.StartPoint.Y));
            raw.Add((line.EndPoint.X, line.EndPoint.Y));
        }

        var corners = new List<(double X, double Y, double Bulge)>();
        foreach (var (x, y) in raw)
        {
            bool exists = false;
            foreach (var c in corners)
            {
                if (Math.Abs(c.X - x) <= tol && Math.Abs(c.Y - y) <= tol)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
                corners.Add((x, y, 0));
        }

        return corners;
    }

    private static List<(double X, double Y, double Bulge)> SelectRectangleCorners(
        List<(double X, double Y, double Bulge)> points,
        double tol)
    {
        double minX = points.Min(p => p.X);
        double maxX = points.Max(p => p.X);
        double minY = points.Min(p => p.Y);
        double maxY = points.Max(p => p.Y);

        var bbox = new[]
        {
            (minX, minY),
            (maxX, minY),
            (maxX, maxY),
            (minX, maxY)
        };

        var result = new List<(double X, double Y, double Bulge)>();
        foreach (var (bx, by) in bbox)
        {
            foreach (var p in points)
            {
                if (Math.Abs(p.X - bx) <= tol && Math.Abs(p.Y - by) <= tol)
                {
                    result.Add(p);
                    break;
                }
            }
        }

        return result;
    }

    private static bool IsAxisAlignedRectangle(
        IReadOnlyList<(double X, double Y, double Bulge)> corners,
        double tol)
    {
        if (corners.Count != 4)
            return false;

        var xs = corners.Select(c => c.X).Distinct().ToList();
        var ys = corners.Select(c => c.Y).Distinct().ToList();
        if (xs.Count != 2 || ys.Count != 2)
            return false;

        return Math.Abs(xs[1] - xs[0]) > tol && Math.Abs(ys[1] - ys[0]) > tol;
    }

    private static List<(double X, double Y, double Bulge)> OrderRectangleCcwFromOrigin(
        List<(double X, double Y, double Bulge)> corners)
    {
        double minX = corners.Min(c => c.X);
        double maxX = corners.Max(c => c.X);
        double minY = corners.Min(c => c.Y);
        double maxY = corners.Max(c => c.Y);

        (double X, double Y, double Bulge) Find(double x, double y)
        {
            foreach (var c in corners)
            {
                if (Math.Abs(c.X - x) < 1e-6 && Math.Abs(c.Y - y) < 1e-6)
                    return c;
            }

            return (x, y, 0);
        }

        var k1 = Find(minX, minY);
        var k2 = Find(maxX, minY);
        var k3 = Find(maxX, maxY);
        var k4 = Find(minX, maxY);

        return new List<(double X, double Y, double Bulge)> { k1, k2, k3, k4 };
    }
}
