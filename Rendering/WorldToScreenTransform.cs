using otomasyon.Models;

namespace otomasyon.Rendering;

/// <summary>
/// DXF dünyasından kontrol istemcisine piksel dönüşümü (Y ekseni ters).
/// </summary>
public readonly struct WorldToScreenTransform
{
    public double Scale { get; init; }
    public double OffsetX { get; init; }
    public double OffsetY { get; init; }

    /// <summary>
    /// Verilen dikdörtgen ve sınır kutusuna göre ölçek ve ötelemeyi hesaplar.
    /// </summary>
    public static bool TryCreate(Rectangle clientRect, SceneBoundingBox bounds, double paddingPixels, out WorldToScreenTransform transform)
    {
        transform = default;
        if (!bounds.HasBounds)
            return false;

        double w = clientRect.Width - 2 * paddingPixels;
        double h = clientRect.Height - 2 * paddingPixels;
        if (w <= 1 || h <= 1)
            return false;

        double dataW = bounds.MaxX - bounds.MinX;
        double dataH = bounds.MaxY - bounds.MinY;

        if (dataW <= 1e-12 && dataH <= 1e-12)
        {
            transform = new WorldToScreenTransform
            {
                Scale = 1,
                OffsetX = clientRect.Width / 2.0,
                OffsetY = clientRect.Height / 2.0
            };
            return true;
        }

        if (dataW <= 1e-12) dataW = 1;
        if (dataH <= 1e-12) dataH = 1;

        double scale = Math.Min(w / dataW, h / dataH);
        double drawW = dataW * scale;
        double drawH = dataH * scale;
        double offsetX = paddingPixels + (w - drawW) / 2.0 - bounds.MinX * scale;
        double offsetY = paddingPixels + (h - drawH) / 2.0 + bounds.MaxY * scale;

        transform = new WorldToScreenTransform { Scale = scale, OffsetX = offsetX, OffsetY = offsetY };
        return true;
    }

    public PointF ToScreen(double worldX, double worldY)
    {
        float sx = (float)(worldX * Scale + OffsetX);
        float sy = (float)(OffsetY - worldY * Scale);
        return new PointF(sx, sy);
    }
}
