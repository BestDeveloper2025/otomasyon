namespace otomasyon.Geometry;

/// <summary>
/// Dünya XY düzleminde açı ve yön hesapları (derece).
/// </summary>
public static class AngleMath
{
    public const double RadToDeg = 180.0 / Math.PI;

    public static double DirectionDeg(double dx, double dy)
    {
        if (Math.Abs(dx) < 1e-12 && Math.Abs(dy) < 1e-12)
            return double.NaN;

        double rad = Math.Atan2(dy, dx);
        double deg = rad * RadToDeg;
        if (deg < 0)
            deg += 360.0;
        return deg;
    }

    /// <summary>İki yön arasındaki küçük açı (0–180 derece).</summary>
    public static double OpeningAngleDeg(double dir1Deg, double dir2Deg)
    {
        if (double.IsNaN(dir1Deg) || double.IsNaN(dir2Deg))
            return double.NaN;

        double d = Math.Abs(Normalize360(dir2Deg) - Normalize360(dir1Deg));
        if (d > 180.0)
            d = 360.0 - d;
        return d;
    }

    public static double Normalize360(double deg)
    {
        double d = deg % 360.0;
        if (d < 0)
            d += 360.0;
        return d;
    }

    /// <summary>DXF yayında CCW ilerleme için uç noktadaki teğet yönü (derece).</summary>
    public static double ArcTangentDeg(double angleFromCenterDeg, bool ccwAlongArc)
        => Normalize360(angleFromCenterDeg + (ccwAlongArc ? 90.0 : -90.0));

    /// <summary>
    /// İki ışın arasında, referans noktası (malzeme içi) hangi dilimdeyse o dilimin açısı.
    /// Teğet–kenar köşesinde geniş veya dar açıyı doğru seçer.
    /// </summary>
    public static double InteriorAngleBetweenRaysDeg(
        double ray1DirDeg,
        double ray2DirDeg,
        double cornerX,
        double cornerY,
        double interiorRefX,
        double interiorRefY)
    {
        if (double.IsNaN(ray1DirDeg) || double.IsNaN(ray2DirDeg))
            return double.NaN;

        double from = Normalize360(ray1DirDeg);
        double to = Normalize360(ray2DirDeg);
        double sweepCcw = to - from;
        if (sweepCcw < 0)
            sweepCcw += 360.0;

        double sweepCw = 360.0 - sweepCcw;
        if (sweepCcw < 1e-6 || sweepCw < 1e-6)
            return 0;

        double refDir = DirectionDeg(interiorRefX - cornerX, interiorRefY - cornerY);
        if (double.IsNaN(refDir))
            return Math.Min(sweepCcw, sweepCw);

        if (IsInWedgeCcw(from, sweepCcw, refDir))
            return sweepCcw;

        return sweepCw;
    }

    private static bool IsInWedgeCcw(double fromDeg, double sweepDeg, double testDeg)
    {
        double rel = Normalize360(testDeg) - fromDeg;
        if (rel < 0)
            rel += 360.0;
        return rel <= sweepDeg + 1e-3;
    }

    /// <summary>(0,0) referansından +X eksenine göre CCW açı (radyan, 0–2π).</summary>
    public static double CcwAngleFromPositiveX(double x, double y)
    {
        double a = Math.Atan2(y, x);
        if (a < 0) a += 2.0 * Math.PI;
        return a;
    }
}
