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

        var tuple = MathHelper.ArcToBulge(
            new Vector2(centerX, centerY),
            radius,
            startAngleDeg,
            endAngleDeg);

        bulge = tuple.Item3;
        return radius > 1e-12;
    }
}
