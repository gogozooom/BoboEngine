using Raylib_cs;
using System.Runtime.CompilerServices;

namespace BoboEngine.GMath
{
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
        public static float GetTriangleDepthAtPoint(float3 a, float3 b, float3 c, float2 p)
        {
            PointInTriangle((float2)a, (float2)b, (float2)c, p, out float3 weights);

            return weights.x * a.z + weights.y * b.z + weights.z * c.z;
        }
        public static float SinedTriangleArea(float2 a, float2 b, float2 c)
        {
            float2 ac = a - c;
            float2 abPerp = (b - a).CrossProduct();

            return -(float2.Dot(ac, abPerp) / 2);
        }
        public static bool PointInTriangle(float2 a, float2 b, float2 c, float2 p)
        {
            // Test if point is on right side of each edge segment
            float areaABP = SinedTriangleArea(a, b, p);
            float areaBCP = SinedTriangleArea(b, c, p);
            float areaCAP = SinedTriangleArea(c, a, p);
            bool inTri = areaABP >= 0 && areaBCP >= 0 && areaCAP >= 0;

            float totalArea = areaABP + areaBCP + areaCAP;

            return inTri && totalArea > 0;
        }
        public static bool PointInTriangle(float2 a, float2 b, float2 c, float2 p, out float3 weights)
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

    public struct int3(int x = 0, int y = 0, int z = 0)
    {
        public int x = x;
        public int y = y;
        public int z = z;

        public int a { get => x; set => x = value; }
        public int b { get => y; set => y = value; }
        public int c { get => z; set => z = value; }
        public override string ToString() => $"({x},{y},{z})";
        public static explicit operator int3(int[] n)
        {
            return new(n[0], n[1], n[2]);
        }
        public static explicit operator int3(float3 f)
        {
            return new((int)f.x, (int)f.y, (int)f.z);
        }
    }
    public struct float3(float x = 0, float y = 0, float z = 0)
    {
        public float x = x;
        public float y = y;
        public float z = z;

        public float r { get => x; set => x = value; }
        public float g { get => y; set => y = value; }
        public float b { get => z; set => z = value; }

        #region variants
        public static float3 white => new(1, 1, 1);
        public static float3 black => new(0, 0, 0);
        public static float3 red => new(1, 0, 0);
        public static float3 green => new(0, 1, 0);
        public static float3 blue => new(0, 0, 1);

        public static float3 xAxis => new(1, 0, 0);
        public static float3 yAxis => new(0, 1, 0);
        public static float3 zAxis => new(0, 0, 1);


        public static float3 zero => new(0, 0, 0);
        public static float3 one => new(1, 1, 1);
        /// <summary>
        /// Random cords from 0f - 1f
        /// </summary>
        public static float3 random => new(Maths.Random(75), Maths.Random(75), Maths.Random(75));
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length() => MathF.Sqrt(Dot(this, this));
        public float3 Normalized() => this / Length();
        public static float Dot(float3 a, float3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static float3 operator +(float3 a, float3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z); // Vector Adding
        public static float3 operator -(float3 a, float3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z); // Vector Subracting
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 operator *(float3 a, float3 b) => new(a.x * b.x, a.y * b.y, a.z * b.y); // Vector Multiplication
        public static float3 operator /(float3 a, float3 b) => new(a.x / b.x, a.y / b.y, a.z / b.y); // Vector Division
        public static float3 operator *(float3 v, float f) => new(v.x * f, v.y * f, v.z * f); // Vector Length Multiplication
        public static float3 operator /(float3 v, float f) => new(v.x / f, v.y / f, v.z / f); // Vector Length Division
        public static float3 operator -(float3 a) => new(-a.x, -a.y, -a.z); // Negative Vector

        public Color ToColor() => new((int)(x * 255), (int)(y * 255), (int)(z * 255));
        public override string ToString() => $"({x.ToString()},{y.ToString()},{z.ToString()})";
        public static float3 Parse(string s) => (float3)s.Split(',').Select(float.Parse).ToArray();

        public int3 RoundToInt3()
        {
            return new((int)float.Round(x), (int)float.Round(y), (int)float.Round(z));
        }

        public static explicit operator float3(float[] f)
        {
            return new(f[0], f[1], f[2]);
        }
        public static implicit operator float3(int3 i)
        {
            return new(i.x, i.y, i.z);
        }
    }
    public struct float2(float x = 0, float y = 0)
    {
        public float x = x;
        public float y = y;


        #region variants

        public static float2 xAxis => new(1, 0);
        public static float2 yAxis => new(0, 1);


        public static float2 zero => new(0, 0);
        public static float2 one => new(1, 1);
        /// <summary>
        /// Random cords from 0f - 1f
        /// </summary>
        public static float2 random => new(Maths.Random(0), Maths.Random(1));
        #endregion

        public float Length() => MathF.Sqrt(x * x + y * y);
        public float2 Normalized() => this / Length();
        public float2 CrossProduct() => new (y, -x);
        public static float Dot(float2 a, float2 b) => a.x * b.x + a.y * b.y;

        public static float2 operator +(float2 a, float2 b) => new(a.x + b.x, a.y + b.y); // Vector Adding
        public static float2 operator -(float2 a, float2 b) => new(a.x - b.x, a.y - b.y); // Vector Subracting
        public static float2 operator *(float2 a, float2 b) => new(a.x * b.x, a.y * b.y); // Vector Multiplication
        public static float2 operator *(float2 v, float f) => new(v.x * f, v.y * f); // Vector Length Multiplication
        public static float2 operator /(float2 v, float f) => new(v.x / f, v.y / f); // Vector Length Division
        public static float2 operator -(float2 a) => new(-a.x, -a.y); // Negative Vector

        public override string ToString() => $"({x},{y})";
        public static float2 Parse(string s) => (float2)s.Split(',').Select(float.Parse).ToArray();

        public static explicit operator float2(float[] f)
        {
            return new(f[0], f[1]);
        }
        public static explicit operator float2(float3 v)
        {
            return new(v.x, v.y);
        }
    }
}
