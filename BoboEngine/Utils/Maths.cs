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
    public static float Sin(float a) => MathF.Sin(ToRad(a));
    /// <summary>
    /// Cos in degrees
    /// </summary>
    public static float Cos(float a) => MathF.Cos(ToRad(a));
    /// <summary>
    /// Tan in degrees
    /// </summary>
    public static float Tan(float a) => MathF.Tan(ToRad(a));
    /// <summary>
    /// Csc in degrees
    /// </summary>
    public static float Csc(float a) => 1 / Sin(a);
    /// <summary>
    /// Sec in degrees
    /// </summary>
    public static float Sec(float a) => 1 / Cos(a);
    /// <summary>
    /// Cot in degrees
    /// </summary>
    public static float Cot(float a) => 1 / Tan(a);

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

    /// <summary>
    /// Triangle wave from 0 - 1 </br>
    /// Desmos: https://www.desmos.com/calculator/myjp5vxk9z
    /// </summary>
    public static float TriangleWave(float x)
    {
        // lxl is supposed to look like |x| :sob:
        float lxl = MathF.Abs(x);

        bool isInt = x == MathF.Floor(x);
        bool evenS = MathF.Floor(lxl) % 2 == 0;

        return isInt && !evenS ? 1 : ((isInt || evenS ? lxl : MathF.Ceiling(lxl) - lxl) % 1);
    }

    public static float GetTriangleDepthAtPoint(Float3 a, Float3 b, Float3 c, Float2 p)
    {
        PointInTriangle((Float2)a, (Float2)b, (Float2)c, p, out Float3 weights);

        return weights.x * a.z + weights.y * b.z + weights.z * c.z;
    }
    public static float SinedTriangleArea(Float2 a, Float2 b, Float2 c)
    {
        Float2 ac = a - c;
        Float2 abPerp = (b - a).Cross;

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

    public static Float3 PlanePointIntersection(Float3 p, Float3 v, Axis axis, float planeOffset = 0)
    {
        switch (axis)
        {
            case Axis.Xaxis:
                p = new(p.y, p.x, p.z);
                v = new(v.y, v.x, v.z);
                break;
            case Axis.Zaxis:
                p = new(p.x, p.z, p.y);
                v = new(v.x, v.z, v.y);
                break;
        }

        float slope = (planeOffset - p.y) / v.y;

        Float3 result = new (slope * v.x + p.x, planeOffset, slope * v.z + p.z);

        switch (axis)
        {
            case Axis.Xaxis:
                return new(result.y, result.x, result.z);
            case Axis.Zaxis:
                return new(result.x, result.z, result.y);
        }

        return result;
    }
}

public enum Axis
{
    Yaxis,
    Xaxis,
    Zaxis
}