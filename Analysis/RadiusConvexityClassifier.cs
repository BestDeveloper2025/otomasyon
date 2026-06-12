using otomasyon.Models;

namespace otomasyon.Analysis;

/// <summary>
/// Yay iç/dış bükeylik sınıflandırması.
/// Kural: kontur CCW yürünürken yay sola dönüyorsa (şeklin dışına doğru bükülür) DIŞ bükey,
/// sağa dönüyorsa (şeklin içine doğru bükülür) İÇ bükey. CW kontur için işaret terslenir.
/// </summary>
public static class RadiusConvexityClassifier
{
    /// <summary>
    /// Yürüyüş yönüne göre düzeltilmiş bulge ile sınıflar.
    /// travelBulge &gt; 0 (sola dönüş, şeklin dışına) → dış bükey; &lt; 0 → iç bükey.
    /// </summary>
    public static RadiusConvexity ClassifyForCcwTraversal(double travelBulge)
    {
        if (Math.Abs(travelBulge) < 1e-12)
            return RadiusConvexity.Unknown;

        return travelBulge > 0 ? RadiusConvexity.DisBubey : RadiusConvexity.IcBubey;
    }

    /// <summary>
    /// Kontur yönünü (CCW/CW) hesaba katarak sınıflar.
    /// </summary>
    public static RadiusConvexity Classify(double bulge, bool contourIsCcw)
    {
        double travelBulge = contourIsCcw ? bulge : -bulge;
        return ClassifyForCcwTraversal(travelBulge);
    }

    public static string ToDisplayName(RadiusConvexity c) => c switch
    {
        RadiusConvexity.IcBubey => "İç bükey",
        RadiusConvexity.DisBubey => "Dış bükey",
        _ => "Belirsiz"
    };
}
