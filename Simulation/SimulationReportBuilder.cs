using System.Text;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

public static class SimulationReportBuilder
{
    public static string BuildReport(SimulationJob job, SimulationSnapshot finalSnapshot)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== SİMÜLASYON RAPORU ===");
        sb.AppendLine($"Dosya: {Path.GetFileName(job.SourceFilePath)}");
        sb.AppendLine($"Toplam Tur Sayısı: {finalSnapshot.TourCount}");
        
        // Zaman tahmini (hız ortalama 2000 mm/dk yani saniyede 33 mm kabul edilirse)
        double speedMmPerMin = 2000.0;
        double speedRapidMmPerMin = 5000.0; // Boşta geçiş (kalkık) hızı daha yüksek
        
        double cuttingDistance = finalSnapshot.TotalCuttingMm;
        double rapidDistance = finalSnapshot.TotalTraversedMm - finalSnapshot.TotalCuttingMm;
        
        double timeMinutes = (cuttingDistance / speedMmPerMin) + (rapidDistance / speedRapidMmPerMin);
        
        sb.AppendLine($"Toplam Makine Hareketi: {finalSnapshot.TotalTraversedMm:N1} mm");
        sb.AppendLine($" - Dolu İşleme: {cuttingDistance:N1} mm");
        sb.AppendLine($" - Boşta (Rapid) Geçiş: {rapidDistance:N1} mm");
        sb.AppendLine($"Tahmini Süre: {timeMinutes:N1} dakika (Kesim: 2m/dk, Rapid: 5m/dk varsayımıyla)");
        sb.AppendLine();
        sb.AppendLine("--- KENAR DETAYLARI ---");

        foreach (var edgePlan in job.Plan.Edges)
        {
            var passes = edgePlan.Passes;
            if (passes.Count == 0 || edgePlan.TargetThicknessMm < 1e-6)
            {
                sb.AppendLine($"L{edgePlan.EdgeIndex}: İşlenmedi (Hedef: 0 mm)");
                continue;
            }

            sb.AppendLine($"L{edgePlan.EdgeIndex}: Hedef Kalınlık = {edgePlan.TargetThicknessMm:N1} mm");
            sb.AppendLine($"   -> Toplam Tur (Pass): {passes.Count}");
            
            for (int i = 0; i < passes.Count; i++)
            {
                sb.AppendLine($"      Tur {i + 1}: Derinlik {passes[i].DepthFromContourMm:N2} mm");
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("Not: Hedefi tamamlanan kenarlarda makine taşı kaldırarak (rapid) diğer kenarlara devam etmiştir.");

        return sb.ToString();
    }
}
