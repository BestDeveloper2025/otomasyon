using netDxf.Entities;

namespace otomasyon.Geometry;

/// <summary>
/// Yay ve daire örneklemesi; DXF açıları (derece) ile çalışır.
/// </summary>
public static class ArcSampler
{
    public static void GetArcEndpointXY(Arc arc, out double startX, out double startY, out double endX, out double endY)
    {
        double cx = arc.Center.X, cy = arc.Center.Y, r = arc.Radius;
        double s = arc.StartAngle * Math.PI / 180.0;
        double e = arc.EndAngle * Math.PI / 180.0;
        startX = cx + r * Math.Cos(s);
        startY = cy + r * Math.Sin(s);
        endX = cx + r * Math.Cos(e);
        endY = cy + r * Math.Sin(e);
    }

    public static double GetCcwSweepRadians(double startDeg, double endDeg)
    {
        double s = startDeg * Math.PI / 180.0;
        double e = endDeg * Math.PI / 180.0;
        double sweep = e - s;
        while (sweep <= 0) sweep += 2 * Math.PI;
        while (sweep > 2 * Math.PI) sweep -= 2 * Math.PI;
        if (sweep < 1e-9 && Math.Abs(endDeg - startDeg) > 1e-6)
            sweep = 2 * Math.PI;
        return sweep;
    }

    public static List<(double X, double Y)> SampleArcPoints(Arc arc, int segments)
    {
        var list = new List<(double X, double Y)>();
        double start = arc.StartAngle * Math.PI / 180.0;
        double sweep = GetCcwSweepRadians(arc.StartAngle, arc.EndAngle);
        double cx = arc.Center.X;
        double cy = arc.Center.Y;
        double r = arc.Radius;

        for (int i = 0; i <= segments; i++)
        {
            double t = i / (double)segments;
            double a = start + sweep * t;
            list.Add((cx + r * Math.Cos(a), cy + r * Math.Sin(a)));
        }

        return list;
    }

    public static List<(double X, double Y)> SampleCirclePoints(Circle circle, int segments)
    {
        var list = new List<(double X, double Y)>();
        double cx = circle.Center.X;
        double cy = circle.Center.Y;
        double r = circle.Radius;
        for (int i = 0; i < segments; i++)
        {
            double a = i * 2.0 * Math.PI / segments;
            list.Add((cx + r * Math.Cos(a), cy + r * Math.Sin(a)));
        }

        return list;
    }
}
