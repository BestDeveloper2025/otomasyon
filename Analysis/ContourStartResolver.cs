using otomasyon.Settings;
using static otomasyon.Geometry.GeometryHelper;

namespace otomasyon.Analysis;

/// <summary>
/// Kontur başlangıcını makine yönüne göre belirler:
/// x eksenine paralel (açı ≈ 0°) düz kenarlar arasından en düşük Y'li kenar seçilir;
/// LTR → sol köşe + taban +X, RTL → sağ köşe + taban −X (saat yönü).
/// </summary>
public static class ContourStartResolver
{
    public static List<(double X, double Y, double Bulge)> Apply(
        List<(double X, double Y, double Bulge)> loop,
        double eps)
    {
        if (ShapeOrientationContext.UseOriginAnchor)
            return ApplyFromOriginAnchor(loop, eps, AppSettingsManager.MachineDirection);

        return ApplyStandard(loop, eps, AppSettingsManager.MachineDirection);
    }

    /// <summary>
    /// (0,0) anchor köşesinden başlar; L1 taban kenarı makine yönüne göre +X veya −X olur.
    /// </summary>
    public static List<(double X, double Y, double Bulge)> ApplyFromOriginAnchor(
        List<(double X, double Y, double Bulge)> loop,
        double eps,
        MachineDirection direction)
    {
        if (loop.Count < 3)
            return loop;

        if (SignedAreaOfBulgeLoop(loop) < 0)
            loop = ReverseBulgeLoop(loop);

        int origin = FindVertexNearOrigin(loop, eps);
        loop = RotateLoop(loop, origin);

        int n = loop.Count;
        var anchor = loop[0];
        var next = loop[1];
        var prev = loop[n - 1];

        bool nextIsBase = IsBaseEdgeFromAnchor(anchor, next, direction, eps);
        bool prevIsBase = IsBaseEdgeFromAnchor(anchor, prev, direction, eps);

        if (prevIsBase && !nextIsBase)
            loop = ReverseLoopKeepingFirstVertex(loop);
        else if (!nextIsBase && !prevIsBase)
            loop = ApplyStandard(loop, eps, direction);
        else if (nextIsBase && prevIsBase)
        {
            // İki yatay kenar birleşimi (nadir): makine yönüne uygun olanı seç.
            double nextDx = next.X - anchor.X;
            double prevDx = prev.X - anchor.X;
            bool pickNext = direction == MachineDirection.LeftToRight
                ? nextDx >= prevDx
                : nextDx <= prevDx;
            if (!pickNext)
                loop = ReverseLoopKeepingFirstVertex(loop);
        }

        return loop;
    }

    private static bool IsBaseEdgeFromAnchor(
        (double X, double Y, double Bulge) anchor,
        (double X, double Y, double Bulge) neighbor,
        MachineDirection direction,
        double eps)
    {
        if (!IsHorizontalLine(anchor.X, anchor.Y, neighbor.X, neighbor.Y, 0, eps))
            return false;

        double dx = neighbor.X - anchor.X;
        return direction == MachineDirection.LeftToRight
            ? dx > eps
            : dx < -eps;
    }

    /// <summary>
    /// İlk köşe sabit kalır; taban kenarı önce gelecek şekilde dönüş yönünü çevirir.
    /// </summary>
    private static List<(double X, double Y, double Bulge)> ReverseLoopKeepingFirstVertex(
        List<(double X, double Y, double Bulge)> loop)
    {
        int n = loop.Count;
        if (n < 3)
            return loop;

        var rev = new List<(double X, double Y, double Bulge)>(n);
        rev.Add((loop[0].X, loop[0].Y, -loop[n - 1].Bulge));
        for (int i = n - 1; i >= 1; i--)
            rev.Add((loop[i].X, loop[i].Y, i > 1 ? -loop[i - 1].Bulge : -loop[0].Bulge));

        return rev;
    }

    private static int FindVertexNearOrigin(List<(double X, double Y, double Bulge)> loop, double eps)
    {
        double originEps = Math.Max(eps, 1e-4);
        for (int i = 0; i < loop.Count; i++)
        {
            if (Math.Abs(loop[i].X) <= originEps && Math.Abs(loop[i].Y) <= originEps)
                return i;
        }

        int best = 0;
        double bestDist = double.PositiveInfinity;
        for (int i = 0; i < loop.Count; i++)
        {
            double d = loop[i].X * loop[i].X + loop[i].Y * loop[i].Y;
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }

        return best;
    }

    public static List<(double X, double Y, double Bulge)> Apply(
        List<(double X, double Y, double Bulge)> loop,
        double eps,
        MachineDirection direction)
        => ApplyStandard(loop, eps, direction);

    private static List<(double X, double Y, double Bulge)> ApplyStandard(
        List<(double X, double Y, double Bulge)> loop,
        double eps,
        MachineDirection direction)
    {
        if (loop.Count < 3)
            return loop;

        if (SignedAreaOfBulgeLoop(loop) < 0)
            loop = ReverseBulgeLoop(loop);

        if (direction == MachineDirection.RightToLeft)
            loop = ReverseBulgeLoop(loop);

        int start = FindStartVertexIndex(loop, direction, eps);
        return RotateLoop(loop, start);
    }

    public static bool IsHorizontalLine(
        double x0, double y0, double x1, double y1, double bulge, double eps)
    {
        if (Math.Abs(bulge) > eps)
            return false;

        double dx = x1 - x0;
        double dy = y1 - y0;
        double len = Math.Sqrt(dx * dx + dy * dy);
        if (len < eps)
            return false;

        return Math.Abs(dy) <= Math.Max(eps, len * 1e-4);
    }

    /// <summary>LINE/ARC grafiğinde izlenecek kenar yönü (başlangıç köşesi makine yönüne göre).</summary>
    public static bool ShouldTraceForward(
        double x0, double y0, double x1, double y1, MachineDirection direction, double eps)
    {
        if (!IsHorizontalLine(x0, y0, x1, y1, 0, eps))
            return direction != MachineDirection.RightToLeft;

        bool leftIsStart = x0 <= x1;
        return direction == MachineDirection.LeftToRight ? leftIsStart : !leftIsStart;
    }

    private static int FindStartVertexIndex(
        List<(double X, double Y, double Bulge)> loop,
        MachineDirection direction,
        double eps)
    {
        int n = loop.Count;
        int bestEdge = -1;
        double bestY = double.PositiveInfinity;
        double bestSpan = double.NegativeInfinity;

        for (int i = 0; i < n; i++)
        {
            var p0 = loop[i];
            var p1 = loop[(i + 1) % n];
            if (!IsHorizontalLine(p0.X, p0.Y, p1.X, p1.Y, p0.Bulge, eps))
                continue;

            double y = Math.Min(p0.Y, p1.Y);
            double span = Math.Abs(p1.X - p0.X);
            if (y < bestY - eps || (Math.Abs(y - bestY) <= eps && span > bestSpan))
            {
                bestY = y;
                bestSpan = span;
                bestEdge = i;
            }
        }

        if (bestEdge >= 0)
            return StartVertexOnHorizontalEdge(loop, bestEdge, direction, eps);

        return FindFallbackStartVertex(loop, direction, eps);
    }

    private static int StartVertexOnHorizontalEdge(
        List<(double X, double Y, double Bulge)> loop,
        int edgeIndex,
        MachineDirection direction,
        double eps)
    {
        var p0 = loop[edgeIndex];
        var p1 = loop[(edgeIndex + 1) % loop.Count];

        if (direction == MachineDirection.LeftToRight)
            return p0.X <= p1.X + eps ? edgeIndex : (edgeIndex + 1) % loop.Count;

        return p0.X >= p1.X - eps ? edgeIndex : (edgeIndex + 1) % loop.Count;
    }

    private static int FindFallbackStartVertex(
        List<(double X, double Y, double Bulge)> loop,
        MachineDirection direction,
        double eps)
    {
        int best = 0;
        for (int i = 1; i < loop.Count; i++)
        {
            if (CompareFallback(loop[i], loop[best], direction, eps) < 0)
                best = i;
        }

        return best;
    }

    private static int CompareFallback(
        (double X, double Y, double Bulge) a,
        (double X, double Y, double Bulge) b,
        MachineDirection direction,
        double eps)
    {
        if (Math.Abs(a.Y - b.Y) > eps)
            return a.Y < b.Y ? -1 : 1;

        if (direction == MachineDirection.LeftToRight)
        {
            if (Math.Abs(a.X - b.X) > eps)
                return a.X < b.X ? -1 : 1;
        }
        else
        {
            if (Math.Abs(a.X - b.X) > eps)
                return a.X > b.X ? -1 : 1;
        }

        return 0;
    }

    private static List<(double X, double Y, double Bulge)> RotateLoop(
        List<(double X, double Y, double Bulge)> loop,
        int start)
    {
        if (start == 0)
            return loop;

        var rotated = new List<(double X, double Y, double Bulge)>(loop.Count);
        for (int k = 0; k < loop.Count; k++)
            rotated.Add(loop[(start + k) % loop.Count]);
        return rotated;
    }

    private static List<(double X, double Y, double Bulge)> ReverseBulgeLoop(
        List<(double X, double Y, double Bulge)> loop)
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
}
