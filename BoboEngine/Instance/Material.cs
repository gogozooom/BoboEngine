using BoboEngine.Shaders;

namespace BoboEngine;
public class Material
{
    public string shaderID { get => _shaderID; set => SetShaderID(value); }
    private string _shaderID;

    public Shader shader { get; private set; }

    public Texture texture;

    public bool cullBackFaces = true;
    public int renderOrder;

    public Material(string shaderID = "default", Texture texture = null, bool cullBackFaces = true, int renderOrder = 0)
    {
        SetShaderID(shaderID);
        this.texture = texture;
        this.cullBackFaces = cullBackFaces;
        this.renderOrder = renderOrder;
    }

    public void SetShaderID(string v)
    {
        _shaderID = v;

        shader = ShaderManager.GetShader(_shaderID);

        if(shader == null)
        {
            Program.LogError($"ShaderID of '{v}' does not exist! Please use ShaderManager.Ensure/CreateShader() to avoid this!");
            return;
        }
    }
}
