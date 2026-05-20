using otomasyon.Models;

namespace otomasyon.Analysis;

/// <summary>DXF uç birleştirme toleransı (çok küçük değerler LINE zincirini koparır).</summary>
internal static class ContourTolerance
{
    public static double FromScene(DxfScene scene)
    {
        if (!scene.Bounds.HasBounds)
            return 1e-3;

        double span = Math.Max(scene.Bounds.Width, scene.Bounds.Height);
        if (span <= 1e-12)
            span = 1;

        return Math.Max(1e-3, span * 1e-4);
    }
}
