using otomasyon.Geometry;
using otomasyon.Settings;

namespace otomasyon.Analysis;

/// <summary>
/// Seçilen kenarı yatay taban yapar; iç bölge yukarıda kalacak şekilde gerekirse 180° çevirir;
/// makine yönüne göre sol veya sağ köşeyi (0,0)'a taşır.
/// </summary>
public static class BaseEdgeOrientator
{
    public static AffineTransform2D ComputeTransform(
        ContourPathOrderer.OrderedSegment edge,
        IReadOnlyList<(double X, double Y)> samplePoints,
        MachineDirection direction)
    {
        double sx = edge.StartX;
        double sy = edge.StartY;
        double ex = edge.EndX;
        double ey = edge.EndY;

        double rot = -Math.Atan2(ey - sy, ex - sx);
        bool negateBulge = false;

        if (NeedsFlip180(sx, sy, ex, ey, samplePoints, rot))
        {
            rot += Math.PI;
            negateBulge = true;
        }

        var rs = Rotate(sx, sy, rot);
        var re = Rotate(ex, ey, rot);

        double anchorX;
        double anchorY;
        if (direction == MachineDirection.LeftToRight)
        {
            if (rs.X <= re.X)
            {
                anchorX = rs.X;
                anchorY = rs.Y;
            }
            else
            {
                anchorX = re.X;
                anchorY = re.Y;
            }
        }
        else
        {
            if (rs.X >= re.X)
            {
                anchorX = rs.X;
                anchorY = rs.Y;
            }
            else
            {
                anchorX = re.X;
                anchorY = re.Y;
            }
        }

        return AffineTransform2D.FromRotationAndTranslation(rot, -anchorX, -anchorY, negateBulge);
    }

    private static bool NeedsFlip180(
        double sx, double sy, double ex, double ey,
        IReadOnlyList<(double X, double Y)> points,
        double rot)
    {
        if (points.Count == 0)
            return false;

        var rs = Rotate(sx, sy, rot);
        var re = Rotate(ex, ey, rot);
        double edgeY = (rs.Y + re.Y) * 0.5;

        double cx = 0;
        double cy = 0;
        foreach (var p in points)
        {
            var r = Rotate(p.X, p.Y, rot);
            cx += r.X;
            cy += r.Y;
        }

        cx /= points.Count;
        cy /= points.Count;
        return cy < edgeY;
    }

    private static (double X, double Y) Rotate(double x, double y, double radians)
    {
        double c = Math.Cos(radians);
        double s = Math.Sin(radians);
        return (c * x - s * y, s * x + c * y);
    }
}
