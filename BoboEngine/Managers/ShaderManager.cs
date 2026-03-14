namespace BoboEngine.Shaders;

public static class ShaderManager
{
    private static Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>{ { "default", new Shader()} };

    public static Shader GetShader(string name)
    {
        _shaders.TryGetValue(name, out var value);

        return value;
    }

    public static Shader EnsureShader(string name, string vertexFilepath = "Shader/modelShader.vert", string fragmentFilepath = "Shader/modelShader.frag")
    {
        var shader = GetShader(name);

        shader ??= CreateShader(name, vertexFilepath, fragmentFilepath);

        return shader;
    }

    public static Shader CreateShader(string name, string vertexFilepath = "Shader/modelShader.vert", string fragmentFilepath = "Shader/modelShader.frag")
    {
        if (name.Trim() == "")
        {
            Engine.LogError("Cannot create a shader with an empty name!");
            return null;
        }
        if (_shaders.ContainsKey(name))
        {
            Engine.LogError($"Shader already loaded with name '{name}'");
            return null;
        }

        var shader = new Shader(vertexFilepath, fragmentFilepath);

        _shaders.Add(name, shader);
        return shader;
    }
}
