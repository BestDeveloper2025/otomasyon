namespace otomasyon.Geometry;

/// <summary>Düz kenar ve bulge yay uzunluğu (mm, DXF dünya birimi).</summary>
public static class SegmentLength
{
    public static double LineMm(double x0, double y0, double x1, double y1)
    {
        double dx = x1 - x0, dy = y1 - y0;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>Bulge yay: |b| = tan(θ/4) → yay uzunluğu = R·4·atan(|b|).</summary>
    public static double FromBulgeMm(double x0, double y0, double x1, double y1, double bulge)
    {
        double chord = LineMm(x0, y0, x1, y1);
        if (chord < 1e-12)
            return 0;

        if (Math.Abs(bulge) < 1e-12)
            return chord;

        if (!BulgeArcConverter.TryFromBulge(x0, y0, x1, y1, bulge, out _, out _, out double radius, out _, out _))
            return chord;

        return radius * 4.0 * Math.Atan(Math.Abs(bulge));
    }
}
