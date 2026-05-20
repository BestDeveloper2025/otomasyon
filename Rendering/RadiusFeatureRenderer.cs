using System.Drawing.Text;
using System.Globalization;
using otomasyon.Analysis;
using otomasyon.Geometry;
using otomasyon.Models;

namespace otomasyon.Rendering;

/// <summary>
/// Radius merkezi, uç köşe açıları ve özet etiketlerini çizer.
/// </summary>
public sealed class RadiusFeatureRenderer
{
    public void Paint(Graphics graphics, IReadOnlyList<RadiusFeature> features, Rectangle clip, in WorldToScreenTransform transform)
    {
        if (features.Count == 0)
            return;

        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        using var centerPen = new Pen(Color.FromArgb(200, 180, 40, 40), 1.5f);
        using var crossPen = new Pen(Color.FromArgb(220, 180, 40, 40), 1f);
        using var anglePen = new Pen(Color.FromArgb(160, 0, 120, 200), 1.2f);
        using var font = new Font("Segoe UI", 7.5f, FontStyle.Regular, GraphicsUnit.Point);
        using var angleFont = new Font("Segoe UI", 7f, FontStyle.Bold, GraphicsUnit.Point);
        using var textBrush = new SolidBrush(Color.FromArgb(120, 20, 20));
        using var angleBrush = new SolidBrush(Color.FromArgb(0, 90, 160));
        using var halo = new SolidBrush(Color.FromArgb(230, 255, 252, 240));
        using var angleHalo = new SolidBrush(Color.FromArgb(230, 240, 248, 255));

        float cross = 5f;

        foreach (var f in features)
        {
            var sc = transform.ToScreen(f.CenterX, f.CenterY);
            graphics.DrawEllipse(centerPen, sc.X - 4f, sc.Y - 4f, 8f, 8f);
            graphics.DrawLine(crossPen, sc.X - cross, sc.Y, sc.X + cross, sc.Y);
            graphics.DrawLine(crossPen, sc.X, sc.Y - cross, sc.X, sc.Y + cross);

            DrawCornerAngleLabel(graphics, clip, transform, f.StartX, f.StartY,
                f.StartCornerAngleDeg, f.StartEdgeAngleDeg, "Baş", angleFont, angleBrush, angleHalo, anglePen);
            DrawCornerAngleLabel(graphics, clip, transform, f.EndX, f.EndY,
                f.EndCornerAngleDeg, f.EndEdgeAngleDeg, "Bit", angleFont, angleBrush, angleHalo, anglePen);

            string label = string.Format(CultureInfo.InvariantCulture,
                "R{0} K{1}\n{2:G4}\n{3}\n∠{4:G2}°",
                f.Index,
                f.EdgeIndex,
                f.Radius,
                RadiusConvexityClassifier.ToDisplayName(f.Convexity),
                f.CornerAngleDeg);

            DrawHaloLabel(graphics, clip, sc.X + 8f, sc.Y - 20f, label, font, textBrush, halo);
        }
    }

    private static void DrawCornerAngleLabel(
        Graphics g,
        Rectangle clip,
        in WorldToScreenTransform transform,
        double worldX,
        double worldY,
        double cornerAngleDeg,
        double edgeAngleDeg,
        string prefix,
        Font font,
        Brush textBrush,
        Brush haloBrush,
        Pen angleArcPen)
    {
        var screenPt = transform.ToScreen(worldX, worldY);
        string text = string.Format(CultureInfo.InvariantCulture,
            "R∠ {0}\n{1:G2}°",
            prefix,
            cornerAngleDeg);

        DrawSmallAngleArc(g, screenPt, edgeAngleDeg, cornerAngleDeg, angleArcPen);
        DrawHaloLabel(g, clip, screenPt.X + 8f, screenPt.Y - 6f, text, font, textBrush, haloBrush);
    }

    private static void DrawSmallAngleArc(Graphics g, PointF corner, double edgeAngleDeg, double spanDeg, Pen pen)
    {
        const float radiusPx = 18f;
        float startGdi = NormalizeDegreesGdi(-edgeAngleDeg);
        float sweepGdi = -(float)Math.Min(90.0, Math.Max(5.0, spanDeg));

        float left = corner.X - radiusPx;
        float top = corner.Y - radiusPx;
        g.DrawArc(pen, left, top, radiusPx * 2f, radiusPx * 2f, startGdi, sweepGdi);
    }

    private static void DrawHaloLabel(
        Graphics g,
        Rectangle clip,
        float preferredX,
        float preferredY,
        string text,
        Font font,
        Brush textBrush,
        Brush haloBrush)
    {
        SizeF sz = g.MeasureString(text, font);
        float lx = preferredX;
        float ly = preferredY;
        if (lx + sz.Width > clip.Right - 2) lx -= sz.Width + 16f;
        if (ly < clip.Top + 2) ly = clip.Top + 2f;
        if (ly + sz.Height > clip.Bottom - 2) ly = clip.Bottom - sz.Height - 2f;
        if (lx < clip.Left + 2) lx = clip.Left + 2f;

        var bg = new RectangleF(lx - 2f, ly - 1f, sz.Width + 4f, sz.Height + 2f);
        g.FillRectangle(haloBrush, bg);
        g.DrawString(text, font, textBrush, lx, ly);
    }

    private static float NormalizeDegreesGdi(double deg)
    {
        double d = deg % 360.0;
        if (d < 0) d += 360.0;
        return (float)d;
    }
}
