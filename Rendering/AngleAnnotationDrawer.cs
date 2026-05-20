using System.Drawing.Drawing2D;
using otomasyon.Geometry;

namespace otomasyon.Rendering;

/// <summary>
/// Köşede teğet–kenar açısı yayı ve etiketini ekran uzayında çizer.
/// </summary>
internal static class AngleAnnotationDrawer
{
    public static void DrawCornerAngle(
        Graphics g,
        Rectangle clip,
        in WorldToScreenTransform transform,
        double cornerWorldX,
        double cornerWorldY,
        double ray1DirDeg,
        double ray2DirDeg,
        double displayAngleDeg,
        string label,
        Font font,
        Brush textBrush,
        Brush haloBrush,
        Pen arcPen,
        double shapeCenterWorldX,
        double shapeCenterWorldY)
    {
        if (double.IsNaN(displayAngleDeg) || displayAngleDeg < 0.3)
            return;

        var corner = transform.ToScreen(cornerWorldX, cornerWorldY);
        var v1 = WorldDirectionToScreenVector(ray1DirDeg, transform.Scale);
        var v2 = WorldDirectionToScreenVector(ray2DirDeg, transform.Scale);

        if (v1.X * v1.X + v1.Y * v1.Y < 1e-6f || v2.X * v2.X + v2.Y * v2.Y < 1e-6f)
            return;

        var centroid = transform.ToScreen(shapeCenterWorldX, shapeCenterWorldY);
        DrawAngleWedge(g, corner, v1, v2, displayAngleDeg, centroid, arcPen, 26f);
        float ox = corner.X - centroid.X;
        float oy = corner.Y - centroid.Y;
        float olen = (float)Math.Sqrt(ox * ox + oy * oy);
        if (olen < 1e-3f)
        {
            ox = 0;
            oy = -1;
            olen = 1;
        }

        ox /= olen;
        oy /= olen;

        var bis = BisectorUnit(v1, v2);
        float dot = bis.X * ox + bis.Y * oy;
        if (dot > 0)
        {
            bis.X = -bis.X;
            bis.Y = -bis.Y;
        }

        const float labelDist = 36f;
        SizeF sz = g.MeasureString(label, font);
        float lx = corner.X + bis.X * labelDist - sz.Width * 0.5f;
        float ly = corner.Y + bis.Y * labelDist - sz.Height * 0.5f;

        lx = Math.Clamp(lx, clip.Left + 4f, clip.Right - sz.Width - 4f);
        ly = Math.Clamp(ly, clip.Top + 4f, clip.Bottom - sz.Height - 4f);

        var bg = new RectangleF(lx - 4f, ly - 3f, sz.Width + 8f, sz.Height + 6f);
        g.FillRectangle(haloBrush, bg);
        using var border = new Pen(Color.FromArgb(140, 0, 90, 160), 1f);
        g.DrawRectangle(border, Rectangle.Round(bg));
        g.DrawString(label, font, textBrush, lx, ly);
    }

    private static void DrawAngleWedge(
        Graphics g,
        PointF corner,
        PointF v1,
        PointF v2,
        double displayAngleDeg,
        PointF materialRefScreen,
        Pen pen,
        float arcRadiusPx)
    {
        if (displayAngleDeg < 0.5)
            return;

        float a1 = ScreenAngleDeg(v1);
        float a2 = ScreenAngleDeg(v2);

        float sweepPrimary = a2 - a1;
        while (sweepPrimary > 180f) sweepPrimary -= 360f;
        while (sweepPrimary < -180f) sweepPrimary += 360f;

        float sweepAlt = sweepPrimary > 0 ? sweepPrimary - 360f : sweepPrimary + 360f;

        float refA = ScreenAngleDeg(new PointF(materialRefScreen.X - corner.X, materialRefScreen.Y - corner.Y));
        float rel = refA - a1;
        if (rel < 0) rel += 360f;

        float spanPrimary = Math.Abs(sweepPrimary);
        float spanAlt = Math.Abs(sweepAlt);
        bool inPrimary = rel <= spanPrimary + 1.5f;

        float sweep = inPrimary ? sweepPrimary : sweepAlt;

        if (Math.Abs(Math.Abs(sweep) - displayAngleDeg) >
            Math.Abs((inPrimary ? spanAlt : spanPrimary) - displayAngleDeg) + 0.5f)
        {
            sweep = inPrimary ? sweepAlt : sweepPrimary;
        }

        if (Math.Abs(sweep) < 1.5f)
            return;

        float left = corner.X - arcRadiusPx;
        float top = corner.Y - arcRadiusPx;
        g.DrawArc(pen, left, top, arcRadiusPx * 2f, arcRadiusPx * 2f, a1, sweep);
    }

    private static PointF WorldDirectionToScreenVector(double dirDeg, double scale)
    {
        double rad = dirDeg * Math.PI / 180.0;
        float dx = (float)(Math.Cos(rad) * scale);
        float dy = (float)(-Math.Sin(rad) * scale);
        float len = (float)Math.Sqrt(dx * dx + dy * dy);
        if (len < 1e-6f)
            return PointF.Empty;
        return new PointF(dx / len, dy / len);
    }

    private static PointF BisectorUnit(PointF v1, PointF v2)
    {
        float bx = v1.X + v2.X;
        float by = v1.Y + v2.Y;
        float len = (float)Math.Sqrt(bx * bx + by * by);
        if (len < 1e-6f)
            return v1;
        return new PointF(bx / len, by / len);
    }

    private static float ScreenAngleDeg(PointF v)
        => (float)(Math.Atan2(v.Y, v.X) * AngleMath.RadToDeg);
}
