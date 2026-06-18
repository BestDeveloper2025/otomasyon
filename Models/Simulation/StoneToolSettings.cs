using otomasyon.Localization;

namespace otomasyon.Models.Simulation;

/// <summary>Taş (takım) boyutu ve bindirme (geçişler arası örtüşme, mm).</summary>
public sealed class StoneToolSettings
{
    public double StoneWidthMm { get; init; }
    public double BindirmeMm { get; init; }

    public void Validate()
    {
        if (StoneWidthMm <= 0)
            throw new ArgumentOutOfRangeException(nameof(StoneWidthMm), L.Get("Error.StoneWidthPositive"));
        if (BindirmeMm < 0)
            throw new ArgumentOutOfRangeException(nameof(BindirmeMm), L.Get("Error.OverlapNonNegative"));
        if (BindirmeMm >= StoneWidthMm)
            throw new ArgumentOutOfRangeException(nameof(BindirmeMm), L.Get("Error.OverlapLessThanStone"));
    }
}
