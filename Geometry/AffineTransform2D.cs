namespace otomasyon.Geometry;

public readonly struct AffineTransform2D
{
    public double Cos { get; init; }
    public double Sin { get; init; }
    public double TranslateX { get; init; }
    public double TranslateY { get; init; }
    public bool NegateBulge { get; init; }

    public static AffineTransform2D FromRotationAndTranslation(double radians, double tx, double ty, bool negateBulge = false)
    {
        return new AffineTransform2D
        {
            Cos = Math.Cos(radians),
            Sin = Math.Sin(radians),
            TranslateX = tx,
            TranslateY = ty,
            NegateBulge = negateBulge
        };
    }

    public (double X, double Y) Apply(double x, double y)
    {
        double rx = Cos * x - Sin * y;
        double ry = Sin * x + Cos * y;
        return (rx + TranslateX, ry + TranslateY);
    }

    public double RotationDegrees => Math.Atan2(Sin, Cos) * 180.0 / Math.PI;
}
