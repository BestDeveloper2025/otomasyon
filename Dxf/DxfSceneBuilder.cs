using netDxf;
using netDxf.Entities;
using otomasyon.Analysis;
using otomasyon.Geometry;
using otomasyon.Models;

namespace otomasyon.Dxf;

/// <summary>
/// netDxf belgesini sahne modeline dönüştürür (Model / aktif layout).
/// </summary>
public sealed class DxfSceneBuilder
{
    private readonly RadiusFeatureExtractor _radiusExtractor = new();
    private readonly List<EntityObject> _entities = new();
    private readonly List<List<(double X, double Y)>> _pointLists = new();
    private double _minX, _maxX, _minY, _maxY;
    private bool _hasBounds;
    private int _edgeLineAndPolySegments;
    private int _arcCount;
    private int _circleCount;
    private int _trackedEntityCount;

    public DxfScene Build(DxfDocument document)
    {
        Reset();

        foreach (EntityObject entity in document.Entities.All)
        {
            switch (entity)
            {
                case Line line:
                    AddLine(line);
                    break;
                case Polyline2D lw:
                    AddPolyline2D(lw);
                    break;
                case Polyline3D poly3:
                    AddPolyline3D(poly3);
                    break;
                case Arc arc:
                    AddArc(arc);
                    break;
                case Circle circle:
                    AddCircle(circle);
                    break;
            }
        }

        var bounds = _hasBounds
            ? new SceneBoundingBox { MinX = _minX, MaxX = _maxX, MinY = _minY, MaxY = _maxY, HasBounds = true }
            : SceneBoundingBox.Empty;

        var pointListsReadOnly = new List<IReadOnlyList<(double X, double Y)>>(_pointLists.Count);
        foreach (var pl in _pointLists)
            pointListsReadOnly.Add(pl);

        var draft = new DxfScene(
            _entities,
            pointListsReadOnly,
            bounds,
            SceneStatistics.Zero,
            Array.Empty<RadiusFeature>(),
            Array.Empty<ContourEdge>());

        var analysis = _radiusExtractor.Extract(draft);

        var stats = new SceneStatistics
        {
            EdgeLineAndPolySegments = _edgeLineAndPolySegments,
            ArcCount = _arcCount,
            CircleCount = _circleCount,
            RadiusFeatureCount = analysis.RadiusFeatures.Count,
            ContourEdgeCount = analysis.ContourEdges.Count,
            TrackedEntityCount = _trackedEntityCount
        };

        return new DxfScene(
            _entities,
            pointListsReadOnly,
            bounds,
            stats,
            analysis.RadiusFeatures,
            analysis.ContourEdges);
    }

    private void Reset()
    {
        _entities.Clear();
        _pointLists.Clear();
        _hasBounds = false;
        _edgeLineAndPolySegments = 0;
        _arcCount = 0;
        _circleCount = 0;
        _trackedEntityCount = 0;
    }

    private void AddLine(Line line)
    {
        var coords = new List<(double X, double Y)>
        {
            (line.StartPoint.X, line.StartPoint.Y),
            (line.EndPoint.X, line.EndPoint.Y)
        };
        IncludePoints(coords);

        _entities.Add(line);
        _pointLists.Add(coords);
        _edgeLineAndPolySegments += 1;
        _trackedEntityCount += 1;
    }

    private void AddPolyline2D(Polyline2D poly)
    {
        var coords = new List<(double X, double Y)>();
        foreach (var vx in poly.Vertexes)
        {
            var p = vx.Position;
            coords.Add((p.X, p.Y));
        }

        IncludePoints(coords);
        IncludePolyline2DExplodedBounds(poly);

        _entities.Add(poly);
        _pointLists.Add(coords);
        _edgeLineAndPolySegments += PolylineMetrics.CountSegments(poly.Vertexes.Count, poly.IsClosed);
        _trackedEntityCount += 1;
    }

    private void AddPolyline3D(Polyline3D poly)
    {
        var coords = new List<(double X, double Y)>();
        foreach (var v in poly.Vertexes)
            coords.Add((v.X, v.Y));

        IncludePoints(coords);

        _entities.Add(poly);
        _pointLists.Add(coords);
        _edgeLineAndPolySegments += PolylineMetrics.CountSegments(poly.Vertexes.Count, poly.IsClosed);
        _trackedEntityCount += 1;
    }

    private void AddArc(Arc arc)
    {
        var coords = ArcSampler.SampleArcPoints(arc, 48);
        IncludePoints(coords);

        _entities.Add(arc);
        _pointLists.Add(coords);
        _arcCount += 1;
        _trackedEntityCount += 1;
    }

    private void AddCircle(Circle circle)
    {
        var coords = ArcSampler.SampleCirclePoints(circle, 64);
        IncludePoints(coords);

        _entities.Add(circle);
        _pointLists.Add(coords);
        _circleCount += 1;
        _trackedEntityCount += 1;
    }

    private void IncludePolyline2DExplodedBounds(Polyline2D poly)
    {
        try
        {
            foreach (var e in poly.Explode())
            {
                switch (e)
                {
                    case Line ln:
                        IncludePoint(ln.StartPoint.X, ln.StartPoint.Y);
                        IncludePoint(ln.EndPoint.X, ln.EndPoint.Y);
                        break;
                    case Arc a:
                        IncludePoints(ArcSampler.SampleArcPoints(a, 32));
                        break;
                }
            }
        }
        catch
        {
            // Explode başarısız olursa tepe noktaları bbox için yeterli olabilir.
        }
    }

    private void IncludePoints(IEnumerable<(double X, double Y)> points)
    {
        foreach (var p in points)
            IncludePoint(p.X, p.Y);
    }

    private void IncludePoint(double x, double y)
    {
        if (!_hasBounds)
        {
            _minX = _maxX = x;
            _minY = _maxY = y;
            _hasBounds = true;
            return;
        }

        if (x < _minX) _minX = x;
        if (x > _maxX) _maxX = x;
        if (y < _minY) _minY = y;
        if (y > _maxY) _maxY = y;
    }
}
