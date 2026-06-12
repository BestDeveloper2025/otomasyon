namespace otomasyon.Models;

/// <summary>
/// Yayın profil içine/dışına göre bükeylik sınıfı (imalat terminolojisi).
/// </summary>
public enum RadiusConvexity
{
    Unknown,
    /// <summary>İç bükey (oyuk) — yay merkezi malzemenin DIŞINDA.</summary>
    IcBubey,
    /// <summary>Dış bükey (çıkık) — yay merkezi malzemenin İÇİNDE (tam daire gibi).</summary>
    DisBubey
}
