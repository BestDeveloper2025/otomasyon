using otomasyon.Geometry;
using otomasyon.Models;

namespace otomasyon.Analysis;

/// <summary>
/// Yay iç/dış bükeylik sınıflandırması.
/// Kesin kural: yay merkezi malzeme (kapalı kontur) içindeyse DIŞ bükey,
/// dışındaysa İÇ bükey. (Tam daire = merkez içeride = dış bükey.)
/// </summary>
public static class RadiusConvexityClassifier
{
    /// <summary>
    /// Yayın merkezinin, örneklenmiş kontur poligonunun içinde olup olmamasına göre sınıflar.
    /// Bu test kontur yönünden (CW/CCW) bağımsızdır; her şekilde geçerlidir.
    /// </summary>
    public static RadiusConvexity ClassifyByCenter(
        IReadOnlyList<(double X, double Y)> contourPolygon,
        double centerX, double centerY)
    {
        if (contourPolygon is null || contourPolygon.Count < 3)
            return RadiusConvexity.Unknown;

        if (double.IsNaN(centerX) || double.IsNaN(centerY))
            return RadiusConvexity.Unknown;

        bool centerInside = GeometryHelper.PointInPolygon(contourPolygon, centerX, centerY);
        return centerInside ? RadiusConvexity.DisBubey : RadiusConvexity.IcBubey;
    }

    /// <summary>
    /// Yedek (poligon kurulamazsa): CCW konturda yay sola dönerse (bulge &gt; 0) dış bükey,
    /// sağa dönerse iç bükey.
    /// </summary>
    public static RadiusConvexity ClassifyForCcwTraversal(double bulge)
    {
        if (Math.Abs(bulge) < 1e-12)
            return RadiusConvexity.Unknown;

        return bulge > 0 ? RadiusConvexity.DisBubey : RadiusConvexity.IcBubey;
    }

    public static string ToDisplayName(RadiusConvexity c) => c switch
    {
        RadiusConvexity.IcBubey => "İç bükey",
        RadiusConvexity.DisBubey => "Dış bükey",
        _ => "Belirsiz"
    };
}
