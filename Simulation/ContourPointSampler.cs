using otomasyon.Geometry;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

/// <summary>Kenar boyunca mesafe (mm); yön CCW kontur (başlangıç → bitiş).</summary>
public static class ContourPointSampler
{
    public readonly struct Sample
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double TangentDirDeg { get; init; }
        public double InwardNormalDirDeg { get; init; }
    }

    public static bool TrySampleOnSegment(ContourPathSegment segment, double distanceAlongMm, out Sample sample)
    {
        sample = default;
        if (segment.LengthMm < 1e-9)
            return false;

        double t = Math.Clamp(distanceAlongMm / segment.LengthMm, 0, 1);

        if (segment.IsArc
            && segment.Radius.HasValue
            && segment.CenterX.HasValue
            && segment.CenterY.HasValue)
        {
            double r = segment.Radius.Value;
            double cx = segment.CenterX.Value;
            double cy = segment.CenterY.Value;
            
            double startAngRad = Math.Atan2(segment.StartY - cy, segment.StartX - cx);
            double theta = 4.0 * Math.Atan(segment.Bulge);
            double angRad = startAngRad + t * theta;
            
            double px = cx + r * Math.Cos(angRad);
            double py = cy + r * Math.Sin(angRad);
            bool ccwAlong = segment.Bulge > 0;
            double tangent = AngleMath.ArcTangentDeg(angRad * 180.0 / Math.PI, ccwAlong);
            return Finish(px, py, tangent, out sample);
        }

        double x = segment.StartX + t * (segment.EndX - segment.StartX);
        double y = segment.StartY + t * (segment.EndY - segment.StartY);
        double dir = AngleMath.DirectionDeg(segment.EndX - segment.StartX, segment.EndY - segment.StartY);
        return Finish(x, y, dir, out sample);
    }

    public static bool TryMidpoint(ContourPathSegment segment, out double midX, out double midY)
    {
        midX = midY = 0;
        if (!TrySampleOnSegment(segment, segment.LengthMm * 0.5, out var s))
            return false;
        midX = s.X;
        midY = s.Y;
        return true;
    }

    private static bool Finish(double x, double y, double tangentDirDeg, out Sample sample)
    {
        sample = default;
        if (double.IsNaN(tangentDirDeg))
            return false;

        double inward = AngleMath.Normalize360(tangentDirDeg + 90.0);
        sample = new Sample
        {
            X = x,
            Y = y,
            TangentDirDeg = tangentDirDeg,
            InwardNormalDirDeg = inward
        };
        return true;
    }
}
