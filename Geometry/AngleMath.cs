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
}
