namespace otomasyon.Models;

/// <summary>
/// Yayın profil içine/dışına göre bükeylik sınıfı (imalat terminolojisi).
/// </summary>
public enum RadiusConvexity
{
    Unknown,
    /// <summary>İç bükey — yay merkezi malzeme tarafında / profil içinde.</summary>
    IcBubey,
    /// <summary>Dış bükey — yay merkezi malzeme dışında / profil dışında.</summary>
    DisBubey
}
