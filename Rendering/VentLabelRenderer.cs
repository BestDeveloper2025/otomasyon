using System.Drawing.Text;
using System.Globalization;
using otomasyon.Geometry;
using otomasyon.Models;

namespace otomasyon.Rendering;

/// <summary>Menfez merkezinde M1, M2… etiketleri.</summary>
public static class VentLabelRenderer
{
    public static void DrawForScene(Graphics g, DxfScene scene, in WorldToScreenTransform transform)
    {
        if (scene.VentFeatures.Count == 0)
            return;

        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        using var font = new Font("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point);
        using var textBrush = new SolidBrush(Color.FromArgb(220, 0, 110, 60));
        using var halo = new SolidBrush(Color.FromArgb(235, 235, 255, 240));
        using var border = new Pen(Color.FromArgb(170, 60, 140, 90), 1.2f);

        foreach (var vent in scene.VentFeatures)
        {
            string text = string.Format(CultureInfo.InvariantCulture, "M{0}", vent.Index);
            var screen = transform.ToScreen(vent.CenterX, vent.CenterY);
            SizeF sz = g.MeasureString(text, font);
            float lx = screen.X - sz.Width * 0.5f;
            float ly = screen.Y - sz.Height * 0.5f;
            var bg = new RectangleF(lx - 4f, ly - 2f, sz.Width + 8f, sz.Height + 4f);
            g.FillRectangle(halo, bg);
            g.DrawRectangle(border, Rectangle.Round(bg));
            g.DrawString(text, font, textBrush, lx, ly);
        }
    }
}
