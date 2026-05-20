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
        double stepDistance = stoneWidthMm - bindirmeMm;
        double currentDepth = stoneWidthMm;

        if (currentDepth >= targetThicknessMm)
        {
            depths.Add(targetThicknessMm);
        }
        else
        {
            depths.Add(currentDepth);
            
            while (currentDepth < targetThicknessMm - 1e-3)
            {
                currentDepth += stepDistance;
                if (currentDepth >= targetThicknessMm)
                {
                    depths.Add(targetThicknessMm);
                    break;
                }
                depths.Add(currentDepth);
            }
        }

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
