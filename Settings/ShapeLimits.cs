namespace otomasyon.Settings;

public sealed class ShapeLimits
{
    public double MaxWidthMm { get; init; }
    public double MaxHeightMm { get; init; }

    public bool IsValid => MaxWidthMm > 0 && MaxHeightMm > 0;
}
