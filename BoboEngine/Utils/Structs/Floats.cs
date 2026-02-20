using System.Numerics;

namespace BoboEngine;

public struct Float4(float x = 0, float y = 0, float z = 0, float w = 0)
{
    public float x = x;
    public float y = y;
    public float z = z;
    public float w = w;

    public float r { get => x; set => x = value; }
    public float g { get => y; set => y = value; }
    public float b { get => z; set => z = value; }
    public float a { get => w; set => w = value; }

    #region variants
    public static Float4 white => new(1, 1, 1, 1);
    public static Float4 black => new(0, 0, 0, 1);
    public static Float4 red => new(1, 0, 0, 1);
    public static Float4 green => new(0, 1, 0, 1);
    public static Float4 blue => new(0, 0, 1, 1);

    public static Float4 xAxis => new(1, 0, 0, 1);
    public static Float4 yAxis => new(0, 1, 0, 1);
    public static Float4 zAxis => new(0, 0, 1, 1);


    public static Float4 zero => new(0, 0, 0, 1);
    public static Float4 one => new(1, 1, 1, 1);
    #endregion

    public static Float4 Lerp(Float4 s, Float4 e, float t)
    {
        return (e - s) * t + s;
    }

    public static Float4 operator *(Float4 f, float v) => new Float4(f.x * v, f.y * v, f.z * v, f.w * v);
    public static Float4 operator -(Float4 a, Float4 b) => new Float4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
    public static Float4 operator +(Float4 a, Float4 b) => new Float4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);

    public static explicit operator Float3(Float4 v) => new Float3(v.x, v.y, v.z);
    public override string ToString() => $"({x.ToString()},{y.ToString()},{z.ToString()},{w.ToString()})";
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

    public float Length => MathF.Sqrt(Dot(this, this));
    public float LengthSquared() => Dot(this, this);
    public Float3 Normalized()
    {
        float l = Length;

        if (l == 0) // Prevent NAN
        {
            return zero;
        }

        return this / Length;
    }
    
    public readonly float GetAxis(Axis axis)
    {
        return axis switch
        {
            Axis.Yaxis => y,
            Axis.Xaxis => x,
            Axis.Zaxis => z,
            _ => 0,
        };
    }
    public readonly Float3 With(Axis axis, float value)
    {
        float x = axis == Axis.Xaxis ? value : this.x;
        float y = axis == Axis.Yaxis ? value : this.y;
        float z = axis == Axis.Zaxis ? value : this.z;
        return new Float3(x, y, z);
    }

    public static float Dot(Float3 a, Float3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
    public static Float3 CrossProduct(Float3 a, Float3 b) => new(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    public static Float3 Lerp(Float3 s, Float3 e, float t) => (e - s) * t + s;
    public static Float3 operator +(Float3 a, Float3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z); // Vector Adding
    public static Float3 operator -(Float3 a, Float3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z); // Vector Subracting
    public static Float3 operator -(float a, Float3 b) => new(a - b.x, a - b.y, a - b.z); // Subracting

    public static Float3 operator *(Float3 a, Float3 b) => new(a.x * b.x, a.y * b.y, a.z * b.z); // Vector Multiplication
    public static Float3 operator /(Float3 a, Float3 b) => new(a.x / b.x, a.y / b.y, a.z / b.z); // Vector Division
    public static Float3 operator *(Float3 v, float f) => new(v.x * f, v.y * f, v.z * f); // Vector Length Multiplication
    public static Float3 operator /(Float3 v, float f) => new(v.x / f, v.y / f, v.z / f); // Vector Length Division
    public static Float3 operator /(float f, Float3 v) => new(f / v.x, f / v.y, f / v.z); // Vector Division
    public static Float3 operator -(Float3 a) => new(-a.x, -a.y, -a.z); // Negative Vector
    public static bool operator ==(Float3 a, Float3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
    public static bool operator !=(Float3 a, Float3 b) => a.x != b.x || a.y != b.y || a.z != b.z;

    public override string ToString() => $"({x.ToString()},{y.ToString()},{z.ToString()})";
    public Int3 RoundToInt3()
    {
        return new((int)float.Round(x), (int)float.Round(y), (int)float.Round(z));
    }

    public static explicit operator Float3(float[] f) => new(f[0], f[1], f[2]);
    public static explicit operator Float3(Double3 d) => new((float)d.x, (float)d.y, (float)d.z);

    public static implicit operator Float3(Int3 i) => new(i.x, i.y, i.z);

    public static implicit operator Vector3(Float3 f) => new(f.x, f.y, f.z);
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

    public float Length => MathF.Sqrt(x * x + y * y);
    public float LengthSquared() => x * x + y * y;
    public Float2 Normalized() => this / Length;
    public Float2 Cross => new(y, -x);
    public static float Dot(Float2 a, Float2 b) => a.x * b.x + a.y * b.y;
    public static float Determinant(Float2 a, Float2 b) => a.x * b.y - a.y * b.x;

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
public struct UVRect(float uMin = 0, float vMin = 0, float uMax = 1, float vMax = 1)
{
    public float uMin = uMin;
    public float vMin = vMin;
    public float uMax = uMax;
    public float vMax = vMax;

    public static UVRect Unit => new(0, 0, 1, 1);

    public Float2 GetMin() => new(uMin, vMin);
    public Float2 GetMax() => new(uMax, vMax);

    public static UVRect operator /(UVRect v, float f) => new(v.uMin / f, v.vMin / f, v.uMax / f, v.vMax / f);

    public static bool operator ==(UVRect v1, UVRect v2) => v1.uMin == v2.uMin && v1.vMin == v2.vMin && v1.uMax == v2.uMax && v1.vMax == v2.vMax;
    public static bool operator !=(UVRect v1, UVRect v2) => v1.uMin != v2.uMin || v1.vMin != v2.vMin || v1.uMax != v2.uMax || v1.vMax != v2.vMax;

    public UVRect FlipV() => new UVRect(uMin, 1 - vMax, uMax, 1 - vMin);

    public override string ToString()
    {
        return $"[{uMin}, {vMin}, {uMax}, {vMax}]";
    }
}

public struct BaseVectors(Float3 leftVector, Float3 upVector, Float3 forwardVector)
{
    public Float3 leftVector { get; private set; } = leftVector;
    public Float3 upVector { get; private set; } = upVector;
    public Float3 forwardVector { get; private set; } = forwardVector;

    public Float3 TransformVector(Float3 point)
    {
        Engine.Log(upVector);
        return leftVector * point.x + upVector * point.y + forwardVector * point.z;
    }
    public Double3 TransformVector(Double3 point)
    {
        return (Double3)leftVector * point.x + (Double3)upVector * point.y + (Double3)forwardVector * point.z;
    }
    public BaseVectors TransformByBaseVectors(BaseVectors baseVectors)
    {
        return new(
            baseVectors.TransformVector(leftVector),
            baseVectors.TransformVector(upVector),
            baseVectors.TransformVector(forwardVector));
    }

    public BaseVectors RotateXBy(float angle)
    {
        return TransformByBaseVectors(FromXRotation(angle));
    }
    public BaseVectors RotateYBy(float angle)
    {
        return TransformByBaseVectors(FromYRotation(angle));
    }
    public BaseVectors RotateZBy(float angle)
    {
        return TransformByBaseVectors(FromZRotation(angle));
    }

    public static BaseVectors FromRotation(Float3 rotation)
    {
        // --- Apply Y Rotation ---
        BaseVectors yBaseVectors = FromYRotation(rotation.y);

        // --- Apply X Rotation ---
        BaseVectors xBaseVectors = FromXRotation(rotation.x);

        // --- Apply Z Rotation ---
        BaseVectors zBaseVectors = FromZRotation(rotation.z);

        // --- Combied Vectors ---

        Float3 leftVector = yBaseVectors.TransformVector(xBaseVectors.TransformVector(zBaseVectors.leftVector));
        Float3 upVector = yBaseVectors.TransformVector(xBaseVectors.TransformVector(zBaseVectors.upVector));
        Float3 forwardVector = yBaseVectors.TransformVector(xBaseVectors.TransformVector(zBaseVectors.forwardVector));

        return new(leftVector, upVector, forwardVector);
    }
    public static BaseVectors FromYRotation(float rotation)
    {
        return new(
            new Float3(Maths.Cos(rotation), 0, Maths.Sin(rotation)),
            Float3.yAxis,
            new(Maths.Sin(-rotation), 0, Maths.Cos(rotation))
            );
    }
    public static BaseVectors FromXRotation(float rotation)
    {
        return new(
            Float3.xAxis,
            new(0, Maths.Cos(rotation), Maths.Sin(rotation)),
            new(0, Maths.Sin(-rotation), Maths.Cos(rotation))
            );
    }
    public static BaseVectors FromZRotation(float rotation)
    {
        return new(
            new Float3(Maths.Cos(rotation), Maths.Sin(-rotation), 0),
            new(Maths.Sin(rotation), Maths.Cos(rotation), 0),
            Float3.zAxis
            );
    }

    public static BaseVectors FacingTowards(Float3 point, Float3 up) // Have to swap point and up????
    {
        Float3 forwardVector = point.Normalized();
        Float3 leftVector = Float3.CrossProduct(forwardVector, up).Normalized();
        Float3 upVector = Float3.CrossProduct(leftVector, forwardVector).Normalized();

        return new BaseVectors(leftVector, upVector, forwardVector);
    }
}

public struct BaseVectors2D(Float2 leftVector, Float2 upVector)
{
    public Float2 leftVector { get; private set; } = leftVector;
    public Float2 upVector { get; private set; } = upVector;

    public Float2 TransformVector(Float2 point)
    {
        return leftVector * point.x + upVector * point.y;
    }

    public static BaseVectors2D FromRotation(float a)
    {
        return new(
            new(-Maths.Sin(a), Maths.Cos(a)),
            new(Maths.Cos(a), Maths.Sin(a))
            );
    }
}