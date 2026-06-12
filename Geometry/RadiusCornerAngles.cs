namespace otomasyon.Geometry;

/// <summary>
/// Radius uç köşelerinde: yayın kirişi (başlangıç→bitiş doğrusu) ile bitişik kenar arasındaki açı.
/// Bitişik kenar düz ise kendi yönü, yay ise onun kirişi kullanılır.
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
        double arcChordDirectionDeg,
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

        // Başlangıç köşesi: bir önceki kenar (kirişi) ile yayın kirişi arası.
        // line1DirectionDeg köşeye DOĞRU bakar; +180 köşeden dışarı bakar. Yay kirişi de köşeden dışarı (başlangıç→bitiş).
        double startCorner = AngleMath.InteriorAngleBetweenRaysDeg(
            AngleMath.Normalize360(line1DirectionDeg + 180.0),
            arcChordDirectionDeg,
            arcStartX, arcStartY, materialCenterX, materialCenterY);

        // Bitiş köşesi: yayın kirişi ile bir sonraki kenar (kirişi) arası.
        // Köşeden dışarı: yay kirişi ters (chordDir+180), sonraki kenar (line2 köşeye bakar, +180 dışarı).
        double endCorner = AngleMath.InteriorAngleBetweenRaysDeg(
            AngleMath.Normalize360(arcChordDirectionDeg + 180.0),
            AngleMath.Normalize360(line2DirectionDeg + 180.0),
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
