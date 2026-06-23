using netDxf;
using netDxf.Entities;
using otomasyon.Geometry;

namespace otomasyon.Dxf;

public static class DxfDocumentTransformer
{
    public static void Apply(DxfDocument doc, in AffineTransform2D transform)
    {
        foreach (EntityObject entity in doc.Entities.All.ToList())
        {
            switch (entity)
            {
                case Line line:
                    line.StartPoint = Transform(line.StartPoint, transform);
                    line.EndPoint = Transform(line.EndPoint, transform);
                    break;

                case Polyline2D poly:
                    foreach (var vx in poly.Vertexes)
                    {
                        vx.Position = Transform(vx.Position, transform);
                        if (transform.NegateBulge)
                            vx.Bulge = -vx.Bulge;
                    }
                    break;

                case Polyline3D poly3:
                    for (int vi = 0; vi < poly3.Vertexes.Count; vi++)
                    {
                        var vertex = poly3.Vertexes[vi];
                        var p = Transform(new Vector3(vertex.X, vertex.Y, vertex.Z), transform);
                        vertex.X = p.X;
                        vertex.Y = p.Y;
                        vertex.Z = p.Z;
                    }
                    break;

                case Arc arc:
                    arc.Center = Transform(arc.Center, transform);
                    double delta = transform.RotationDegrees;
                    arc.StartAngle = NormalizeAngle(arc.StartAngle + delta);
                    arc.EndAngle = NormalizeAngle(arc.EndAngle + delta);
                    break;

                case Circle circle:
                    circle.Center = Transform(circle.Center, transform);
                    break;
            }
        }
    }

    public static void SnapNearZero(DxfDocument doc, double eps = 1e-4)
    {
        foreach (EntityObject entity in doc.Entities.All)
        {
            switch (entity)
            {
                case Line line:
                    line.StartPoint = Snap(line.StartPoint, eps);
                    line.EndPoint = Snap(line.EndPoint, eps);
                    break;
                case Polyline2D poly:
                    foreach (var vx in poly.Vertexes)
                        vx.Position = Snap(vx.Position, eps);
                    break;
                case Polyline3D poly3:
                    for (int vi = 0; vi < poly3.Vertexes.Count; vi++)
                    {
                        var vertex = poly3.Vertexes[vi];
                        if (Math.Abs(vertex.X) < eps) vertex.X = 0;
                        if (Math.Abs(vertex.Y) < eps) vertex.Y = 0;
                    }
                    break;
                case Arc arc:
                    arc.Center = Snap(arc.Center, eps);
                    break;
                case Circle circle:
                    circle.Center = Snap(circle.Center, eps);
                    break;
            }
        }
    }

    private static Vector2 Snap(Vector2 p, double eps)
    {
        double x = Math.Abs(p.X) < eps ? 0 : p.X;
        double y = Math.Abs(p.Y) < eps ? 0 : p.Y;
        return new Vector2(x, y);
    }

    private static Vector3 Snap(Vector3 p, double eps)
    {
        double x = Math.Abs(p.X) < eps ? 0 : p.X;
        double y = Math.Abs(p.Y) < eps ? 0 : p.Y;
        return new Vector3(x, y, p.Z);
    }

    public static List<(double X, double Y)> CollectSamplePoints(DxfDocument doc)
    {
        var points = new List<(double X, double Y)>();
        foreach (EntityObject entity in doc.Entities.All)
        {
            switch (entity)
            {
                case Line line:
                    points.Add((line.StartPoint.X, line.StartPoint.Y));
                    points.Add((line.EndPoint.X, line.EndPoint.Y));
                    break;
                case Polyline2D poly:
                    foreach (var vx in poly.Vertexes)
                        points.Add((vx.Position.X, vx.Position.Y));
                    break;
                case Polyline3D poly3:
                    foreach (var vx in poly3.Vertexes)
                        points.Add((vx.X, vx.Y));
                    break;
                case Arc arc:
                    points.Add((arc.Center.X, arc.Center.Y));
                    break;
                case Circle circle:
                    points.Add((circle.Center.X, circle.Center.Y));
                    break;
            }
        }

        return points;
    }

    private static Vector2 Transform(Vector2 p, in AffineTransform2D t)
    {
        var (x, y) = t.Apply(p.X, p.Y);
        return new Vector2(x, y);
    }

    private static Vector3 Transform(Vector3 p, in AffineTransform2D t)
    {
        var (x, y) = t.Apply(p.X, p.Y);
        return new Vector3(x, y, p.Z);
    }

    private static double NormalizeAngle(double deg)
    {
        double a = deg % 360.0;
        if (a < 0) a += 360.0;
        return a;
    }
}
