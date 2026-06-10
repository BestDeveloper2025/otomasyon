using System.Globalization;
using System.Text;
using otomasyon.Analysis;
using otomasyon.Geometry;
using otomasyon.Models;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

/// <summary>
/// Makine .dat çıktısı (noktalı virgülle ayrılmış satırlar).
/// SA[1..12] kenar kalınlığı, L[1..12] kenar uzunluğu, R[1..12] radius, A[1..12] köşe açısı.
/// </summary>
public static class DatFileExporter
{
    private const int SlotCount = 12;
    private const int SekilIsmiSerbest = 999;
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public sealed class ExportOptions
    {
        /// <summary>ChangedData[2] — genel kalınlık (mm).</summary>
        public double KalinlikMm { get; init; }

        /// <summary>ChangedData[4] — istenilen adet.</summary>
        public int IstenilenAdet { get; init; } = 1;

        /// <summary>ChangedData[3] — cam tipi (boş bırakılabilir).</summary>
        public string CamTipi { get; init; } = string.Empty;

        /// <summary>ChangedData[5] — üretilen adet (boş bırakılabilir).</summary>
        public string UretilenAdet { get; init; } = string.Empty;
    }

    public static ExportOptions CreateDefaultOptions()
    {
        return new ExportOptions
        {
            // .dat ChangedData[2] — kenar kalınlıklarından (SA) bağımsız; varsayılan 1.
            KalinlikMm = 1
        };
    }

    public static bool TryWrite(SimulationJob job, string filePath, out string? error)
        => TryWrite(job, CreateDefaultOptions(), filePath, out error);

    public static bool TryWrite(SimulationJob job, ExportOptions options, string filePath, out string? error)
    {
        error = null;
        if (job.Path.Segments.Count == 0)
        {
            error = "Kontur yolu boş; çıktı üretilemedi.";
            return false;
        }

        if (job.Path.Segments.Count > SlotCount)
        {
            error = $"En fazla {SlotCount} kenar desteklenir; konturda {job.Path.Segments.Count} kenar var.";
            return false;
        }

        try
        {
            var sa = new double[SlotCount + 1];
            var lengths = new double[SlotCount + 1];
            var radii = new double[SlotCount + 1];
            var angles = new double[SlotCount + 1];

            FillArrays(job, sa, lengths, radii, angles);

            var line = BuildLine(1, options, sa, lengths, radii, angles);
            File.WriteAllText(filePath, line + Environment.NewLine, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static void FillArrays(
        SimulationJob job,
        double[] sa,
        double[] lengths,
        double[] radii,
        double[] angles)
    {
        var radiusByEdge = BuildRadiusByEdge(job.Scene);

        if (!job.Scene.Bounds.HasBounds)
            return;

        double mcx = (job.Scene.Bounds.MinX + job.Scene.Bounds.MaxX) * 0.5;
        double mcy = (job.Scene.Bounds.MinY + job.Scene.Bounds.MaxY) * 0.5;

        var segs = job.Path.Segments;
        int n = segs.Count;

        for (int i = 0; i < n; i++)
        {
            var seg = segs[i];
            int idx = seg.EdgeIndex;
            if (idx < 1 || idx > SlotCount)
                continue;

            var edgePlan = job.Plan.FindEdge(idx);
            sa[idx] = edgePlan?.TargetThicknessMm ?? 0;
            lengths[idx] = seg.LengthMm > 1e-9
                ? seg.LengthMm
                : ContourSegmentLength.ComputeMm(seg);

            if (radiusByEdge.TryGetValue(idx, out double r))
                radii[idx] = r;

            var prev = segs[(i - 1 + n) % n];
            double inDir = OutgoingDirAtStart(seg);
            double prevInDir = IncomingDirAtEnd(prev);
            angles[idx] = AngleMath.InteriorAngleBetweenRaysDeg(
                AngleMath.Normalize360(prevInDir + 180.0), inDir,
                seg.StartX, seg.StartY, mcx, mcy);
        }
    }

    private static Dictionary<int, double> BuildRadiusByEdge(DxfScene scene)
    {
        var map = new Dictionary<int, double>();
        foreach (var rf in scene.RadiusFeatures)
        {
            if (rf.EdgeIndex >= 1 && rf.EdgeIndex <= SlotCount)
                map[rf.EdgeIndex] = rf.Radius;
        }

        foreach (var e in scene.ContourEdges)
        {
            if (!e.IsRadiusSegment || e.RadiusIndex is not int ri)
                continue;

            foreach (var rf in scene.RadiusFeatures)
            {
                if (rf.Index == ri && e.Index >= 1 && e.Index <= SlotCount)
                    map[e.Index] = rf.Radius;
            }
        }

        return map;
    }

    private static double OutgoingDirAtStart(ContourPathSegment s)
    {
        if (!s.IsArc)
            return AngleMath.DirectionDeg(s.EndX - s.StartX, s.EndY - s.StartY);

        if (BulgeArcConverter.TryFromBulge(s.StartX, s.StartY, s.EndX, s.EndY, s.Bulge,
                out double cx, out double cy, out _, out _, out _))
        {
            double ang = AngleMath.DirectionDeg(s.StartX - cx, s.StartY - cy);
            return AngleMath.ArcTangentDeg(ang, s.Bulge > 0);
        }

        return AngleMath.DirectionDeg(s.EndX - s.StartX, s.EndY - s.StartY);
    }

    private static double IncomingDirAtEnd(ContourPathSegment s)
    {
        if (!s.IsArc)
            return AngleMath.DirectionDeg(s.EndX - s.StartX, s.EndY - s.StartY);

        if (BulgeArcConverter.TryFromBulge(s.StartX, s.StartY, s.EndX, s.EndY, s.Bulge,
                out double cx, out double cy, out _, out _, out _))
        {
            double ang = AngleMath.DirectionDeg(s.EndX - cx, s.EndY - cy);
            return AngleMath.ArcTangentDeg(ang, s.Bulge > 0);
        }

        return AngleMath.DirectionDeg(s.EndX - s.StartX, s.EndY - s.StartY);
    }

    private static string BuildLine(
        int rowIndex,
        ExportOptions options,
        double[] sa,
        double[] lengths,
        double[] radii,
        double[] angles)
    {
        // İlk alan: satır sıra numarası (örnek: 1, 2, …)
        var fields = new List<string>(54)
        {
            rowIndex.ToString(Inv),
            SekilIsmiSerbest.ToString(Inv),
            FormatKalinlik(options.KalinlikMm),
            options.CamTipi,
            options.IstenilenAdet.ToString(Inv),
            options.UretilenAdet
        };

        for (int i = 1; i <= SlotCount; i++)
            fields.Add(FormatSa(sa[i]));

        for (int i = 1; i <= SlotCount; i++)
            fields.Add(FormatLength(lengths[i]));

        for (int i = 1; i <= SlotCount; i++)
            fields.Add(FormatSa(radii[i]));

        for (int i = 1; i <= SlotCount; i++)
            fields.Add(FormatAngle(angles[i]));

        return string.Join(";", fields);
    }

    private static string FormatKalinlik(double v)
    {
        if (Math.Abs(v - Math.Round(v)) < 1e-6)
            return ((int)Math.Round(v)).ToString(Inv);
        return v.ToString("0.0", Inv);
    }

    private static string FormatSa(double v)
    {
        if (Math.Abs(v) < 1e-9)
            return "0.0";
        if (Math.Abs(v - Math.Round(v)) < 1e-6)
            return ((int)Math.Round(v)).ToString(Inv);
        return v.ToString("0.0", Inv);
    }

    private static string FormatLength(double v)
    {
        if (Math.Abs(v) < 1e-9)
            return "0.0";
        if (Math.Abs(v - Math.Round(v)) < 1e-6)
            return ((int)Math.Round(v)).ToString(Inv);
        return v.ToString("0.0", Inv);
    }

    private static string FormatAngle(double v)
    {
        if (Math.Abs(v) < 1e-9 || double.IsNaN(v))
            return "0.000";
        if (Math.Abs(v - Math.Round(v)) < 1e-3)
            return ((int)Math.Round(v)).ToString(Inv);
        return v.ToString("0.000", Inv);
    }
}
