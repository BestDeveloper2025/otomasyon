using System.Globalization;
using System.Text;
using otomasyon.Analysis;
using otomasyon.Geometry;
using otomasyon.Models;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

/// <summary>
/// Makine CSV çıktısı (noktalı virgülle ayrılmış satırlar — Excel TR uyumlu).
/// SA[1..12] kenar kalınlığı, L[1..12] kenar uzunluğu — yaylarda kiriş uzunluğu (küçük yay +, büyük yay −),
/// R[1..12] radius (dış bükey +, iç bükey −), A[1..12] köşe açısı, O[1..12] kenar offset (mm).
/// </summary>
public static class CsvFileExporter
{
    private const int SlotCount = 12;
    private const int SekilIsmiSerbest = 999;
    private const string FieldSeparator = ";";
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

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
            KalinlikMm = 1
        };
    }

    public static bool TryWrite(SimulationJob job, string filePath, out string? error)
        => TryWrite(job, CreateDefaultOptions(), filePath, out error);

    public static bool TryWrite(SimulationJob job, ExportOptions options, string filePath, out string? error)
    {
        if (!TryBuildLine(job, 1, options, out string? line, out error))
            return false;

        try
        {
            WriteCsvFile(filePath, line + Environment.NewLine);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>Reçetedeki tüm şekilleri tek CSV dosyasına yazar; her şekil ayrı satır.</summary>
    public static bool TryWriteBatch(
        IReadOnlyList<SimulationJob> jobs,
        ExportOptions options,
        string filePath,
        out string? error)
    {
        var entries = new (SimulationJob Job, ExportOptions Options)[jobs.Count];
        for (int i = 0; i < jobs.Count; i++)
            entries[i] = (jobs[i], options);
        return TryWriteBatch(entries, filePath, out error);
    }

    /// <summary>Her şekil için ayrı genel kalınlık ve adet ile toplu CSV yazar.</summary>
    public static bool TryWriteBatch(
        IReadOnlyList<(SimulationJob Job, ExportOptions Options)> entries,
        string filePath,
        out string? error)
    {
        error = null;
        if (entries.Count == 0)
        {
            error = "Reçetede kayıtlı şekil yok.";
            return false;
        }

        var lines = new List<string>(entries.Count);
        for (int i = 0; i < entries.Count; i++)
        {
            var (job, options) = entries[i];
            if (!TryBuildLine(job, i + 1, options, out string? line, out string? itemError))
            {
                error = $"Şekil {i + 1}: {itemError}";
                return false;
            }

            lines.Add(line);
        }

        try
        {
            WriteCsvFile(filePath, string.Join(Environment.NewLine, lines) + Environment.NewLine);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static bool TryBuildLine(
        SimulationJob job,
        int rowIndex,
        ExportOptions options,
        out string line,
        out string? error)
    {
        if (!TryBuildFields(job, rowIndex, options, out IReadOnlyList<string>? fields, out error))
        {
            line = string.Empty;
            return false;
        }

        line = string.Join(FieldSeparator, fields.Select(EscapeCsvField));
        return true;
    }

    public static bool TryBuildFields(
        SimulationJob job,
        int rowIndex,
        ExportOptions options,
        out IReadOnlyList<string> fields,
        out string? error)
    {
        fields = Array.Empty<string>();
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

        var sa = new double[SlotCount + 1];
        var lengths = new double[SlotCount + 1];
        var radii = new double[SlotCount + 1];
        var angles = new double[SlotCount + 1];
        var offsets = new double[SlotCount + 1];

        FillArrays(job, sa, lengths, radii, angles, offsets);
        fields = BuildFields(rowIndex, options, sa, lengths, radii, angles, offsets);
        return true;
    }

    private static void FillArrays(
        SimulationJob job,
        double[] sa,
        double[] lengths,
        double[] radii,
        double[] angles,
        double[] offsets)
    {
        var radiusByEdge = BuildRadiusByEdge(job.Scene);

        if (!job.Scene.Bounds.HasBounds)
            return;

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
            offsets[idx] = edgePlan?.OffsetMm ?? 0;
            double len = SegmentLength.LineMm(seg.StartX, seg.StartY, seg.EndX, seg.EndY);
            if (seg.IsArc && Math.Abs(seg.Bulge) > 1.0)
                len = -len;
            lengths[idx] = len;

            var prev = segs[(i - 1 + n) % n];

            if (radiusByEdge.TryGetValue(idx, out double r))
                radii[idx] = r;
            else if (seg.IsArc && seg.Radius is double sr && sr > 1e-9)
                radii[idx] = SignedRadius(sr, ClassifyArcByPath(segs, seg));

            double inDir = OutgoingDirAtStart(seg);
            double prevInDir = IncomingDirAtEnd(prev);
            angles[idx] = AngleMath.OpeningAngleDeg(prevInDir, inDir);
        }
    }

    private static Dictionary<int, double> BuildRadiusByEdge(DxfScene scene)
    {
        var map = new Dictionary<int, double>();
        foreach (var rf in scene.RadiusFeatures)
        {
            if (rf.EdgeIndex >= 1 && rf.EdgeIndex <= SlotCount)
                map[rf.EdgeIndex] = SignedRadius(rf.Radius, rf.Convexity);
        }

        foreach (var e in scene.ContourEdges)
        {
            if (!e.IsRadiusSegment || e.RadiusIndex is not int ri)
                continue;

            foreach (var rf in scene.RadiusFeatures)
            {
                if (rf.Index == ri && e.Index >= 1 && e.Index <= SlotCount)
                    map[e.Index] = SignedRadius(rf.Radius, rf.Convexity);
            }
        }

        return map;
    }

    private static RadiusConvexity ClassifyArcByPath(
        IReadOnlyList<ContourPathSegment> segs, ContourPathSegment arc)
    {
        double sum = 0;
        foreach (var s in segs)
            sum += s.StartX * s.EndY - s.EndX * s.StartY;
        bool ccw = sum > 0;

        return RadiusConvexityClassifier.Classify(arc.Bulge, ccw);
    }

    private static double SignedRadius(double radius, RadiusConvexity convexity)
    {
        if (radius <= 1e-9)
            return 0;

        return convexity switch
        {
            RadiusConvexity.IcBubey => -radius,
            RadiusConvexity.DisBubey => radius,
            _ => radius
        };
    }

    private static double OutgoingDirAtStart(ContourPathSegment s)
        => AngleMath.DirectionDeg(s.EndX - s.StartX, s.EndY - s.StartY);

    private static double IncomingDirAtEnd(ContourPathSegment s)
        => AngleMath.DirectionDeg(s.EndX - s.StartX, s.EndY - s.StartY);

    private static IReadOnlyList<string> BuildFields(
        int rowIndex,
        ExportOptions options,
        double[] sa,
        double[] lengths,
        double[] radii,
        double[] angles,
        double[] offsets)
    {
        var fields = new List<string>(66)
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
            fields.Add(FormatRadius(radii[i]));

        for (int i = 1; i <= SlotCount; i++)
            fields.Add(FormatAngle(angles[i]));

        for (int i = 1; i <= SlotCount; i++)
            fields.Add(FormatOffset(offsets[i]));

        return fields;
    }

    private static void WriteCsvFile(string filePath, string content)
        => File.WriteAllText(filePath, content, Utf8WithBom);

    private static string EscapeCsvField(string value)
    {
        if (value.Contains(FieldSeparator) || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

        return value;
    }

    private static string FormatOffset(double v)
    {
        if (Math.Abs(v) < 1e-9)
            return "0.00";
        return v.ToString("0.00", Inv);
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

    private static string FormatRadius(double v)
    {
        if (Math.Abs(v) < 1e-9)
            return "0.00";
        return v.ToString("0.00", Inv);
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
