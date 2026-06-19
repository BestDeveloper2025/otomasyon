using otomasyon.Localization;
using otomasyon.Models;
using otomasyon.Settings;

namespace otomasyon.Analysis;

public static class ShapeLimitsValidator
{
    public static bool IsWithinLimits(DxfScene scene)
        => TryValidate(scene, out _);

    public static bool TryValidate(DxfScene scene, out string? message)
    {
        message = null;
        if (!AppSettingsManager.IsConfigured || !AppSettingsManager.Limits.IsValid)
            return true;

        if (!scene.Bounds.HasBounds)
            return true;

        double width = scene.Bounds.Width;
        double height = scene.Bounds.Height;
        double maxW = AppSettingsManager.Limits.MaxWidthMm;
        double maxH = AppSettingsManager.Limits.MaxHeightMm;

        if (width <= maxW + 1e-6 && height <= maxH + 1e-6)
            return true;

        message = L.F("Msg.ShapeLimitExceeded",
            width.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
            height.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
            maxW.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
            maxH.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        return false;
    }
}
