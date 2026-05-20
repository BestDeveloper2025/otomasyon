using otomasyon.Geometry;

namespace otomasyon.Analysis;

internal static class ContourQuality
{
    public static double ScoreSegments(IReadOnlyList<ContourPathOrderer.OrderedSegment> segments)
    {
        if (segments.Count == 0)
            return double.NegativeInfinity;

        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;
        int valid = 0;
        double lenSum = 0;
        double signedArea = 0;
        bool hasDiagonal = false;

        foreach (var seg in segments)
        {
            double len = SegmentLength.FromBulgeMm(seg.StartX, seg.StartY, seg.EndX, seg.EndY, seg.Bulge);
            if (len < 1e-4)
                continue;

            valid++;
            lenSum += len;

            minX = Math.Min(minX, Math.Min(seg.StartX, seg.EndX));
            maxX = Math.Max(maxX, Math.Max(seg.StartX, seg.EndX));
            minY = Math.Min(minY, Math.Min(seg.StartY, seg.EndY));
            maxY = Math.Max(maxY, Math.Max(seg.StartY, seg.EndY));

            signedArea += seg.StartX * seg.EndY - seg.EndX * seg.StartY;

            double dx = seg.EndX - seg.StartX;
            double dy = seg.EndY - seg.StartY;
            double w = maxX - minX;
            double h = maxY - minY;
            if (w > 1e-6 && h > 1e-6)
            {
                double diag = Math.Sqrt(w * w + h * h);
                if (len > diag * 0.95)
                    hasDiagonal = true;
            }
        }

        if (valid < 3)
            return double.NegativeInfinity;

        if (hasDiagonal)
            return valid * 100.0 + lenSum;

        double bboxArea = (maxX - minX) * (maxY - minY);
        double area = Math.Abs(signedArea * 0.5);
        double fill = bboxArea > 1e-6 ? area / bboxArea : 0;
        if (fill > 0.98)
            fill = 1.0;

        return valid * 1_000_000.0 + fill * 10_000.0 + lenSum;
    }
}
