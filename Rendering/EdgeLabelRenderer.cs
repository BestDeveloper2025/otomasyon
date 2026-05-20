using System.Drawing.Text;
using System.Globalization;
using otomasyon.Analysis;
using otomasyon.Geometry;
using otomasyon.Models;
using otomasyon.Models.Simulation;
using otomasyon.Simulation;

namespace otomasyon.Rendering;

/// <summary>Kenar ortasında L1, L2… etiketleri (CCW sıra).</summary>
public static class EdgeLabelRenderer
{
    public static void DrawForPath(Graphics g, ContourPath path, in WorldToScreenTransform transform, int? highlightEdgeIndex = null)
    {
        foreach (var seg in path.Segments)
            DrawLabel(g, seg.EdgeIndex, seg, transform, highlightEdgeIndex);
    }

    public static void DrawForScene(Graphics g, DxfScene scene, in WorldToScreenTransform transform)
    {
        if (ContourPathOrderer.TryBuildOrderedSegments(scene, out var ordered))
        {
            foreach (var s in ordered)
            {
                var seg = ToPathSegment(s);
                DrawLabel(g, s.EdgeIndex, seg, transform, null);
            }

            return;
        }

        foreach (var e in scene.ContourEdges)
        {
            var seg = new ContourPathSegment
            {
                EdgeIndex = e.Index,
                StartX = e.StartX,
                StartY = e.StartY,
                EndX = e.EndX,
                EndY = e.EndY,
                Bulge = e.Bulge,
                LengthMm = e.LengthMm > 0
                    ? e.LengthMm
                    : SegmentLength.FromBulgeMm(e.StartX, e.StartY, e.EndX, e.EndY, e.Bulge),
                IsArc = e.IsRadiusSegment
            };
            EnrichArcParams(seg, out var enriched);
            DrawLabel(g, e.Index, enriched, transform, null);
        }
    }

    private static ContourPathSegment ToPathSegment(ContourPathOrderer.OrderedSegment s)
    {
        var seg = new ContourPathSegment
        {
            EdgeIndex = s.EdgeIndex,
            StartX = s.StartX,
            StartY = s.StartY,
            EndX = s.EndX,
            EndY = s.EndY,
            Bulge = s.Bulge,
            IsArc = s.IsArc,
            LengthMm = SegmentLength.FromBulgeMm(s.StartX, s.StartY, s.EndX, s.EndY, s.Bulge)
        };
        EnrichArcParams(seg, out var enriched);
        return enriched;
    }

    private static void EnrichArcParams(ContourPathSegment seg, out ContourPathSegment result)
    {
        result = seg;
        if (!seg.IsArc || Math.Abs(seg.Bulge) < 1e-12)
            return;

        if (BulgeArcConverter.TryFromBulge(seg.StartX, seg.StartY, seg.EndX, seg.EndY, seg.Bulge,
                out double cx, out double cy, out double r, out double sa, out _))
        {
            result = new ContourPathSegment
            {
                EdgeIndex = seg.EdgeIndex,
                StartX = seg.StartX,
                StartY = seg.StartY,
                EndX = seg.EndX,
                EndY = seg.EndY,
                Bulge = seg.Bulge,
                LengthMm = seg.LengthMm,
                IsArc = true,
                CenterX = cx,
                CenterY = cy,
                Radius = r,
                StartAngleDeg = sa
            };
        }
    }

    private static void DrawLabel(
        Graphics g,
        int edgeIndex,
        ContourPathSegment seg,
        in WorldToScreenTransform transform,
        int? highlightEdgeIndex)
    {
        if (!ContourPointSampler.TryMidpoint(seg, out double midX, out double midY))
        {
            midX = (seg.StartX + seg.EndX) * 0.5;
            midY = (seg.StartY + seg.EndY) * 0.5;
        }

        bool active = highlightEdgeIndex == edgeIndex;
        string text = string.Format(CultureInfo.InvariantCulture, "L{0}", edgeIndex);
        var screen = transform.ToScreen(midX, midY);

        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        using var font = new Font("Segoe UI", active ? 10f : 9f, FontStyle.Bold, GraphicsUnit.Point);
        using var textBrush = new SolidBrush(active ? Color.FromArgb(220, 160, 0, 90) : Color.FromArgb(210, 0, 70, 130));
        using var halo = new SolidBrush(Color.FromArgb(235, 255, 252, 200));
        using var border = new Pen(active ? Color.FromArgb(230, 200, 60, 100) : Color.FromArgb(150, 100, 100, 90), 1.2f);

        SizeF sz = g.MeasureString(text, font);
        float lx = screen.X - sz.Width * 0.5f;
        float ly = screen.Y - sz.Height * 0.5f;
        var bg = new RectangleF(lx - 4f, ly - 2f, sz.Width + 8f, sz.Height + 4f);
        g.FillRectangle(halo, bg);
        g.DrawRectangle(border, Rectangle.Round(bg));
        g.DrawString(text, font, textBrush, lx, ly);
    }
}
