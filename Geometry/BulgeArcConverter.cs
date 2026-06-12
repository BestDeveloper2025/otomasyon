using netDxf;
using netDxf.Entities;

namespace otomasyon.Geometry;

/// <summary>
/// DXF bulge değerinden yay parametrelerine dönüşüm (netDxf MathHelper).
/// </summary>
public static class BulgeArcConverter
{
    public static bool TryFromBulge(
        double startX, double startY,
        double endX, double endY,
        double bulge,
        out double centerX, out double centerY,
        out double radius,
        out double startAngleDeg,
        out double endAngleDeg)
    {
        centerX = centerY = radius = startAngleDeg = endAngleDeg = 0;

        if (Math.Abs(bulge) < 1e-12)
            return false;

        var tuple = MathHelper.ArcFromBulge(
            new Vector2(startX, startY),
            new Vector2(endX, endY),
            bulge);

        var center = tuple.Item1;
        centerX = center.X;
        centerY = center.Y;
        radius = tuple.Item2;
        startAngleDeg = tuple.Item3;
        endAngleDeg = tuple.Item4;
        return radius > 1e-12;
    }

    public static bool TryFromArcEntity(Arc arc,
        out double centerX, out double centerY,
        out double radius,
        out double startAngleDeg,
        out double endAngleDeg,
        out double bulge)
    {
        centerX = arc.Center.X;
        centerY = arc.Center.Y;
        radius = arc.Radius;
        startAngleDeg = arc.StartAngle;
        endAngleDeg = arc.EndAngle;

        // DXF ARC her zaman başlangıç→bitiş CCW yönündedir. Bitiş açısı başlangıçtan
        // küçükse (360° sarması) süpürmeyi normalize etmeden hesaplanan bulge yanlış
        // (tümleyen yay) çıkar. Bu yüzden CCW süpürmeyi normalize edip bulge'u doğrudan üretiyoruz.
        double sweepRad = ArcSampler.GetCcwSweepRadians(startAngleDeg, endAngleDeg);
        bulge = Math.Tan(sweepRad / 4.0);
        return radius > 1e-12;
    }
}
