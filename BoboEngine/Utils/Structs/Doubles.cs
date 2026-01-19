namespace BoboEngine;

public struct Double3(double x = 0, double y = 0, double z = 0)
{
    public double x = x;
    public double y = y;
    public double z = z;

    public static Double3 zero = new();

    public double Length => Math.Sqrt(Dot(this, this));
    public double LengthSquared() => Dot(this, this);
    public Double3 Normalized() => this / Length;

    public readonly double GetAxis(Axis axis)
    {
        return axis switch
        {
            Axis.Yaxis => y,
            Axis.Xaxis => x,
            Axis.Zaxis => z,
            _ => 0,
        };
    }
    public readonly Double3 With(Axis axis, double value)
    {
        double x = axis == Axis.Xaxis ? value : this.x;
        double y = axis == Axis.Yaxis ? value : this.y;
        double z = axis == Axis.Zaxis ? value : this.z;
        return new Double3(x, y, z);
    }

    public readonly double HorizontalDistance() => Math.Sqrt(x * x + z * z);

    public readonly double HorizontalDistanceSqr() => x * x + z * z;

    public static double Dot(Double3 a, Double3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
    public static Double3 CrossProduct(Double3 a, Double3 b) => new(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    public static Double3 Lerp(Double3 s, Double3 e, float t) => (e - s) * t + s;


    public static Double3 operator +(Double3 a, Double3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z); // Vector Adding
    public static Double3 operator -(Double3 a, Double3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z); // Vector Subracting
    public static Double3 operator -(double a, Double3 b) => new(a - b.x, a - b.y, a - b.z); // Subracting

    public static Double3 operator *(Double3 a, Double3 b) => new(a.x * b.x, a.y * b.y, a.z * b.z); // Vector Multiplication
    public static Double3 operator /(Double3 a, Double3 b) => new(a.x / b.x, a.y / b.y, a.z / b.z); // Vector Division
    public static Double3 operator *(Double3 v, double f) => new(v.x * f, v.y * f, v.z * f); // Vector Length Multiplication
    public static Double3 operator /(Double3 v, double f) => new(v.x / f, v.y / f, v.z / f); // Vector Length Division
    public static Double3 operator /(double f, Double3 v) => new(f / v.x, f / v.y, f / v.z); // Vector Division


    public static implicit operator Double3(Int3 v) => new(v.x, v.y, v.z);
    public static implicit operator Double3(Float3 v) => new(v.x, v.y, v.z);
    public override readonly string ToString() => $"({x.ToString()},{y.ToString()},{z.ToString()})";
}
public struct Double2(double x = 0, double y = 0)
{
    public double x = x;
    public double y = y;

    public readonly static Double2 zero = new();
    public readonly static Double2 xAxis = new (1, 0);
    public readonly static Double2 yAxis = new (0, 1);

    public double Length => Math.Sqrt(Dot(this, this));
    public double LengthSquared() => Dot(this, this);
    public Double2 Normalized() => this / Length;


    public static double Dot(Double2 a, Double2 b) => a.x * b.x + a.y * b.y;
    public static Double2 Lerp(Double2 s, Double2 e, float t) => (e - s) * t + s;


    public static Double2 operator +(Double2 a, Double2 b) => new(a.x + b.x, a.y + b.y); // Vector Adding
    public static Double2 operator -(Double2 a, Double2 b) => new(a.x - b.x, a.y - b.y); // Vector Subracting
    public static Double2 operator -(double a, Double2 b) => new(a - b.x, a - b.y); // Subracting

    public static Double2 operator *(Double2 a, Double2 b) => new(a.x * b.x, a.y * b.y); // Vector Multiplication
    public static Double2 operator /(Double2 a, Double2 b) => new(a.x / b.x, a.y / b.y); // Vector Division
    public static Double2 operator *(Double2 v, double f) => new(v.x * f, v.y * f); // Vector Length Multiplication
    public static Double2 operator /(Double2 v, double f) => new(v.x / f, v.y / f); // Vector Length Division
    public static Double2 operator /(double f, Double2 v) => new(f / v.x, f / v.y); // Vector Division


    public static implicit operator Double2(Int2 v) => new(v.x, v.y);
    public static implicit operator Double2(Float2 v) => new(v.x, v.y);
    public override string ToString() => $"({x.ToString()},{y.ToString()})";
}