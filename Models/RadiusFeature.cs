namespace otomasyon.Models;

/// <summary>
/// Tek bir radiuslu kenar (yay veya polyline bulge segmenti) için analiz sonucu.
/// </summary>
public sealed class RadiusFeature
{
    public int Index { get; init; }
    public string SourceLabel { get; init; } = string.Empty;

    public double CenterX { get; init; }
    public double CenterY { get; init; }
    public double Radius { get; init; }
    public RadiusConvexity Convexity { get; init; }

    public double StartX { get; init; }
    public double StartY { get; init; }
    public double EndX { get; init; }
    public double EndY { get; init; }

    /// <summary>Yay başındaki bitişik düz kenar yönü (derece, 0–360).</summary>
    public double Line1DirectionDeg { get; init; }

    /// <summary>Yay sonundaki bitişik düz kenar yönü (derece, 0–360).</summary>
    public double Line2DirectionDeg { get; init; }

    /// <summary>İki düz kenar arasındaki sanal köşe açısı (derece, 0–180).</summary>
    public double CornerAngleDeg { get; init; }

    /// <summary>Yay başlangıç noktasında düz kenar yön açısı (+X referans, 0–360).</summary>
    public double StartEdgeAngleDeg { get; init; }

    /// <summary>Yay bitiş noktasında düz kenar yön açısı.</summary>
    public double EndEdgeAngleDeg { get; init; }

    /// <summary>Yay başlangıç köşesindeki profil köşe açısı (düz kenarlar arası).</summary>
    public double StartCornerAngleDeg { get; init; }

    /// <summary>Yay bitiş köşesindeki profil köşe açısı.</summary>
    public double EndCornerAngleDeg { get; init; }

    public double StartTangentAngleDeg { get; init; }
    public double EndTangentAngleDeg { get; init; }

    public double Bulge { get; init; }

    public int EdgeIndex { get; init; }
    public int CornerIndex { get; init; }
}
