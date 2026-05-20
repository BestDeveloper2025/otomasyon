namespace otomasyon.Geometry;

/// <summary>
/// Radius uç köşelerinde: teğet çizgisi ile bitişik düz kenar arasındaki açı.
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
        double tangentAtEndDeg,
        double arcStartX,
        double arcStartY,
        double arcEndX,
        double arcEndY,
        double materialCenterX,
        double materialCenterY)
    {
        double virtualCorner = AngleMath.OpeningAngleDeg(line1DirectionDeg, line2DirectionDeg);

        double lineOutAtStart = AngleMath.Normalize360(line1DirectionDeg + 180.0);
        double arcBackAtEnd = AngleMath.DirectionDeg(arcStartX - arcEndX, arcStartY - arcEndY);

        double startCorner = AngleMath.InteriorAngleBetweenRaysDeg(
            lineOutAtStart, tangentAtStartDeg,
            arcStartX, arcStartY, materialCenterX, materialCenterY);

        double endCorner = AngleMath.InteriorAngleBetweenRaysDeg(
            arcBackAtEnd, line2DirectionDeg,
            arcEndX, arcEndY, materialCenterX, materialCenterY);

        return new Result
        {
            StartEdgeAngleDeg = line1DirectionDeg,
            EndEdgeAngleDeg = line2DirectionDeg,
            StartTangentAngleDeg = tangentAtStartDeg,
            EndTangentAngleDeg = tangentAtEndDeg,
            StartCornerAngleDeg = startCorner,
            EndCornerAngleDeg = endCorner,
            VirtualCornerAngleDeg = virtualCorner
        };
    }
}
