namespace otomasyon.Geometry;

/// <summary>
/// Radius uç köşelerindeki açı değerlerini hesaplar.
/// </summary>
public static class RadiusCornerAngles
{
    public readonly struct Result
    {
        public double StartEdgeAngleDeg { get; init; }
        public double EndEdgeAngleDeg { get; init; }
        public double StartTangentAngleDeg { get; init; }
        public double EndTangentAngleDeg { get; init; }
        public double StartCornerAngleDeg { get; init; }
        public double EndCornerAngleDeg { get; init; }
        public double VirtualCornerAngleDeg { get; init; }
    }

    public static Result Compute(
        double line1DirectionDeg,
        double line2DirectionDeg,
        double tangentAtStartDeg,
        double tangentAtEndDeg)
    {
        double virtualCorner = AngleMath.OpeningAngleDeg(line1DirectionDeg, line2DirectionDeg);

        return new Result
        {
            StartEdgeAngleDeg = line1DirectionDeg,
            EndEdgeAngleDeg = line2DirectionDeg,
            StartTangentAngleDeg = tangentAtStartDeg,
            EndTangentAngleDeg = tangentAtEndDeg,
            StartCornerAngleDeg = virtualCorner,
            EndCornerAngleDeg = virtualCorner,
            VirtualCornerAngleDeg = virtualCorner
        };
    }
}
