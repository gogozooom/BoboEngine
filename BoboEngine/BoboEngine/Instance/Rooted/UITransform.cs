using System.Numerics;

namespace BoboEngine.UI;

public class UITransform : ObjectBehavior
{
    /// <summary>
    /// Offset of the UI element in pixels
    /// </summary>
    public Float2 position { get => _position; set => UpdatePosition(value); }
    private Float2 _position;
    /// <summary>
    /// Scale offset of the UI element in pixels
    /// </summary>
    public Float2 scale { get => _scale; set => UpdateScale(value); }
    private Float2 _scale;
    /// <summary>
    /// Where the element gets scaled from
    /// (0, 0) - Bottom Left |
    /// (1, 1) - top Right |
    /// </summary>
    public Float2 pivot { get => _pivot; set => UpdatePivot(value); }
    private Float2 _pivot;
    /// <summary>
    /// Where the UI element is placed relative to the screen
    /// (0, 0, 0, 0) - Bottom Left |
    /// (0.5, 0.5, 0.5, 0.5) - Center |
    /// (1, 1, 1, 1) - Top Right |
    /// (0, 0, 1, 1) - Cover the whole screen |
    /// </summary>
    public UVRect anchor { get => _anchor; set => UpdateAnchor(value); }
    private UVRect _anchor;

    public Int2 pixelScale => GetPixelScale(scale, anchor);

    public Action onTransformUpdate;


    #region Variable Update Functions
    public void UpdatePosition(Float2 v)
    {
        _position = v;
        onTransformUpdate?.Invoke();
    }
    public void UpdateScale(Float2 v)
    {
        _scale = v;
        onTransformUpdate?.Invoke();
    }
    public void UpdatePivot(Float2 v)
    {
        _pivot = v;
        onTransformUpdate?.Invoke();
    }
    public void UpdateAnchor(UVRect v)
    {
        _anchor = v;
        onTransformUpdate?.Invoke();
    }
    #endregion

    public UITransform()
    {
        scale = new Float2(100f, 100f);
        pivot = new Float2(0.5f, 0.5f);
        anchor = new UVRect(0.5f, 0.5f, 0.5f, 0.5f);
    }

    public static Int2 GetPixelScale(Float2 scale, UVRect anchor)
    {
        return (Int2)(new Double2(anchor.width, anchor.height) * WindowManager.WindowSize + new Double2(scale.x, scale.y));
    }

    public static Matrix4x4 CreateMatrixTrans(Float2 position, Float2 scale, Float2 pivot, UVRect anchor)
    {
        var screenPosition = new Double2(position.x - scale.x * pivot.x, position.y - scale.y * pivot.y); // Pixel Position (With pivot offset applied)

        screenPosition *= 2 / WindowManager.WindowSize; // To OpenGl View Frustrum (-1 to 1)

        screenPosition += anchor.GetMin()*2 - 1; // Applies anchor offset

        return Matrix4x4.CreateTranslation(new Vector3((float)screenPosition.x, (float)screenPosition.y, 0));
    }

    public Matrix4x4 MatrixTrans => CreateMatrixTrans(position, scale, pivot, anchor);

    public static Matrix4x4 CreateMatrixScale(Float2 scale, UVRect anchor)
    {
        var screenSize = new Double2(scale.x, scale.y); // Pixel Scale

        screenSize *= 1 / WindowManager.WindowSize; // To OpenGl View Frustrum (-1 to 1)

        screenSize += new Double2(anchor.width, anchor.height); // Extra Anchor Scale

        screenSize *= 4; // To scale size from center to (-1 to 1) view frustrum

        return Matrix4x4.CreateScale(new Vector3((float)screenSize.x, (float)screenSize.y, 1));
    }

    public Matrix4x4 MatrixScale => CreateMatrixScale(scale, anchor);

    public Matrix4x4 Matrix => MatrixScale * MatrixTrans;
}