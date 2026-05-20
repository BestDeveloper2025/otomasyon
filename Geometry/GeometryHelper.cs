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
}
