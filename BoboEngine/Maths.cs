using Raylib_cs;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BoboEngine;
public static class Maths
{
    static Random random = new(75);
    /// <summary>
    /// Random number from 0f - 1f
    /// </summary>
    public static float Random(int seed)
    {
        float v = random.NextSingle();
        return v;
    }

    public static float Min(float a, float b, float c)
    {
        return Min(Min(a, b), c);
    }
    public static float Min(float a, float b)
    {
        if (a < b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }
    public static float Max(float a, float b, float c)
    {
        return Max(Max(a, b), c);
    }
    public static float Max(float a, float b)
    {
        if (a > b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    
    /// <summary>
    /// Will wrap the input value to go between 0 and range; <br/>
    /// (SAME AS MOD!)
    /// </summary>
    public static float Wrap(float input, float range) => Mod(input, range);

    /// <summary>
    /// Will wrap the input value to go between value1 and value2 <br/>
    /// <br/>
    /// e.g. <br/>
    /// * WrapRange(1405, -8, 12) => 5 <br/>
    /// * WrapRange(6.5f, 5, 8) => 6.5f <br/>
    /// * WrapRange(-3, -3, 10) => -3 <br/>
    /// * WrapRange(-3, 10, -3) => 10 <br/>
    /// <br/>
    /// Desmos: \operatorname{mod}\left(x-m,r-m\right)+m
    /// </summary>
    /// <param name="input"></param>
    /// <param name="value1">The equal value to wrap by</param>
    /// <param name="value2">The non equal value to wrap by</param>
    /// <returns></returns>
    public static float WrapRange(float input, float value1, float value2) => Mod(input - value1, value2 - value1) + value1;
    /// <summary>
    /// Will wrap the input value to go between the -absRange and +absRange <br/>
    /// <br/>
    /// e.g. <br/>
    /// > Wrap(134, 180); => 134 <br/>
    /// > Wrap(190, 180); => -170 <br/>
    /// > Wrap(-360, 180); => 0 <br/>
    /// <br/>
    /// Desmos: \operatorname{mod}\left(x-r,2r\right)-r
    /// </summary>
    /// <param name="absRange">The positive and negative min and maxes of the output value</param>
    public static float WrapAbs(float input, float absRange) => Mod(input - absRange, absRange * 2) - absRange;

    /// <summary>
    /// SLIGHTLY INACURATE! <br/>
    /// <br/>
    /// Sould be = mod(-10,10) => 0 <br/>
    /// Function results in = mod(-10,10) => -10 <br/>
    /// <br/>
    /// Desmos: <br/>
    /// r_{em}\left(x,r\right)=\operatorname{sign}\left(x\right)\cdot\operatorname{mod}\left(\operatorname{abs}\left(x\right),r\right) <br/>
    /// ^ Works as % operator ^  <br/>
    /// M_{od}\left(x,r\right)=r_{em}\left(x,r\right)+r\cdot N_{EG}\left(x\right) <br/>
    /// ^ This equation ^  <br/>
    /// Same as:  <br/>
    /// \operatorname{mod}\left(x,r\right)  <br/>
    /// In Desmos
    /// </summary>
    public static float Mod(float input1, float input2) => (input1 % input2) + input2 * (input1 < 0 ? 1 : 0);

    /// <summary>
    /// Sin in degrees
    /// </summary>
    public static float Sin(float a)
    {
        return MathF.Sin(ToRad(a));
    }
    /// <summary>
    /// Cos in degrees
    /// </summary>
    public static float Cos(float a)
    {
        return MathF.Cos(ToRad(a));
    }
    /// <summary>
    /// Tan in degrees
    /// </summary>
    public static float Tan(float a)
    {
        return MathF.Tan(ToRad(a));
    }

    /// <summary>
    /// Converts degrees to radians
    /// </summary>
    public static float ToRad(float d)
    {
        return d * MathF.PI / 180;
    }
    /// <summary>
    /// Converts radians to degrees
    /// </summary>
    public static float ToDeg(float r)
    {
        return r * 180 / MathF.PI;
    }
    public static float GetTriangleDepthAtPoint(Float3 a, Float3 b, Float3 c, Float2 p)
    {
        PointInTriangle((Float2)a, (Float2)b, (Float2)c, p, out Float3 weights);

        return weights.x * a.z + weights.y * b.z + weights.z * c.z;
    }
    public static float SinedTriangleArea(Float2 a, Float2 b, Float2 c)
    {
        Float2 ac = a - c;
        Float2 abPerp = (b - a).CrossProduct();

        return -(Float2.Dot(ac, abPerp) / 2);
    }
    public static bool PointInTriangle(Float2 a, Float2 b, Float2 c, Float2 p)
    {
        // Test if point is on right side of each edge segment
        float areaABP = SinedTriangleArea(a, b, p);
        float areaBCP = SinedTriangleArea(b, c, p);
        float areaCAP = SinedTriangleArea(c, a, p);
        bool inTri = areaABP >= 0 && areaBCP >= 0 && areaCAP >= 0;

        float totalArea = areaABP + areaBCP + areaCAP;

        return inTri && totalArea > 0;
    }
    public static bool PointInTriangle(Float2 a, Float2 b, Float2 c, Float2 p, out Float3 weights)
    {
        // Test if point is on right side of each edge segment
        float areaABP = SinedTriangleArea(a, b, p);
        float areaBCP = SinedTriangleArea(b, c, p);
        float areaCAP = SinedTriangleArea(c, a, p);
        bool inTri = areaABP >= 0 && areaBCP >= 0 && areaCAP >= 0;

        // Weighting factors
        float totalArea = areaABP + areaBCP + areaCAP;
        float invAreaSum = 1 / totalArea;
        float weightA = areaBCP * invAreaSum;
        float weightB = areaCAP * invAreaSum;
        float weightC = areaABP * invAreaSum;
        weights = new(weightA, weightB, weightC);

        return inTri && totalArea > 0;
    }
    public static float UnitClamp(this float f)
    {
        return Math.Clamp(f, 0, 1);
    }
}

public struct Int3(int x = 0, int y = 0, int z = 0)
{
    public int x = x;
    public int y = y;
    public int z = z;

    public int a { get => x; set => x = value; }
    public int b { get => y; set => y = value; }
    public int c { get => z; set => z = value; }
    public override string ToString() => $"({x},{y},{z})";
    public static explicit operator Int3(int[] n)
    {
        return new(n[0], n[1], n[2]);
    }
    public static explicit operator Int3(Float3 f)
    {
        return new((int)f.x, (int)f.y, (int)f.z);
    }
}
public struct Int2(int x = 0, int y = 0)
{
    public int x = x;
    public int y = y;

    public static Int2 operator +(Int2 a, Int2 b) => new(a.x + b.x, a.y + b.y); // Adding

    public override string ToString() => $"({x},{y})";
    public static explicit operator Int2(int[] n)
    {
        return new(n[0], n[1]);
    }
    public static explicit operator Int2(Float2 f)
    {
        return new((int)f.x, (int)f.y);
    }
    public static explicit operator Int2(Float3 f)
    {
        return new((int)f.x, (int)f.y);
    }
}
public struct Float3(float x = 0, float y = 0, float z = 0)
{
    public float x = x;
    public float y = y;
    public float z = z;

    public float r { get => x; set => x = value; }
    public float g { get => y; set => y = value; }
    public float b { get => z; set => z = value; }

    #region variants
    public static Float3 white => new(1, 1, 1);
    public static Float3 black => new(0, 0, 0);
    public static Float3 red => new(1, 0, 0);
    public static Float3 green => new(0, 1, 0);
    public static Float3 blue => new(0, 0, 1);

    public static Float3 xAxis => new(1, 0, 0);
    public static Float3 yAxis => new(0, 1, 0);
    public static Float3 zAxis => new(0, 0, 1);


    public static Float3 zero => new(0, 0, 0);
    public static Float3 one => new(1, 1, 1);
    /// <summary>
    /// Random cords from 0f - 1f
    /// </summary>
    public static Float3 random => new(Maths.Random(75), Maths.Random(75), Maths.Random(75));
    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Length() => MathF.Sqrt(Dot(this, this));
    public Float3 Normalized()
    {
        float l = Length();

        if (l == 0) // Prevent NAN
        {
            return zero;
        }

        return this / Length();
    }
    public static float Dot(Float3 a, Float3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
    public static Float3 operator +(Float3 a, Float3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z); // Vector Adding
    public static Float3 operator -(Float3 a, Float3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z); // Vector Subracting
    public static Float3 operator -(float a, Float3 b) => new(a - b.x, a - b.y, a - b.z); // Subracting
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float3 operator *(Float3 a, Float3 b) => new(a.x * b.x, a.y * b.y, a.z * b.y); // Vector Multiplication
    public static Float3 operator /(Float3 a, Float3 b) => new(a.x / b.x, a.y / b.y, a.z / b.y); // Vector Division
    public static Float3 operator *(Float3 v, float f) => new(v.x * f, v.y * f, v.z * f); // Vector Length Multiplication
    public static Float3 operator /(Float3 v, float f) => new(v.x / f, v.y / f, v.z / f); // Vector Length Division
    public static Float3 operator /(float f, Float3 v) => new(f / v.x, f / v.y, f / v.z); // Vector Division
    public static Float3 operator -(Float3 a) => new(-a.x, -a.y, -a.z); // Negative Vector

    public Color ToColor() => new((int)(x * 255), (int)(y * 255), (int)(z * 255));
    public override string ToString() => $"({x.ToString()},{y.ToString()},{z.ToString()})";
    public static Float3 Parse(string s) => (Float3)s.Split(',').Select(float.Parse).ToArray();

    public Int3 RoundToInt3()
    {
        return new((int)float.Round(x), (int)float.Round(y), (int)float.Round(z));
    }

    public static explicit operator Float3(float[] f)
    {
        return new(f[0], f[1], f[2]);
    }
    public static implicit operator Float3(Int3 i)
    {
        return new(i.x, i.y, i.z);
    }

    public static implicit operator Vector3(Float3 f) => new Vector3(f.x, f.y, f.z);
}
public struct Float2(float x = 0, float y = 0)
{
    public float x = x;
    public float y = y;


    #region variants

    public static Float2 xAxis => new(1, 0);
    public static Float2 yAxis => new(0, 1);


    public static Float2 zero => new(0, 0);
    public static Float2 one => new(1, 1);
    /// <summary>
    /// Random cords from 0f - 1f
    /// </summary>
    public static Float2 random => new(Maths.Random(0), Maths.Random(1));
    #endregion

    public float Length() => MathF.Sqrt(x * x + y * y);
    public Float2 Normalized() => this / Length();
    public Float2 CrossProduct() => new (y, -x);
    public static float Dot(Float2 a, Float2 b) => a.x * b.x + a.y * b.y;

    public static Float2 operator +(Float2 a, Float2 b) => new(a.x + b.x, a.y + b.y); // Vector Adding
    public static Float2 operator -(Float2 a, Float2 b) => new(a.x - b.x, a.y - b.y); // Vector Subracting
    public static Float2 operator *(Float2 a, Float2 b) => new(a.x * b.x, a.y * b.y); // Vector Multiplication
    public static Float2 operator *(Float2 v, float f) => new(v.x * f, v.y * f); // Vector Length Multiplication
    public static Float2 operator /(Float2 v, float f) => new(v.x / f, v.y / f); // Vector Length Division
    public static Float2 operator -(Float2 a) => new(-a.x, -a.y); // Negative Vector

    public override string ToString() => $"({x},{y})";
    public static Float2 Parse(string s) => (Float2)s.Split(',').Select(float.Parse).ToArray();

    public static explicit operator Float2(float[] f)
    {
        return new(f[0], f[1]);
    }
    public static explicit operator Float2(Float3 v)
    {
        return new(v.x, v.y);
    }
}
