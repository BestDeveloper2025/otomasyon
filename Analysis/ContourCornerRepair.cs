using static otomasyon.Geometry.GeometryHelper;

namespace otomasyon.Analysis;

internal static class ContourCornerRepair
{
    public static void InsertMissingAxisAlignedCorners(
        List<(double X, double Y, double Bulge)> corners,
        double tol)
    {
        while (corners.Count >= 3 && corners.Count < 4)
        {
            int before = corners.Count;
            InsertOneMissingBBoxCorner(corners, tol);
            if (corners.Count == before)
                break;
        }
    }

    public static void EnsureFourRectangleCornersIfThreeLShape(
        List<(double X, double Y, double Bulge)> corners,
        double tol)
    {
        if (corners.Count != 3)
            return;

        EnsureFourRectangleCorners(corners, tol);
    }

    public static void EnsureFourRectangleCorners(
        List<(double X, double Y, double Bulge)> corners,
        double tol)
    {
        if (corners.Count < 3)
            return;

        double minX = corners.Min(c => c.X);
        double maxX = corners.Max(c => c.X);
        double minY = corners.Min(c => c.Y);
        double maxY = corners.Max(c => c.Y);

        if (maxX - minX < tol || maxY - minY < tol)
            return;

        var target = new List<(double X, double Y, double Bulge)>
        {
            (minX, minY, 0),
            (maxX, minY, 0),
            (maxX, maxY, 0),
            (minX, maxY, 0)
        };

        corners.Clear();
        corners.AddRange(target);
    }

    private static void InsertOneMissingBBoxCorner(
        List<(double X, double Y, double Bulge)> corners,
        double tol)
    {
        double minX = corners.Min(c => c.X);
        double maxX = corners.Max(c => c.X);
        double minY = corners.Min(c => c.Y);
        double maxY = corners.Max(c => c.Y);

        if (maxX - minX < tol || maxY - minY < tol)
            return;

        var bbox = new[]
        {
            (minX, minY),
            (maxX, minY),
            (maxX, maxY),
            (minX, maxY)
        };

        foreach (var (bx, by) in bbox)
        {
            if (corners.Any(c => PointsNear(c.X, c.Y, bx, by, tol)))
                continue;

            InsertInCcwPerimeterOrder(corners, (bx, by, 0), tol);
            return;
        }
    }

    /// <summary>Dikdörtgen çevresi sırası: (0,0)→(maxX,0)→(maxX,maxY)→(0,maxY).</summary>
    private static void InsertInCcwPerimeterOrder(
        List<(double X, double Y, double Bulge)> corners,
        (double X, double Y, double Bulge) missing,
        double tol)
    {
        double minX = corners.Min(c => c.X);
        double maxX = corners.Max(c => c.X);
        double minY = corners.Min(c => c.Y);
        double maxY = corners.Max(c => c.Y);

        var perimeter = new List<(double X, double Y, double Bulge)>
        {
            FindCorner(corners, minX, minY, tol),
            FindCorner(corners, maxX, minY, tol),
            FindCorner(corners, maxX, maxY, tol),
            FindCorner(corners, minX, maxY, tol)
        };

        int insertIdx = -1;
        for (int i = 0; i < 4; i++)
        {
            var expected = i switch
            {
                0 => (minX, minY),
                1 => (maxX, minY),
                2 => (maxX, maxY),
                _ => (minX, maxY)
            };

            if (PointsNear(missing.X, missing.Y, expected.Item1, expected.Item2, tol))
            {
                insertIdx = i;
                break;
            }
        }

        if (insertIdx < 0)
        {
            corners.Add(missing);
            return;
        }

        perimeter.Insert(insertIdx, missing);
        corners.Clear();
        foreach (var p in perimeter)
        {
            if (!double.IsNaN(p.X))
                corners.Add(p);
        }
    }

    private static (double X, double Y, double Bulge) FindCorner(
        List<(double X, double Y, double Bulge)> corners,
        double x, double y,
        double tol)
    {
        foreach (var c in corners)
        {
            if (PointsNear(c.X, c.Y, x, y, tol))
                return c;
        }

        return (double.NaN, double.NaN, 0);
    }

}
