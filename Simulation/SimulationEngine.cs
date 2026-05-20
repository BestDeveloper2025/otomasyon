using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

/// <summary>
/// Tam kontur turları (CCW): her turda L1→L2→…→Ln bir kez; tur sayısı en çok geçiş isteyen kenara göre.
/// Kenar bu turda işlenmiyorsa taş kalkık (rapid); tur arası kalkış.
/// </summary>
public sealed class SimulationEngine
{
    private readonly ContourPath _path;
    private readonly MachiningPlan _plan;
    private readonly int _totalTourCount;

    private int _tourIndex;
    private int _segmentIndex;
    private double _distanceOnEdgeMm;
    private double _totalTraversedMm;
    private bool _pendingTourLift = true;

    public SimulationEngine(ContourPath path, MachiningPlan plan)
    {
        _path = path;
        _plan = plan;
        _totalTourCount = MachiningTourPlanner.GetGlobalTourCount(plan);
        Reset();
    }

    public SimulationSnapshot Current => BuildSnapshot();

    public void Reset()
    {
        _tourIndex = 0;
        _segmentIndex = 0;
        _distanceOnEdgeMm = 0;
        _totalTraversedMm = 0;
        _pendingTourLift = _totalTourCount > 0;
    }

    public bool Step(double stepMm)
    {
        if (stepMm <= 0 || _tourIndex >= _totalTourCount || _path.Segments.Count == 0)
            return false;

        if (_pendingTourLift)
            _pendingTourLift = false;

        var seg = _path.Segments[_segmentIndex];
        _distanceOnEdgeMm += stepMm;
        _totalTraversedMm += stepMm;

        if (_distanceOnEdgeMm < seg.LengthMm - 1e-6)
            return true;

        _distanceOnEdgeMm = 0;
        int nextSegment = _segmentIndex + 1;

        if (nextSegment >= _path.Segments.Count)
        {
            _tourIndex++;
            _segmentIndex = 0;
            _pendingTourLift = _tourIndex < _totalTourCount;
            return _tourIndex < _totalTourCount;
        }

        _segmentIndex = nextSegment;
        return true;
    }

    private SimulationSnapshot BuildSnapshot()
    {
        if (_tourIndex >= _totalTourCount || _path.Segments.Count == 0)
        {
            return new SimulationSnapshot
            {
                IsFinished = true,
                TotalTraversedMm = _totalTraversedMm,
                TourCount = _totalTourCount,
                StatusText = "Simülasyon tamamlandı."
            };
        }

        var seg = _path.Segments[_segmentIndex];
        bool cutting = MachiningTourPlanner.IsCuttingOnEdge(_plan, seg.EdgeIndex, _tourIndex);
        double depth = cutting
            ? MachiningTourPlanner.GetDepthOnEdge(_plan, seg.EdgeIndex, _tourIndex)
            : 0;

        bool lift = _pendingTourLift
            || (!cutting && depth < 1e-6)
            || NeedsLiftAtCorner();

        double dist = Math.Min(_distanceOnEdgeMm, seg.LengthMm);
        if (!ContourPointSampler.TrySampleOnSegment(seg, dist, out var pt))
        {
            pt = new ContourPointSampler.Sample
            {
                X = seg.StartX,
                Y = seg.StartY,
                TangentDirDeg = 0,
                InwardNormalDirDeg = 90
            };
        }

        string mode = lift ? "Taş kalkık (rapid)"
            : cutting ? $"İşleme, derinlik {depth:G4} mm"
            : "Taş kalkık";

        return new SimulationSnapshot
        {
            IsFinished = false,
            SegmentIndex = _segmentIndex,
            EdgeIndex = seg.EdgeIndex,
            CornerIndex = seg.CornerIndex,
            TourIndex = _tourIndex,
            TourCount = _totalTourCount,
            PassIndex = _tourIndex,
            PassCountOnEdge = _plan.FindEdge(seg.EdgeIndex)?.Passes.Count ?? 0,
            PassDepthMm = depth,
            ToolIsEngaged = cutting && !lift,
            DistanceOnEdgeMm = dist,
            EdgeLengthMm = seg.LengthMm,
            TotalTraversedMm = _totalTraversedMm,
            ToolX = pt.X,
            ToolY = pt.Y,
            InwardNormalDeg = pt.InwardNormalDirDeg,
            StatusText = $"Tur {_tourIndex + 1}/{_totalTourCount} (tam kontur CCW) | L{seg.EdgeIndex} (K{seg.CornerIndex}) | {mode} | Kenar: {dist:G2}/{seg.LengthMm:G2} mm"
        };
    }

    private bool NeedsLiftAtCorner()
    {
        if (_distanceOnEdgeMm > 1e-3)
            return false;

        if (_segmentIndex == 0 && _tourIndex > 0)
            return true;

        if (_segmentIndex == 0)
            return false;

        var prev = _path.Segments[_segmentIndex - 1];
        var curr = _path.Segments[_segmentIndex];
        bool prevCut = MachiningTourPlanner.IsCuttingOnEdge(_plan, prev.EdgeIndex, _tourIndex);
        bool currCut = MachiningTourPlanner.IsCuttingOnEdge(_plan, curr.EdgeIndex, _tourIndex);
        return prevCut != currCut;
    }
}
