using BoboEngine.Shaders;

namespace BoboEngine;
public class Material
{
    public string shaderID { get => _shaderID; set => SetShaderID(value); }
    private string _shaderID;

    public Shader shader { get; private set; }

    public Texture texture;

    public RenderTranformMode transformMode;
    public BlendMode blendMode;
    public bool useDepth = true;
    public bool cullBackFaces = true;
    public int renderOrder;

    public Material(string shaderID = "default", Texture texture = null, bool cullBackFaces = true, bool useDepth = true, BlendMode blendMode = BlendMode.Normal, RenderTranformMode transformMode = RenderTranformMode.Global, int renderOrder = 0)
    {
        SetShaderID(shaderID);
        this.texture = texture;
        this.cullBackFaces = cullBackFaces;
        this.useDepth = useDepth;
        this.blendMode = blendMode;
        this.transformMode = transformMode;
        this.renderOrder = renderOrder;
    }

    public void SetShaderID(string v)
    {
        _shaderID = v;

        shader = ShaderManager.GetShader(_shaderID);

        if(shader == null)
        {
            Engine.LogError($"ShaderID of '{v}' does not exist! Please use ShaderManager.Ensure/CreateShader() to avoid this!");
            return;
        }
    }

    #region GL Shader Property Buffer

    private readonly Dictionary<string, int> shaderProps_Int = new();
    private readonly Dictionary<string, Float4> shaderProps_Vec4 = new();
    private readonly Dictionary<string, Float3> shaderProps_Vec3 = new();
    private readonly Dictionary<string, Float2> shaderProps_Vec2 = new();
    private readonly Dictionary<string, float> shaderProps_Float = new();

    public void GlBindShaderProperties()
    {
        foreach (var item in shaderProps_Int)
        {
            shader.glSetInt(item.Key, item.Value);
        }
        foreach (var item in shaderProps_Vec4)
        {
            shader.glSetVec4(item.Key, item.Value);
        }
        foreach (var item in shaderProps_Vec3)
        {
            shader.glSetVec3(item.Key, item.Value);
        }
        foreach (var item in shaderProps_Vec2)
        {
            shader.glSetVec2(item.Key, item.Value);
        }
        foreach (var item in shaderProps_Float)
        {
            shader.glSetFloat(item.Key, item.Value);
        }
    }

    public void SetInt(string id, int value)
    {
        if (shaderProps_Int.ContainsKey(id))
            shaderProps_Int[id] = value;
        else
            shaderProps_Int.Add(id, value);
    }
    public void SetVec4(string id, Float4 value)
    {
        if (shaderProps_Vec4.ContainsKey(id))
            shaderProps_Vec4[id] = value;
        else
            shaderProps_Vec4.Add(id, value);
    }
    public void SetVec3(string id, Float3 value)
    {
        if (shaderProps_Vec3.ContainsKey(id))
            shaderProps_Vec3[id] = value;
        else
            shaderProps_Vec3.Add(id, value);
    }
    public void SetVec2(string id, Float2 value)
    {
        if (shaderProps_Vec2.ContainsKey(id))
            shaderProps_Vec2[id] = value;
        else
            shaderProps_Vec2.Add(id, value);
    }
    public void SetFloat(string id, float value)
    {
        if (shaderProps_Float.ContainsKey(id))
            shaderProps_Float[id] = value;
        else
            shaderProps_Float.Add(id, value);
    }

    #endregion
}

public enum RenderTranformMode
{
    Global,
    LocalTransform,
    LocalRotation,
    Local
}
public enum BlendMode
{
    Normal,
    Blend,
    Invert,
    Disable
}