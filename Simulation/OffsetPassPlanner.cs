using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

/// <summary>
/// Kenar kalınlığı için taş geçiş derinliklerini planlar.
/// İlk geçiş: taş genişliği kadar içeride; sonraki geçişler bindirme kadar yaklaşır (ör. 10 mm → 8 mm → 6 mm, bindirme 2).
/// </summary>
public static class OffsetPassPlanner
{
    public static IReadOnlyList<MachiningPass> PlanPasses(
        double targetThicknessMm,
        double stoneWidthMm,
        double bindirmeMm)
    {
        if (targetThicknessMm <= 0)
            return Array.Empty<MachiningPass>();

        if (stoneWidthMm <= 0)
            throw new ArgumentOutOfRangeException(nameof(stoneWidthMm));
        if (bindirmeMm < 0 || bindirmeMm >= stoneWidthMm)
            throw new ArgumentOutOfRangeException(nameof(bindirmeMm));

        var depths = new List<double>();
        double depth = Math.Min(stoneWidthMm, targetThicknessMm);
        depths.Add(depth);

        while (depth > bindirmeMm + 1e-6)
        {
            double next = depth - bindirmeMm;
            if (next <= 1e-6)
                break;
            depth = next;
            if (depth <= targetThicknessMm + 1e-6)
                depths.Add(depth);
        }

        if (depths[^1] > targetThicknessMm + 1e-3 && targetThicknessMm > 1e-6)
            depths.Add(targetThicknessMm);

        var passes = new MachiningPass[depths.Count];
        for (int i = 0; i < depths.Count; i++)
        {
            passes[i] = new MachiningPass
            {
                PassNumber = i + 1,
                DepthFromContourMm = depths[i]
            };
        }

        return passes;
    }
}
