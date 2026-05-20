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
        
        // line2DirectionDeg points backwards from the next segment into the end junction.
        // So lineIntoAtEnd is literally line2DirectionDeg.
        double lineIntoAtEnd = line2DirectionDeg;

        // Interior angle of a CCW contour at any corner: Normalize360(vBackwards - vForwards)
        // At start junction: vBackwards = lineOutAtStart, vForwards = tangentAtStartDeg
        double startCorner = AngleMath.Normalize360(lineOutAtStart - tangentAtStartDeg);

        // At end junction: vBackwards = tangentAtEndDeg + 180
        // vForwards = lineIntoAtEnd + 180 (because it points backwards, adding 180 points forward)
        // Interior = (tangentAtEndDeg + 180) - (lineIntoAtEnd + 180) = tangentAtEndDeg - lineIntoAtEnd
        double endCorner = AngleMath.Normalize360(tangentAtEndDeg - lineIntoAtEnd);

        double tangentIntoArcAtEnd = AngleMath.Normalize360(tangentAtEndDeg + 180.0);

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
