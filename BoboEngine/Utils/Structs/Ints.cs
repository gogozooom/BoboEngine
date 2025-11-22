namespace BoboEngine;

public struct Int3(int x = 0, int y = 0, int z = 0)
{
    public int x = x;
    public int y = y;
    public int z = z;

    public int a { get => x; set => x = value; }
    public int b { get => y; set => y = value; }
    public int c { get => z; set => z = value; }

    public static Int3 operator +(Int3 v1, Int3 v2) => new(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    public static Int3 operator -(Int3 v1, Int3 v2) => new(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    public static Int3 operator *(Int3 v1, Int3 v2) => new(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    public static Int3 operator -(Int3 v, int i) => new(v.x - i, v.y - i, v.z - i);
    public static Int3 operator *(Int3 v, int i) => new(v.x * i, v.y * i, v.z * i); // Length Multiplication

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
    public static Int2 operator -(Int2 a, Int2 b) => new(a.x - b.x, a.y - b.y); // Subtracting

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