using otomasyon.Models;

namespace otomasyon.Analysis;

/// <summary>
/// Bulge işareti ve profil yönüne göre iç/dış bükeylik sınıflandırması.
/// </summary>
public static class RadiusConvexityClassifier
{
    /// <summary>
    /// CCW dış kontur yürüyüşü: pozitif bulge → iç bükey; negatif bulge → dış bükey.
    /// </summary>
    public static RadiusConvexity ClassifyForCcwTraversal(double bulge)
    {
        if (Math.Abs(bulge) < 1e-12)
            return RadiusConvexity.Unknown;

        return bulge > 0 ? RadiusConvexity.IcBubey : RadiusConvexity.DisBubey;
    }

    public static string ToDisplayName(RadiusConvexity c) => c switch
    {
        RadiusConvexity.IcBubey => "İç bükey",
        RadiusConvexity.DisBubey => "Dış bükey",
        _ => "Belirsiz"
    };
}
