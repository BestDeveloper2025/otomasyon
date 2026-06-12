namespace otomasyon.Geometry;

/// <summary>
/// Radius uç köşelerinde dönüş (sapma) açısı: ardışık kirişlerin gidiş yönleri arasındaki açı.
/// İki kiriş aynı doğrultudaysa (düz devam) açı 0'dır.
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

    /// <param name="line1DirectionDeg">Önceki kenarın gidiş yönü (köşeye doğru).</param>
    /// <param name="line2DirectionDeg">Sonraki kenarın köşeye doğru yönü (gidişin tersi).</param>
    /// <param name="arcChordDirectionDeg">Yay kirişinin gidiş yönü (başlangıç→bitiş).</param>
    public static Result Compute(
        double line1DirectionDeg,
        double line2DirectionDeg,
        double arcChordDirectionDeg,
        double tangentAtStartDeg,
        double tangentAtEndDeg)
    {
        double virtualCorner = AngleMath.OpeningAngleDeg(line1DirectionDeg, line2DirectionDeg);

        // Başlangıç: önceki kirişin gidişi ile yay kirişinin gidişi arasındaki sapma. Düz ise 0.
        double startCorner = AngleMath.OpeningAngleDeg(line1DirectionDeg, arcChordDirectionDeg);

        // Bitiş: yay kirişinin gidişi ile sonraki kirişin gidişi (line2 köşeye bakar; +180 gidiş yönü) arası.
        double nextTravel = AngleMath.Normalize360(line2DirectionDeg + 180.0);
        double endCorner = AngleMath.OpeningAngleDeg(arcChordDirectionDeg, nextTravel);

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
