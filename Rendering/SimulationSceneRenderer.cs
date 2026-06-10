using System.Drawing.Drawing2D;
using otomasyon.Models.Simulation;
using otomasyon.Simulation;

namespace otomasyon.Rendering;

/// <summary>
/// Simülasyon sahnesi: işlenen (kaldırılan) malzeme bantları, aktif kenar, takım ve etiketler.
/// Tüm geometri <see cref="ContourPointSampler"/> ile örneklenir; böylece bant, vurgulama
/// ve takım konumu aynı kaynaktan gelir (yaylarda tutarlı).
/// </summary>
public sealed class SimulationSceneRenderer
{
    private readonly DxfSceneRenderer _baseRenderer = new();

    private static readonly Color[] TourPalette =
    {
        Color.FromArgb(120, 20, 160, 140),  // teal-green
        Color.FromArgb(120, 40, 120, 220),  // blue
        Color.FromArgb(120, 220, 120, 30),  // orange
        Color.FromArgb(120, 180, 60, 200),  // purple
        Color.FromArgb(120, 220, 40, 90),   // magenta/red
        Color.FromArgb(120, 120, 170, 30),  // olive
        Color.FromArgb(120, 30, 170, 210),  // cyan
        Color.FromArgb(120, 200, 90, 150),  // pink
    };

    public void Paint(
        Graphics g,
        SimulationJob job,
        SimulationSnapshot snapshot,
        Rectangle clip,
        in WorldToScreenTransform transform)
    {
        _baseRenderer.Paint(g, job.Scene, clip, transform, drawEdgeLabels: false);

        DrawMachinedAreas(g, job, snapshot, transform);
        DrawToolPath(g, job, snapshot, transform);

        EdgeLabelRenderer.DrawForPath(g, job.Path, transform, snapshot.IsFinished ? null : snapshot.EdgeIndex);
        DrawDepthLabels(g, job, snapshot, transform);
        HighlightActiveEdge(g, job, snapshot, transform);
        DrawTool(g, snapshot, job, transform);
    }

    private static void DrawMachinedAreas(Graphics g, SimulationJob job, SimulationSnapshot snapshot, in WorldToScreenTransform transform)
    {
        for (int i = 0; i < job.Path.Segments.Count; i++)
        {
            var seg = job.Path.Segments[i];
            var edgePlan = job.Plan.FindEdge(seg.EdgeIndex);
            if (edgePlan == null || edgePlan.Passes.Count == 0)
                continue;

            int completedPasses = snapshot.IsFinished ? edgePlan.Passes.Count : snapshot.TourIndex;
            if (!snapshot.IsFinished && i < snapshot.SegmentIndex)
                completedPasses++;

            if (completedPasses > edgePlan.Passes.Count)
                completedPasses = edgePlan.Passes.Count;

            // Her turu farklı renkte göstermek için, tamamlanan tüm turların bantlarını ayrı ayrı çiz.
            for (int t = 0; t < completedPasses; t++)
            {
                double depth = edgePlan.Passes[t].DepthFromContourMm;
                using var brush = new SolidBrush(GetTourColor(t));
                FillMachinedSwath(g, brush, seg, depth, transform);
            }

            bool isCurrent = !snapshot.IsFinished && i == snapshot.SegmentIndex && snapshot.ToolIsEngaged;
            if (isCurrent && completedPasses < edgePlan.Passes.Count)
            {
                double depth = edgePlan.Passes[completedPasses].DepthFromContourMm;
                using var brush = new SolidBrush(GetTourColor(completedPasses));
                FillMachinedSwath(g, brush, seg, depth, transform, snapshot.DistanceOnEdgeMm);
            }
        }
    }

    private static Color GetTourColor(int tourIndex)
        => TourPalette[tourIndex % TourPalette.Length];

    /// <summary>Konturdan içeri <paramref name="depth"/> kadar kaldırılan malzeme bandını doldurur.</summary>
    private static void FillMachinedSwath(
        Graphics g, Brush brush, ContourPathSegment seg, double depth,
        in WorldToScreenTransform transform, double limitDistance = -1)
    {
        if (depth < 1e-6)
            return;

        double toMm = limitDistance >= 0 ? Math.Min(limitDistance, seg.LengthMm) : seg.LengthMm;
        if (toMm < 1e-6)
            return;

        var outer = SampleSegment(seg, 0, toMm, 0, transform);
        var inner = SampleSegment(seg, 0, toMm, depth, transform);
        if (outer.Count < 2 || inner.Count < 2)
            return;

        inner.Reverse();
        var poly = new List<PointF>(outer.Count + inner.Count);
        poly.AddRange(outer);
        poly.AddRange(inner);
        g.FillPolygon(brush, poly.ToArray());
    }

    /// <summary>İşlenen kenarlarda taş merkezinin izlediği yolu (kontur içine ofset) kesik çizgiyle gösterir.</summary>
    private static void DrawToolPath(Graphics g, SimulationJob job, SimulationSnapshot snapshot, in WorldToScreenTransform transform)
    {
        using var pen = new Pen(Color.FromArgb(150, 0, 110, 200), 1.2f) { DashStyle = DashStyle.Dash };

        for (int i = 0; i < job.Path.Segments.Count; i++)
        {
            var seg = job.Path.Segments[i];
            var edgePlan = job.Plan.FindEdge(seg.EdgeIndex);
            if (edgePlan == null || edgePlan.Passes.Count == 0)
                continue;

            int completedPasses = snapshot.IsFinished ? edgePlan.Passes.Count : snapshot.TourIndex;
            if (!snapshot.IsFinished && i < snapshot.SegmentIndex)
                completedPasses++;
            if (completedPasses <= 0)
                continue;
            if (completedPasses > edgePlan.Passes.Count)
                completedPasses = edgePlan.Passes.Count;

            double depth = edgePlan.Passes[completedPasses - 1].DepthFromContourMm;
            double centerOffset = depth - job.Tool.StoneWidthMm / 2.0;
            var pts = SampleSegment(seg, 0, seg.LengthMm, centerOffset, transform);
            if (pts.Count >= 2)
                g.DrawLines(pen, pts.ToArray());
        }
    }

    /// <summary>Segmenti uzunluk boyunca örnekler; her noktayı iç normal yönünde ofsetler.</summary>
    private static List<PointF> SampleSegment(
        ContourPathSegment seg, double fromMm, double toMm, double inwardOffsetMm,
        in WorldToScreenTransform transform)
    {
        var pts = new List<PointF>();
        double span = toMm - fromMm;
        if (span < 1e-9)
            return pts;

        int n = seg.IsArc ? Math.Max(2, (int)Math.Ceiling(span / 1.5)) : 1;
        for (int i = 0; i <= n; i++)
        {
            double d = fromMm + span * i / n;
            if (!ContourPointSampler.TrySampleOnSegment(seg, d, out var s))
                continue;

            double wx = s.X, wy = s.Y;
            if (Math.Abs(inwardOffsetMm) > 1e-9)
            {
                double rad = s.InwardNormalDirDeg * Math.PI / 180.0;
                wx += Math.Cos(rad) * inwardOffsetMm;
                wy += Math.Sin(rad) * inwardOffsetMm;
            }

            pts.Add(transform.ToScreen(wx, wy));
        }

        return pts;
    }

    private static void HighlightActiveEdge(Graphics g, SimulationJob job, SimulationSnapshot snapshot, in WorldToScreenTransform transform)
    {
        if (snapshot.IsFinished || snapshot.SegmentIndex >= job.Path.Segments.Count)
            return;

        var seg = job.Path.Segments[snapshot.SegmentIndex];
        using var pen = new Pen(Color.FromArgb(220, 40, 120), 3f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        var pts = SampleSegment(seg, 0, seg.LengthMm, 0, transform);
        if (pts.Count >= 2)
            g.DrawLines(pen, pts.ToArray());
    }

    private static void DrawTool(Graphics g, SimulationSnapshot snapshot, SimulationJob job, in WorldToScreenTransform transform)
    {
        if (snapshot.IsFinished)
            return;

        var contact = transform.ToScreen(snapshot.ToolX, snapshot.ToolY);

        // Taş merkezi: temas noktasından iç normal boyunca (derinlik - yarı taş genişliği) kadar.
        double shiftMm = 0;
        if (snapshot.ToolIsEngaged && snapshot.PassDepthMm > 1e-6)
            shiftMm = snapshot.PassDepthMm - job.Tool.StoneWidthMm / 2.0;

        double rad = snapshot.InwardNormalDeg * Math.PI / 180.0;
        double cx = snapshot.ToolX + Math.Cos(rad) * shiftMm;
        double cy = snapshot.ToolY + Math.Sin(rad) * shiftMm;
        var center = transform.ToScreen(cx, cy);

        bool engaged = snapshot.ToolIsEngaged;

        if (engaged)
        {
            using var stem = new Pen(Color.FromArgb(160, 0, 120, 200), 1.5f) { DashStyle = DashStyle.Dot };
            g.DrawLine(stem, center, contact);
        }

        // Temas noktası (konturda gerçekten geçtiğimiz yer).
        using var contactBrush = new SolidBrush(engaged ? Color.FromArgb(255, 220, 40, 90) : Color.FromArgb(200, 150, 150, 150));
        const float cr = 3.5f;
        g.FillEllipse(contactBrush, contact.X - cr, contact.Y - cr, cr * 2f, cr * 2f);

        // Taş kafası.
        const float r = 5f;
        using var brush = new SolidBrush(engaged ? Color.FromArgb(255, 20, 120, 220) : Color.FromArgb(180, 160, 160, 160));
        using var border = new Pen(Color.White, 1.5f);
        g.FillEllipse(brush, center.X - r, center.Y - r, r * 2f, r * 2f);
        g.DrawEllipse(border, center.X - r, center.Y - r, r * 2f, r * 2f);
    }

    private static void DrawDepthLabels(Graphics g, SimulationJob job, SimulationSnapshot snapshot, in WorldToScreenTransform transform)
    {
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        using var font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.FromArgb(0, 80, 60));
        using var halo = new SolidBrush(Color.FromArgb(220, 255, 255, 255));

        for (int i = 0; i < job.Path.Segments.Count; i++)
        {
            var seg = job.Path.Segments[i];
            var edgePlan = job.Plan.FindEdge(seg.EdgeIndex);
            if (edgePlan == null || edgePlan.Passes.Count == 0)
                continue;

            int completedPasses = snapshot.IsFinished ? edgePlan.Passes.Count : snapshot.TourIndex;
            if (!snapshot.IsFinished && i < snapshot.SegmentIndex)
                completedPasses++;
            if (!snapshot.IsFinished && i == snapshot.SegmentIndex && snapshot.ToolIsEngaged)
                completedPasses++;

            if (completedPasses > edgePlan.Passes.Count)
                completedPasses = edgePlan.Passes.Count;
            if (completedPasses == 0)
                continue;

            double currentDepth = edgePlan.Passes[completedPasses - 1].DepthFromContourMm;

            if (!ContourPointSampler.TrySampleOnSegment(seg, seg.LengthMm * 0.5, out var mid))
                continue;

            double nrad = mid.InwardNormalDirDeg * Math.PI / 180.0;
            double lx = mid.X + Math.Cos(nrad) * currentDepth;
            double ly = mid.Y + Math.Sin(nrad) * currentDepth;
            var screen = transform.ToScreen(lx, ly);

            string text = $"{currentDepth:G4} mm";
            SizeF sz = g.MeasureString(text, font);
            float tx = screen.X - sz.Width * 0.5f;
            float ty = screen.Y - sz.Height * 0.5f;

            g.FillRectangle(halo, new RectangleF(tx - 2, ty - 1, sz.Width + 4, sz.Height + 2));
            g.DrawString(text, font, textBrush, tx, ty);
        }
    }
}
