using netDxf.Entities;
using otomasyon.Geometry;
using otomasyon.Models;
using static otomasyon.Geometry.GeometryHelper;

namespace otomasyon.Analysis;

/// <summary>
/// Ana kontur dışındaki kapalı iç şekilleri (menfez) bulur;
/// başlangıç noktasına (0,0) uzaklığa göre M1, M2… atar.
/// </summary>
public static class VentExtractor
{
    private readonly struct LoopCandidate
    {
        public List<(double X, double Y, double Bulge)> Vertices { get; init; }
        public double Area { get; init; }
        public double CenterX { get; init; }
        public double CenterY { get; init; }
        public double RadiusMm { get; init; }
    }

    public static IReadOnlyList<VentFeature> Extract(DxfScene scene)
    {
        if (scene.ContourEdges.Count < 3)
            return Array.Empty<VentFeature>();

        double tol = ContourTolerance.FromScene(scene);
        var mainPolygon = BuildMainPolygon(scene);
        if (mainPolygon.Count < 3)
            return Array.Empty<VentFeature>();

        double mainArea = Math.Abs(SignedArea(mainPolygon));
        if (mainArea < tol * tol)
            return Array.Empty<VentFeature>();

        var candidates = new List<LoopCandidate>();
        CollectPolylineLoops(scene, tol, candidates);
        CollectLineArcLoops(scene, candidates);
        CollectCircleLoops(scene, tol, candidates);

        candidates = DeduplicateCandidates(candidates, tol);

        var vents = new List<VentFeature>();
        foreach (var c in candidates)
        {
            if (IsSameLoopAsMain(c, mainArea, mainPolygon, tol))
                continue;

            if (!IsPointInsidePolygon(c.CenterX, c.CenterY, mainPolygon))
                continue;

            double dist = Math.Sqrt(c.CenterX * c.CenterX + c.CenterY * c.CenterY);
            vents.Add(new VentFeature
            {
                CenterX = c.CenterX,
                CenterY = c.CenterY,
                DistanceFromOriginMm = dist,
                AreaMm2 = c.Area,
                RadiusMm = c.RadiusMm
            });
        }

        vents.Sort((a, b) =>
        {
            int cmp = a.DistanceFromOriginMm.CompareTo(b.DistanceFromOriginMm);
            if (cmp != 0)
                return cmp;
            cmp = a.AreaMm2.CompareTo(b.AreaMm2);
            if (cmp != 0)
                return cmp;
            return a.CenterX.CompareTo(b.CenterX);
        });

        for (int i = 0; i < vents.Count; i++)
        {
            var v = vents[i];
            vents[i] = new VentFeature
            {
                Index = i + 1,
                CenterX = v.CenterX,
                CenterY = v.CenterY,
                DistanceFromOriginMm = v.DistanceFromOriginMm,
                AreaMm2 = v.AreaMm2,
                RadiusMm = v.RadiusMm
            };
        }

        return vents;
    }

    private static List<(double X, double Y)> BuildMainPolygon(DxfScene scene)
    {
        var poly = new List<(double X, double Y)>(scene.ContourEdges.Count);
        foreach (var edge in scene.ContourEdges)
            poly.Add((edge.StartX, edge.StartY));
        return poly;
    }

    private static void CollectPolylineLoops(
        DxfScene scene,
        double tol,
        List<LoopCandidate> dest)
    {
        foreach (var entity in scene.Entities)
        {
            if (!TryExtractPolylineLoop(entity, tol, out var loop))
                continue;

            AddCandidate(dest, loop);
        }
    }

    private static void CollectLineArcLoops(DxfScene scene, List<LoopCandidate> dest)
    {
        var loops = new List<List<(double X, double Y, double Bulge)>>();
        LineLoopContourBuilder.CollectAllClosedLoops(scene, loops);
        foreach (var loop in loops)
            AddCandidate(dest, loop);
    }

    private static void CollectCircleLoops(DxfScene scene, double tol, List<LoopCandidate> dest)
    {
        foreach (var entity in scene.Entities)
        {
            if (entity is not Circle circle || circle.Radius <= tol)
                continue;

            double cx = circle.Center.X;
            double cy = circle.Center.Y;
            double area = Math.PI * circle.Radius * circle.Radius;
            dest.Add(new LoopCandidate
            {
                Vertices = new List<(double X, double Y, double Bulge)>(),
                Area = area,
                CenterX = cx,
                CenterY = cy,
                RadiusMm = circle.Radius
            });
        }
    }

    private static bool TryExtractPolylineLoop(
        object entity,
        double tol,
        out List<(double X, double Y, double Bulge)> loop)
    {
        loop = new List<(double X, double Y, double Bulge)>();

        switch (entity)
        {
            case Polyline2D p2 when IsClosedPolyline(p2.Vertexes.Select(v => (v.Position.X, v.Position.Y)).ToList(), p2.IsClosed, tol):
                foreach (var v in p2.Vertexes)
                    loop.Add((v.Position.X, v.Position.Y, v.Bulge));
                break;

            case Polyline3D p3 when IsClosedPolyline(p3.Vertexes.Select(v => (v.X, v.Y)).ToList(), p3.IsClosed, tol):
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

    private static bool IsClosedPolyline(IReadOnlyList<(double X, double Y)> verts, bool isClosedFlag, double tol)
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

    private static void AddCandidate(
        List<LoopCandidate> dest,
        List<(double X, double Y, double Bulge)> loop)
    {
        double area = Math.Abs(SignedAreaOfBulgeLoop(loop));
        if (area < 1e-6)
            return;

        ComputeCentroid(loop, out double cx, out double cy);
        dest.Add(new LoopCandidate
        {
            Vertices = loop,
            Area = area,
            CenterX = cx,
            CenterY = cy,
            RadiusMm = ComputeRadiusMm(loop, cx, cy)
        });
    }

    private static double ComputeRadiusMm(
        IReadOnlyList<(double X, double Y, double Bulge)> loop,
        double cx,
        double cy)
    {
        double maxR = 0;
        foreach (var v in loop)
        {
            double dx = v.X - cx;
            double dy = v.Y - cy;
            double d = Math.Sqrt(dx * dx + dy * dy);
            if (d > maxR)
                maxR = d;
        }

        return maxR;
    }

    private static void ComputeCentroid(
        IReadOnlyList<(double X, double Y, double Bulge)> loop,
        out double cx,
        out double cy)
    {
        double sx = 0;
        double sy = 0;
        foreach (var v in loop)
        {
            sx += v.X;
            sy += v.Y;
        }

        cx = sx / loop.Count;
        cy = sy / loop.Count;
    }

    private static List<LoopCandidate> DeduplicateCandidates(List<LoopCandidate> candidates, double tol)
    {
        if (candidates.Count <= 1)
            return candidates;

        double matchEps = Math.Max(tol, 1e-3);
        var kept = new List<LoopCandidate>();

        foreach (var c in candidates)
        {
            bool duplicate = false;
            foreach (var k in kept)
            {
                double dx = c.CenterX - k.CenterX;
                double dy = c.CenterY - k.CenterY;
                if (dx * dx + dy * dy > matchEps * matchEps)
                    continue;

                double areaRatio = Math.Abs(c.Area - k.Area) / Math.Max(c.Area, k.Area);
                if (areaRatio <= 0.02)
                {
                    duplicate = true;
                    break;
                }
            }

            if (!duplicate)
                kept.Add(c);
        }

        return kept;
    }

    private static bool IsSameLoopAsMain(
        LoopCandidate candidate,
        double mainArea,
        IReadOnlyList<(double X, double Y)> mainPolygon,
        double tol)
    {
        double areaRatio = Math.Abs(candidate.Area - mainArea) / Math.Max(candidate.Area, mainArea);
        if (areaRatio > 0.02)
            return false;

        return IsPointInsidePolygon(candidate.CenterX, candidate.CenterY, mainPolygon)
            || DistanceToMainPolygon(candidate.CenterX, candidate.CenterY, mainPolygon) <= tol * 10;
    }

    private static double DistanceToMainPolygon(
        double px, double py,
        IReadOnlyList<(double X, double Y)> poly)
    {
        double best = double.PositiveInfinity;
        int n = poly.Count;
        for (int i = 0; i < n; i++)
        {
            var a = poly[i];
            var b = poly[(i + 1) % n];
            double d = DistancePointToSegment(px, py, a.X, a.Y, b.X, b.Y);
            if (d < best)
                best = d;
        }

        return best;
    }

    private static double DistancePointToSegment(
        double px, double py,
        double ax, double ay,
        double bx, double by)
    {
        double dx = bx - ax;
        double dy = by - ay;
        double len2 = dx * dx + dy * dy;
        if (len2 < 1e-18)
            return Math.Sqrt((px - ax) * (px - ax) + (py - ay) * (py - ay));

        double t = Math.Clamp(((px - ax) * dx + (py - ay) * dy) / len2, 0, 1);
        double qx = ax + t * dx;
        double qy = ay + t * dy;
        double ex = px - qx;
        double ey = py - qy;
        return Math.Sqrt(ex * ex + ey * ey);
    }

    private static bool IsPointInsidePolygon(
        double px, double py,
        IReadOnlyList<(double X, double Y)> poly)
    {
        bool inside = false;
        int n = poly.Count;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var pi = poly[i];
            var pj = poly[j];
            if ((pi.Y > py) != (pj.Y > py)
                && px < (pj.X - pi.X) * (py - pi.Y) / (pj.Y - pi.Y) + pi.X)
            {
                inside = !inside;
            }
        }

        return inside;
    }
}
