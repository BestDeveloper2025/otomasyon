using netDxf.Entities;
using otomasyon.Geometry;
using otomasyon.Models;
using static otomasyon.Geometry.GeometryHelper;

namespace otomasyon.Analysis;

/// <summary>
/// Ayrı LINE ve ARC parçalarından kapalı kontur (CCW).
/// </summary>
public static class LineLoopContourBuilder
{
    private const double MinEps = 1e-6;

    private readonly struct RawEdge
    {
        public double X0 { get; init; }
        public double Y0 { get; init; }
        public double X1 { get; init; }
        public double Y1 { get; init; }
        public double Bulge { get; init; }
    }

    public static bool TryBuild(DxfScene scene, out List<ContourPathOrderer.OrderedSegment> segments)
    {
        segments = new List<ContourPathOrderer.OrderedSegment>();
        if (!scene.Bounds.HasBounds)
            return false;

        double eps = ContourTolerance.FromScene(scene);
        var raw = SnapEdgeEndpoints(CollectChainEdges(scene), eps);
        if (raw.Count < 2)
            return false;

        if (!TryFindLargestClosedLoop(raw, eps, out var loop))
            return false;

        ContourPathOrderer.AppendOrderedSegmentsFromChain(loop, segments);
        return segments.Count >= 2;
    }

    private static List<RawEdge> CollectChainEdges(DxfScene scene)
    {
        var list = new List<RawEdge>();
        foreach (var entity in scene.Entities)
        {
            switch (entity)
            {
                case Line line:
                    AddLine(list, line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y, 0);
                    break;
                case Arc arc:
                    ArcSampler.GetArcEndpointXY(arc, out double sx, out double sy, out double ex, out double ey);
                    if (BulgeArcConverter.TryFromArcEntity(arc, out _, out _, out _, out _, out _, out double bulge))
                        AddLine(list, sx, sy, ex, ey, bulge);
                    else
                        AddLine(list, sx, sy, ex, ey, 0);
                    break;
            }
        }

        return list;
    }

    private static void AddLine(List<RawEdge> list, double x0, double y0, double x1, double y1, double bulge)
    {
        if (PointsNear(x0, y0, x1, y1, MinEps))
            return;

        list.Add(new RawEdge { X0 = x0, Y0 = y0, X1 = x1, Y1 = y1, Bulge = bulge });
    }

    /// <summary>Yakın uç noktaları tek koordinata çeker; LINE–ARC birleşimini garanti eder.</summary>
    private static List<RawEdge> SnapEdgeEndpoints(List<RawEdge> raw, double eps)
    {
        if (raw.Count == 0)
            return raw;

        var nodes = new List<(double X, double Y)>();

        (double X, double Y) Snap(double x, double y)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (PointsNear(n.X, n.Y, x, y, eps))
                    return n;
            }

            var created = (x, y);
            nodes.Add(created);
            return created;
        }

        var snapped = new List<RawEdge>(raw.Count);
        foreach (var e in raw)
        {
            var p0 = Snap(e.X0, e.Y0);
            var p1 = Snap(e.X1, e.Y1);
            if (PointsNear(p0.X, p0.Y, p1.X, p1.Y, MinEps))
                continue;

            snapped.Add(new RawEdge
            {
                X0 = p0.X,
                Y0 = p0.Y,
                X1 = p1.X,
                Y1 = p1.Y,
                Bulge = e.Bulge
            });
        }

        return snapped;
    }

    private static bool TryFindLargestClosedLoop(List<RawEdge> raw, double eps, out List<(double X, double Y, double Bulge)> loop)
    {
        loop = new List<(double X, double Y, double Bulge)>();
        double bestArea = 0;
        List<(double X, double Y, double Bulge)>? best = null;

        var components = SplitIntoComponents(raw, eps);
        foreach (var comp in components)
        {
            if (!TryTraceSingleLoop(comp, eps, out var candidate))
                continue;

            double area = Math.Abs(SignedAreaOfLoop(candidate));
            if (area <= bestArea)
                continue;

            bestArea = area;
            best = candidate;
        }

        if (best is null)
            return false;

        loop = best;
        return true;
    }

    private static List<List<RawEdge>> SplitIntoComponents(List<RawEdge> raw, double eps)
    {
        var used = new bool[raw.Count];
        var components = new List<List<RawEdge>>();

        for (int i = 0; i < raw.Count; i++)
        {
            if (used[i])
                continue;

            var comp = new List<RawEdge>();
            var stack = new Stack<int>();
            stack.Push(i);
            used[i] = true;

            while (stack.Count > 0)
            {
                int idx = stack.Pop();
                var e = raw[idx];
                comp.Add(e);

                for (int j = 0; j < raw.Count; j++)
                {
                    if (used[j])
                        continue;

                    var f = raw[j];
                    if (SharesEndpoint(e, f, eps))
                    {
                        used[j] = true;
                        stack.Push(j);
                    }
                }
            }

            if (comp.Count > 0)
                components.Add(comp);
        }

        return components;
    }

    private static bool SharesEndpoint(RawEdge a, RawEdge b, double eps)
    {
        return PointsNear(a.X0, a.Y0, b.X0, b.Y0, eps)
            || PointsNear(a.X0, a.Y0, b.X1, b.Y1, eps)
            || PointsNear(a.X1, a.Y1, b.X0, b.Y0, eps)
            || PointsNear(a.X1, a.Y1, b.X1, b.Y1, eps);
    }

    private static bool TryTraceSingleLoop(List<RawEdge> comp, double eps, out List<(double X, double Y, double Bulge)> loop)
    {
        loop = new List<(double X, double Y, double Bulge)>();
        if (comp.Count < 3)
            return false;

        foreach (var e in comp)
        {
            int deg0 = CountEndpoint(comp, e.X0, e.Y0, eps);
            int deg1 = CountEndpoint(comp, e.X1, e.Y1, eps);
            if (deg0 != 2 || deg1 != 2)
                return false;
        }

        int preferred = PickOriginStartEdge(comp, eps);
        if (TryTraceFromFirstEdge(comp, preferred, forward: true, eps, out loop) &&
            loop.Count == comp.Count &&
            LoopUsesOnlyGraphEdges(loop, comp, eps))
            return true;

        for (int fi = 0; fi < comp.Count; fi++)
        {
            if (TryTraceFromFirstEdge(comp, fi, forward: true, eps, out var cw) &&
                cw.Count == comp.Count &&
                LoopUsesOnlyGraphEdges(cw, comp, eps))
            {
                loop = cw;
                return true;
            }

            if (TryTraceFromFirstEdge(comp, fi, forward: false, eps, out var ccw) &&
                ccw.Count == comp.Count &&
                LoopUsesOnlyGraphEdges(ccw, comp, eps))
            {
                loop = ccw;
                return true;
            }
        }

        return false;
    }

    private static bool TryTraceFromFirstEdge(
        List<RawEdge> comp,
        int firstIdx,
        bool forward,
        double eps,
        out List<(double X, double Y, double Bulge)> loop)
    {
        loop = new List<(double X, double Y, double Bulge)>();
        var remaining = new List<RawEdge>(comp);
        var first = remaining[firstIdx];
        remaining.RemoveAt(firstIdx);

        double startX, startY, curX, curY;
        double firstBulge;
        if (forward)
        {
            startX = first.X0;
            startY = first.Y0;
            curX = first.X1;
            curY = first.Y1;
            firstBulge = first.Bulge;
        }
        else
        {
            startX = first.X1;
            startY = first.Y1;
            curX = first.X0;
            curY = first.Y0;
            firstBulge = -first.Bulge;
        }

        loop.Add((startX, startY, firstBulge));
        loop.Add((curX, curY, 0));
        double prevX = startX, prevY = startY;

        for (int step = 0; step < comp.Count - 1; step++)
        {
            int idx = FindNextEdge(remaining, curX, curY, prevX, prevY, eps);
            if (idx < 0)
                return false;

            var edge = remaining[idx];
            remaining.RemoveAt(idx);

            prevX = curX;
            prevY = curY;

            double departBulge;
            if (PointsNear(edge.X0, edge.Y0, curX, curY, eps))
            {
                departBulge = edge.Bulge;
                curX = edge.X1;
                curY = edge.Y1;
            }
            else
            {
                departBulge = -edge.Bulge;
                curX = edge.X0;
                curY = edge.Y0;
            }

            var last = loop[^1];
            loop[^1] = (last.X, last.Y, departBulge);
            loop.Add((curX, curY, 0));
        }

        if (remaining.Count > 0)
            return false;

        if (!PointsNear(curX, curY, startX, startY, eps))
            return false;

        if (loop.Count > 1 && PointsNear(loop[0].X, loop[0].Y, loop[^1].X, loop[^1].Y, eps))
            loop.RemoveAt(loop.Count - 1);

        return loop.Count >= 3;
    }

    private static bool LoopUsesOnlyGraphEdges(
        List<(double X, double Y, double Bulge)> loop,
        List<RawEdge> comp,
        double eps)
    {
        int n = loop.Count;
        if (n < 3)
            return false;

        double connectEps = Math.Max(eps, 1e-3);

        for (int i = 0; i < n; i++)
        {
            var a = loop[i];
            var b = loop[(i + 1) % n];
            if (PointsNear(a.X, a.Y, b.X, b.Y, connectEps))
                continue;

            if (!HasGraphEdge(comp, a.X, a.Y, b.X, b.Y, connectEps))
                return false;
        }

        return true;
    }

    private static bool HasGraphEdge(
        List<RawEdge> comp,
        double ax, double ay,
        double bx, double by,
        double eps)
    {
        foreach (var e in comp)
        {
            if ((PointsNear(e.X0, e.Y0, ax, ay, eps) && PointsNear(e.X1, e.Y1, bx, by, eps))
                || (PointsNear(e.X1, e.Y1, ax, ay, eps) && PointsNear(e.X0, e.Y0, bx, by, eps)))
                return true;
        }

        return false;
    }

    private static int CountEndpoint(List<RawEdge> comp, double x, double y, double eps)
    {
        int c = 0;
        foreach (var e in comp)
        {
            if (PointsNear(e.X0, e.Y0, x, y, eps)) c++;
            if (PointsNear(e.X1, e.Y1, x, y, eps)) c++;
        }

        return c;
    }

    private static int FindNextEdge(
        List<RawEdge> edges,
        double x, double y,
        double prevX, double prevY,
        double eps)
    {
        var candidates = new List<int>();
        for (int i = 0; i < edges.Count; i++)
        {
            var e = edges[i];
            double nx, ny;
            if (PointsNear(e.X0, e.Y0, x, y, eps))
            {
                nx = e.X1;
                ny = e.Y1;
            }
            else if (PointsNear(e.X1, e.Y1, x, y, eps))
            {
                nx = e.X0;
                ny = e.Y0;
            }
            else
            {
                continue;
            }

            if (PointsNear(nx, ny, prevX, prevY, eps))
                continue;

            candidates.Add(i);
        }

        if (candidates.Count == 0)
            return -1;
        if (candidates.Count == 1)
            return candidates[0];

        double inDx = x - prevX;
        double inDy = y - prevY;
        double inLen = Math.Sqrt(inDx * inDx + inDy * inDy);
        if (inLen < eps)
            return candidates[0];

        inDx /= inLen;
        inDy /= inLen;

        int best = candidates[0];
        double bestCross = double.NegativeInfinity;

        foreach (int idx in candidates)
        {
            var e = edges[idx];
            double outDx, outDy;
            if (PointsNear(e.X0, e.Y0, x, y, eps))
            {
                outDx = e.X1 - x;
                outDy = e.Y1 - y;
            }
            else
            {
                outDx = e.X0 - x;
                outDy = e.Y0 - y;
            }

            double outLen = Math.Sqrt(outDx * outDx + outDy * outDy);
            if (outLen < eps)
                continue;

            outDx /= outLen;
            outDy /= outLen;
            double cross = inDx * outDy - inDy * outDx;
            if (cross > bestCross)
            {
                bestCross = cross;
                best = idx;
            }
        }

        return best;
    }

    private static double SignedAreaOfLoop(List<(double X, double Y, double Bulge)> verts)
        => SignedAreaOfBulgeLoop(verts);

    private static int PickOriginStartEdge(List<RawEdge> comp, double eps)
    {
        GetOriginVertex(comp, out double ox, out double oy);
        int bestIdx = -1;
        double bestScore = double.NegativeInfinity;

        for (int i = 0; i < comp.Count; i++)
        {
            var e = comp[i];
            double score = OriginDepartureScore(e, ox, oy, eps);
            if (score > bestScore)
            {
                bestScore = score;
                bestIdx = i;
            }
        }

        return bestIdx >= 0 ? bestIdx : 0;
    }

    /// <summary>(0,0) çıkışında +X yönüne en yakın kenarı seçer.</summary>
    private static double OriginDepartureScore(RawEdge e, double ox, double oy, double eps)
    {
        double dx, dy;
        if (PointsNear(e.X0, e.Y0, ox, oy, eps))
        {
            dx = e.X1 - ox;
            dy = e.Y1 - oy;
        }
        else if (PointsNear(e.X1, e.Y1, ox, oy, eps))
        {
            dx = e.X0 - ox;
            dy = e.Y0 - oy;
        }
        else
        {
            return double.NegativeInfinity;
        }

        double len = Math.Sqrt(dx * dx + dy * dy);
        if (len < eps)
            return double.NegativeInfinity;

        return dx / len;
    }

    private static void GetOriginVertex(List<RawEdge> comp, out double ox, out double oy)
    {
        ox = comp[0].X0;
        oy = comp[0].Y0;
        foreach (var e in comp)
        {
            foreach (var (x, y) in new[] { (e.X0, e.Y0), (e.X1, e.Y1) })
            {
                if (CompareOriginStart(x, y, ox, oy) < 0)
                {
                    ox = x;
                    oy = y;
                }
            }
        }
    }

    /// <summary>(0,0) yakın, +X öncelikli köşe sıralaması.</summary>
    private static int CompareOriginStart(double ax, double ay, double bx, double by)
    {
        double da = ax * ax + ay * ay;
        double db = bx * bx + by * by;
        if (Math.Abs(da - db) > MinEps)
            return da < db ? -1 : 1;
        if (Math.Abs(ax - bx) > MinEps)
            return ax > bx ? -1 : 1;
        if (Math.Abs(ay - by) > MinEps)
            return ay < by ? -1 : 1;
        return 0;
    }
}
