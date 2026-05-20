using System.Drawing.Text;
using System.Globalization;
using otomasyon.Analysis;
using otomasyon.Geometry;
using otomasyon.Models;

namespace otomasyon.Rendering;

/// <summary>
/// Radius merkezi ve uç köşe açıları (teğet–kenar) — ekran uzayında doğru konumda.
/// </summary>
public sealed class RadiusFeatureRenderer
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public void Paint(
        Graphics graphics,
        DxfScene scene,
        IReadOnlyList<RadiusFeature> features,
        Rectangle clip,
        in WorldToScreenTransform transform)
    {
        if (features.Count == 0 || !scene.Bounds.HasBounds)
            return;

        double cx = (scene.Bounds.MinX + scene.Bounds.MaxX) * 0.5;
        double cy = (scene.Bounds.MinY + scene.Bounds.MaxY) * 0.5;

        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        using var centerPen = new Pen(Color.FromArgb(160, 200, 60, 60), 1.2f);
        using var anglePen = new Pen(Color.FromArgb(220, 0, 110, 200), 2.2f)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };
        using var centerFont = new Font("Segoe UI", 7f, FontStyle.Regular, GraphicsUnit.Point);
        using var angleFont = new Font("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point);
        using var centerBrush = new SolidBrush(Color.FromArgb(140, 120, 40, 40));
        using var angleBrush = new SolidBrush(Color.FromArgb(0, 30, 110));
        using var angleHalo = new SolidBrush(Color.FromArgb(250, 245, 250, 255));

        foreach (var f in features)
        {
            double lineOutAtStart = AngleMath.Normalize360(f.Line1DirectionDeg + 180.0);
            double arcBackAtEnd = AngleMath.DirectionDeg(f.StartX - f.EndX, f.StartY - f.EndY);

            AngleAnnotationDrawer.DrawCornerAngle(
                graphics, clip, transform,
                f.StartX, f.StartY,
                lineOutAtStart, f.StartTangentAngleDeg,
                f.StartCornerAngleDeg,
                string.Format(Inv, "R{0} baş {1:0.0}°", f.Index, f.StartCornerAngleDeg),
                angleFont, angleBrush, angleHalo, anglePen, cx, cy);

            AngleAnnotationDrawer.DrawCornerAngle(
                graphics, clip, transform,
                f.EndX, f.EndY,
                arcBackAtEnd, f.Line2DirectionDeg,
                f.EndCornerAngleDeg,
                string.Format(Inv, "R{0} bit {1:0.0}°", f.Index, f.EndCornerAngleDeg),
                angleFont, angleBrush, angleHalo, anglePen, cx, cy);

            var sc = transform.ToScreen(f.CenterX, f.CenterY);
            graphics.DrawEllipse(centerPen, sc.X - 3f, sc.Y - 3f, 6f, 6f);
            string centerLabel = string.Format(Inv, "R{0}", f.Index);
            SizeF csz = graphics.MeasureString(centerLabel, centerFont);
            graphics.DrawString(centerLabel, centerFont, centerBrush,
                sc.X - csz.Width * 0.5f, sc.Y - csz.Height - 10f);
        }
    }
}
