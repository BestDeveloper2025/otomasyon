using otomasyon.Geometry;
using otomasyon.Models;

namespace otomasyon.Analysis;

/// <summary>
/// Radius uçlarını (0,0) → CCW yürüyüşte başlangıç/bitiş olacak şekilde hizalar.
/// </summary>
public static class RadiusEndpointOrienter
{
    public static RadiusFeature Orient(RadiusFeature f)
    {
        if (!ShouldSwapForCcw(f.StartX, f.StartY, f.EndX, f.EndY))
            return f;

        return new RadiusFeature
        {
            Index = f.Index,
            SourceLabel = f.SourceLabel,
            CenterX = f.CenterX,
            CenterY = f.CenterY,
            Radius = f.Radius,
            Convexity = f.Convexity,
            StartX = f.EndX,
            StartY = f.EndY,
            EndX = f.StartX,
            EndY = f.StartY,
            Line1DirectionDeg = f.Line2DirectionDeg,
            Line2DirectionDeg = f.Line1DirectionDeg,
            CornerAngleDeg = f.CornerAngleDeg,
            StartEdgeAngleDeg = f.EndEdgeAngleDeg,
            EndEdgeAngleDeg = f.StartEdgeAngleDeg,
            StartCornerAngleDeg = f.EndCornerAngleDeg,
            EndCornerAngleDeg = f.StartCornerAngleDeg,
            StartTangentAngleDeg = f.EndTangentAngleDeg,
            EndTangentAngleDeg = f.StartTangentAngleDeg,
            Bulge = f.Bulge,
            EdgeIndex = f.EdgeIndex,
            CornerIndex = f.CornerIndex
        };
    }

    public static bool ShouldSwapForCcw(double startX, double startY, double endX, double endY)
    {
        double ps = AngleMath.CcwAngleFromPositiveX(startX, startY);
        double pe = AngleMath.CcwAngleFromPositiveX(endX, endY);
        double diff = pe - ps;
        if (diff > Math.PI) diff -= 2.0 * Math.PI;
        if (diff < -Math.PI) diff += 2.0 * Math.PI;
        return diff < -1e-6;
    }
}
