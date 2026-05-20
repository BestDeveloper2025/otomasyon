using System.Drawing.Drawing2D;
using System.Globalization;
using otomasyon.Models.Simulation;
using otomasyon.Simulation;

namespace otomasyon.Rendering;

public sealed class SimulationSceneRenderer
{
    private readonly DxfSceneRenderer _baseRenderer = new();

    public void Paint(
        Graphics g,
        SimulationJob job,
        SimulationSnapshot snapshot,
        Rectangle clip,
        in WorldToScreenTransform transform,
        Bitmap? trailOverlay = null)
    {
        _baseRenderer.Paint(g, job.Scene, clip, transform, drawEdgeLabels: false);

        if (trailOverlay != null)
        {
            g.DrawImage(trailOverlay, 0, 0);
        }

        EdgeLabelRenderer.DrawForPath(g, job.Path, transform, snapshot.IsFinished ? null : snapshot.EdgeIndex);
        HighlightActiveEdge(g, job, snapshot, transform);
        DrawTool(g, snapshot, job, transform);
    }

    private static void HighlightActiveEdge(
        Graphics g,
        SimulationJob job,
        SimulationSnapshot snapshot,
        in WorldToScreenTransform transform)
    {
        if (snapshot.IsFinished || snapshot.SegmentIndex >= job.Path.Segments.Count)
            return;

        var seg = job.Path.Segments[snapshot.SegmentIndex];
        using var pen = new Pen(Color.FromArgb(220, 40, 120), 3f);

        if (seg.IsArc && seg.CenterX is double cx && seg.CenterY is double cy && seg.Radius is double r
            && seg.StartAngleDeg is double sa && seg.EndAngleDeg is double ea)
        {
            DrawArcSegment(g, pen, transform, cx, cy, r, sa, ea, seg.Bulge);
        }
        else
        {
            var a = transform.ToScreen(seg.StartX, seg.StartY);
            var b = transform.ToScreen(seg.EndX, seg.EndY);
            g.DrawLine(pen, a, b);
        }
    }

    private static void DrawArcSegment(
        Graphics g, Pen pen, in WorldToScreenTransform transform,
        double cx, double cy, double r, double startDeg, double endDeg, double bulge)
    {
        var center = transform.ToScreen(cx, cy);
        float radiusPx = (float)(r * transform.Scale);
        if (radiusPx < 1f)
            return;

        float startGdi = ScreenAngleFromWorld(startDeg, transform);
        float endGdi = ScreenAngleFromWorld(endDeg, transform);
        float sweep = endGdi - startGdi;
        if (bulge > 0)
        {
            while (sweep < 0) sweep += 360f;
        }
        else
        {
            while (sweep > 0) sweep -= 360f;
        }

        float left = center.X - radiusPx;
        float top = center.Y - radiusPx;
        g.DrawArc(pen, left, top, radiusPx * 2f, radiusPx * 2f, startGdi, sweep);
    }

    private static float ScreenAngleFromWorld(double worldDeg, in WorldToScreenTransform transform)
    {
        double rad = worldDeg * Math.PI / 180.0;
        var p = transform.ToScreen(Math.Cos(rad), Math.Sin(rad));
        var o = transform.ToScreen(0, 0);
        float dx = p.X - o.X;
        float dy = p.Y - o.Y;
        float deg = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
        if (deg < 0) deg += 360f;
        return deg;
    }

    private static void DrawTool(Graphics g, SimulationSnapshot snapshot, SimulationJob job, in WorldToScreenTransform transform)
    {
        if (snapshot.IsFinished)
            return;

        double shiftMm = 0;
        if (snapshot.ToolIsEngaged && snapshot.PassDepthMm > 1e-6)
        {
            shiftMm = snapshot.PassDepthMm - (job.Tool.StoneWidthMm / 2.0);
        }

        double rad = snapshot.InwardNormalDeg * Math.PI / 180.0;
        double nx = Math.Cos(rad);
        double ny = Math.Sin(rad);

        double cx = snapshot.ToolX + nx * shiftMm;
        double cy = snapshot.ToolY + ny * shiftMm;

        var tip = transform.ToScreen(cx, cy);
        float r = (float)((job.Tool.StoneWidthMm / 2.0) * transform.Scale);
        if (r < 3f) r = 3f;

        bool engaged = snapshot.ToolIsEngaged;
        using var brush = new SolidBrush(engaged
            ? Color.FromArgb(230, 0, 140, 220)
            : Color.FromArgb(200, 120, 120, 120));
        g.FillEllipse(brush, tip.X - r, tip.Y - r, r * 2f, r * 2f);

        if (engaged && snapshot.PassDepthMm > 1e-6)
        {
            var edgePoint = transform.ToScreen(snapshot.ToolX, snapshot.ToolY);
            using var pen = new Pen(Color.FromArgb(180, 0, 120, 200), 1.5f) { DashStyle = DashStyle.Dash };
            g.DrawLine(pen, tip, edgePoint);
        }
    }
}
