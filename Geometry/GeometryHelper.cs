namespace otomasyon.Geometry;

/// <summary>
/// Ortak geometri yardımcı metotları (nokta yakınlık, signed area vb.).
/// </summary>
public static class GeometryHelper
{
    /// <summary>İki noktanın bileşen bazında tolerans içinde yakın olup olmadığını kontrol eder.</summary>
    public static bool PointsNear(double x1, double y1, double x2, double y2, double eps)
        => Math.Abs(x1 - x2) <= eps && Math.Abs(y1 - y2) <= eps;

    /// <summary>Basit çokgenin (kapalı köşe dizisi) işaretli alanı (Shoelace). Pozitif = CCW.</summary>
    public static double SignedArea(IReadOnlyList<(double X, double Y)> vertices)
    {
        int n = vertices.Count;
        if (n < 3) return 0;

        double sum = 0;
        for (int i = 0; i < n; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % n];
            sum += a.X * b.Y - b.X * a.Y;
        }

        return sum * 0.5;
    }

    /// <summary>Bulge'li köşe dizisinin işaretli alanı.</summary>
    public static double SignedAreaOfBulgeLoop(IReadOnlyList<(double X, double Y, double Bulge)> vertices)
    {
        int n = vertices.Count;
        if (n < 3) return 0;

        double sum = 0;
        for (int i = 0; i < n; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % n];
            sum += a.X * b.Y - b.X * a.Y;
        }

        return sum * 0.5;
    }

    /// <summary>
    /// Even-odd (ışın yöntemi) nokta-poligon içi testi. Yön (CW/CCW) bağımsızdır.
    /// </summary>
    public static bool PointInPolygon(IReadOnlyList<(double X, double Y)> poly, double px, double py)
    {
        int n = poly.Count;
        if (n < 3) return false;

        bool inside = false;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            double xi = poly[i].X, yi = poly[i].Y;
            double xj = poly[j].X, yj = poly[j].Y;

            bool crosses = (yi > py) != (yj > py);
            if (crosses)
            {
                double xCross = (xj - xi) * (py - yi) / (yj - yi) + xi;
                if (px < xCross)
                    inside = !inside;
            }
        }

        return inside;
    }

    /// <summary>
    /// Bir kenarı (düz veya bulge yay) poligon nokta listesine ekler.
    /// Yalnızca başlangıç + yay ara noktaları eklenir (bitiş, sıradaki kenarın başlangıcıdır).
    /// </summary>
    public static void AppendSegmentSamples(
        List<(double X, double Y)> pts,
        double sx, double sy, double ex, double ey, double bulge)
    {
        pts.Add((sx, sy));

        if (Math.Abs(bulge) < 1e-12)
            return;

        if (!BulgeArcConverter.TryFromBulge(sx, sy, ex, ey, bulge,
                out double cx, out double cy, out double r, out _, out _))
            return;

        double startAng = Math.Atan2(sy - cy, sx - cx);
        double theta = 4.0 * Math.Atan(bulge);
        int steps = Math.Max(2, (int)Math.Ceiling(Math.Abs(theta) / (Math.PI / 18.0)));

        for (int k = 1; k < steps; k++)
        {
            double a = startAng + theta * k / steps;
            pts.Add((cx + r * Math.Cos(a), cy + r * Math.Sin(a)));
        }
    }
}
