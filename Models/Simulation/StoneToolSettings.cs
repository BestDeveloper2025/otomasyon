namespace otomasyon.Models.Simulation;

/// <summary>Taş (takım) boyutu ve bindirme (geçişler arası örtüşme, mm).</summary>
public sealed class StoneToolSettings
{
    public double StoneWidthMm { get; init; }
    public double BindirmeMm { get; init; }

    public void Validate()
    {
        if (StoneWidthMm <= 0)
            throw new ArgumentOutOfRangeException(nameof(StoneWidthMm), "Taş genişliği 0'dan büyük olmalıdır.");
        if (BindirmeMm < 0)
            throw new ArgumentOutOfRangeException(nameof(BindirmeMm), "Bindirme negatif olamaz.");
        if (BindirmeMm >= StoneWidthMm)
            throw new ArgumentOutOfRangeException(nameof(BindirmeMm), "Bindirme, taş genişliğinden küçük olmalıdır.");
    }
}
