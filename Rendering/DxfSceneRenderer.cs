using System.Drawing.Text;
using System.Globalization;
using netDxf.Entities;
using otomasyon.Geometry;
using otomasyon.Models;

namespace otomasyon.Rendering;

/// <summary>
/// Bir <see cref="DxfScene"/> örneğini GDI+ ile çizer: eksenler, geometri, köşe etiketleri.
/// </summary>
public sealed class DxfSceneRenderer
{
    private readonly RadiusFeatureRenderer _radiusRenderer = new();

    public void Paint(Graphics graphics, DxfScene scene, Rectangle clip, in WorldToScreenTransform transform, bool drawEdgeLabels = true)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(scene);

        graphics.Clear(Color.White);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var pen = new Pen(Color.Black, 1.5f);

        DrawWorldAxes(graphics, clip, transform.OffsetX, transform.OffsetY);

        foreach (var entity in scene.Entities)
        {
            switch (entity)
            {
                case Line line:
                    {
                        var a = transform.ToScreen(line.StartPoint.X, line.StartPoint.Y);
                        var b = transform.ToScreen(line.EndPoint.X, line.EndPoint.Y);
                        graphics.DrawLine(pen, a, b);
                        break;
                    }
                case Polyline2D pl2:
                    DrawPolyline2D(graphics, pen, pl2, transform);
                    break;
                case Polyline3D pl3:
                    DrawPolyline3D(graphics, pen, pl3, transform);
                    break;
                case Arc arc:
                    DrawArcEntity(graphics, pen, arc, transform);
                    break;
                case Circle circle:
                    DrawCircleEntity(graphics, pen, circle, transform);
                    break;
            }
        }

        DrawCornerLabels(graphics, clip, scene, transform);
        if (drawEdgeLabels)
            EdgeLabelRenderer.DrawForScene(graphics, scene, transform);
        _radiusRenderer.Paint(graphics, scene, scene.RadiusFeatures, clip, transform);
    }

    private static void DrawWorldAxes(Graphics g, Rectangle clip, double offsetX, double offsetY)
    {
        using var axisPen = new Pen(Color.FromArgb(190, 190, 190), 1f);
        float yAxisScreenX = (float)offsetX;
        float xAxisScreenY = (float)offsetY;

        if (xAxisScreenY >= clip.Top && xAxisScreenY <= clip.Bottom)
            g.DrawLine(axisPen, clip.Left, xAxisScreenY, clip.Right, xAxisScreenY);
        if (yAxisScreenX >= clip.Left && yAxisScreenX <= clip.Right)
            g.DrawLine(axisPen, yAxisScreenX, clip.Top, yAxisScreenX, clip.Bottom);
    }

    private static void DrawCornerLabels(Graphics g, Rectangle clip, DxfScene scene, in WorldToScreenTransform transform)
    {
        var corners = CornerDisplayCollector.Collect(scene);
        if (corners.Count == 0)
            return;

        double cx = (scene.Bounds.MinX + scene.Bounds.MaxX) * 0.5;
        double cy = (scene.Bounds.MinY + scene.Bounds.MaxY) * 0.5;

        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        using var font = new Font("Segoe UI", 8f, FontStyle.Regular, GraphicsUnit.Point);
        using var textBrush = new SolidBrush(Color.FromArgb(0, 51, 102));
        using var haloBrush = new SolidBrush(Color.FromArgb(235, 255, 255, 255));
        using var outlinePen = new Pen(Color.FromArgb(180, 160, 160, 160), 1f);
        using var pointBrush = new SolidBrush(Color.FromArgb(0, 51, 102));

        for (int i = 0; i < corners.Count; i++)
        {
            var corner = corners[i];
            string text = corner.CornerIndex is int k
                ? string.Format(CultureInfo.InvariantCulture, "K{0}\r\n{1:G6} , {2:G6}", k, corner.X, corner.Y)
                : string.Format(CultureInfo.InvariantCulture, "{0:G6} , {1:G6}", corner.X, corner.Y);

            var screenPt = transform.ToScreen(corner.X, corner.Y);
            const float pointRadius = 3f;
            g.FillEllipse(pointBrush,
                screenPt.X - pointRadius,
                screenPt.Y - pointRadius,
                pointRadius * 2f,
                pointRadius * 2f);

            double dx = corner.X - cx;
            double dy = corner.Y - cy;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 1e-9)
            {
                dx = 1;
                dy = 0;
                len = 1;
            }

            float nx = (float)(dx / len);
            float ny = (float)(dy / len);
            float offset = 10f + (i % 3) * 2f;

            SizeF sz = g.MeasureString(text, font);
            float lx = screenPt.X + nx * offset;
            float ly = screenPt.Y - ny * offset - sz.Height;

            if (!clip.Contains((int)screenPt.X, (int)screenPt.Y))
            {
                lx = Math.Clamp(lx, clip.Left + 2f, clip.Right - sz.Width - 2f);
                ly = Math.Clamp(ly, clip.Top + 2f, clip.Bottom - sz.Height - 2f);
            }
            else
            {
                if (lx + sz.Width > clip.Right - 2) lx = screenPt.X - sz.Width - offset;
                if (lx < clip.Left + 2) lx = clip.Left + 2f;
                if (ly < clip.Top + 2) ly = screenPt.Y + offset;
                if (ly + sz.Height > clip.Bottom - 2) ly = clip.Bottom - sz.Height - 2f;
            }

            var bg = new RectangleF(lx - 2f, ly - 1f, sz.Width + 4f, sz.Height + 2f);
            g.FillRectangle(haloBrush, bg);
            g.DrawRectangle(outlinePen, Rectangle.Round(bg));
            g.DrawString(text, font, textBrush, lx, ly);
        }
    }

    private static void DrawPolyline2D(Graphics g, Pen pen, Polyline2D pl2, in WorldToScreenTransform transform)
    {
        try
        {
            foreach (var fragment in pl2.Explode())
            {
                switch (fragment)
                {
                    case Line ln:
                        {
                            var a = transform.ToScreen(ln.StartPoint.X, ln.StartPoint.Y);
                            var b = transform.ToScreen(ln.EndPoint.X, ln.EndPoint.Y);
                            g.DrawLine(pen, a, b);
                            break;
                        }
                    case Arc arc:
                        DrawArcEntity(g, pen, arc, transform);
                        break;
                }
            }
        }
        catch
        {
            DrawPolylineAsLineStrip(g, pen, pl2.Vertexes.Select(v => (v.Position.X, v.Position.Y)).ToList(), pl2.IsClosed, transform);
        }
    }

    private static void DrawPolyline3D(Graphics g, Pen pen, Polyline3D pl3, in WorldToScreenTransform transform)
    {
        var pts = pl3.Vertexes.Select(v => (v.X, v.Y)).ToList();
        DrawPolylineAsLineStrip(g, pen, pts, pl3.IsClosed, transform);
    }

    private static void DrawPolylineAsLineStrip(Graphics g, Pen pen, List<(double X, double Y)> pts, bool closed, in WorldToScreenTransform transform)
    {
        if (pts.Count < 2)
            return;

        for (int i = 0; i < pts.Count - 1; i++)
        {
            var a = transform.ToScreen(pts[i].X, pts[i].Y);
            var b = transform.ToScreen(pts[i + 1].X, pts[i + 1].Y);
            g.DrawLine(pen, a, b);
        }

        if (closed)
        {
            var a = transform.ToScreen(pts[^1].X, pts[^1].Y);
            var b = transform.ToScreen(pts[0].X, pts[0].Y);
            g.DrawLine(pen, a, b);
        }
    }

    private static void DrawArcEntity(Graphics g, Pen pen, Arc arc, in WorldToScreenTransform transform)
    {
        var center = transform.ToScreen(arc.Center.X, arc.Center.Y);
        double rScreen = arc.Radius * transform.Scale;
        if (rScreen <= 0.5)
            return;

        float left = center.X - (float)rScreen;
        float top = center.Y - (float)rScreen;
        float size = (float)(2 * rScreen);

        float startGdi = NormalizeDegreesGdi(-arc.StartAngle);
        double sweepDeg = ArcSampler.GetCcwSweepRadians(arc.StartAngle, arc.EndAngle) * 180.0 / Math.PI;
        float sweepGdi = -(float)sweepDeg;

        g.DrawArc(pen, left, top, size, size, startGdi, sweepGdi);
    }

    private static void DrawCircleEntity(Graphics g, Pen pen, Circle circle, in WorldToScreenTransform transform)
    {
        var center = transform.ToScreen(circle.Center.X, circle.Center.Y);
        double rScreen = circle.Radius * transform.Scale;
        if (rScreen <= 0.5)
            return;

        float left = center.X - (float)rScreen;
        float top = center.Y - (float)rScreen;
        float size = (float)(2 * rScreen);
        g.DrawEllipse(pen, left, top, size, size);
    }

    private static float NormalizeDegreesGdi(double deg)
    {
        double d = deg % 360.0;
        if (d < 0) d += 360.0;
        return (float)d;
    }
}
