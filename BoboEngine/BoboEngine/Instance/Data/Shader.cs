using System.Numerics;
using static OpenGL.GL;

namespace BoboEngine.Shaders;

public class Shader
{
    public bool isLoaded => _vertexCode != null && _fragmentCode != null;
    public bool isCompiled => shaderRef != 0;

    public string vertexFilepath;
    public string fragmentFilepath;

    private uint shaderRef;

    private string _vertexCode;
    private string _fragmentCode;

    public Shader(string _vertexFilepath = "Shader/modelShader.vert", string _fragmentFilepath = "Shader/modelShader.frag")
    {
        if (!File.Exists(_vertexFilepath))
        {
            Engine.LogError($"File path for '{_vertexFilepath}' does not exist!");
            return;
        }
        if (!File.Exists(_fragmentFilepath))
        {
            Engine.LogError($"File path for '{_fragmentFilepath}' does not exist!");
            return;
        }

        vertexFilepath = _vertexFilepath;
        fragmentFilepath = _fragmentFilepath;

        try
        {
            _vertexCode = File.ReadAllText(vertexFilepath);
            _fragmentCode = File.ReadAllText(fragmentFilepath);

        }
        catch (Exception e)
        {
            Engine.LogError("Could not load shaders!");
            Engine.LogError(e.Message);
            return;
        }
    }

    public void CreateShader()
    {
        if (!WindowManager.Initialized)
        {
            Engine.LogError("Cannot CreateShader without a window!");
            return;
        }

        if(_vertexCode == null || _vertexCode == "")
        {
            Engine.LogError($"'{this}' No vertex shader loaded!");
            return;
        }
        if(_fragmentCode == null || _fragmentCode == "")
        {
            Engine.LogError($"'{this}'No fragment shader loaded!");
            return;
        }

        shaderRef = glCreateProgram();
        uint _vs = CompileShader(_vertexCode, GL_VERTEX_SHADER);
        uint _fs = CompileShader(_fragmentCode, GL_FRAGMENT_SHADER);

        glAttachShader(shaderRef, _vs);
        glAttachShader(shaderRef, _fs);

        glLinkProgram(shaderRef);

        glDetachShader(shaderRef, _vs);
        glDetachShader(shaderRef, _fs);

        glDeleteShader(_vs);
        glDeleteShader(_fs);

        //Program.Log($"[{this}] CreateShader Success");
    }

    private unsafe uint CompileShader(string _code, int _type)
    {
        uint _id = glCreateShader(_type);

        glShaderSource(_id, _code);
        glCompileShader(_id);

        // check for errors
        int _result;
        glGetShaderiv(_id, GL_COMPILE_STATUS, &_result);
        if(_result != GL_TRUE)
        {
            string _msg = glGetShaderInfoLog(_id);
            Console.WriteLine("Shader error: " + _msg + "\n\n src: " + _code);
            Environment.Exit(0);
            return 0;
        }

        return _id;
    }

    public void glBind()
    {
        if (!isCompiled) CreateShader();

        glUseProgram(shaderRef);
    }

    public void glUnbind()
    {
        glUseProgram(0);
    }

    public void Delete()
    {
        glDeleteShader(shaderRef);
    }

    public void glSetMatrix4x4(string uniformName, Matrix4x4 input)
    {
        int location = glGetUniformLocation(shaderRef, uniformName);

        glUniformMatrix4fv(location, 1, false, MatrixToData(input));
    }
    public void glSetInt(string uniformName, int input)
    {
        int location = glGetUniformLocation(shaderRef, uniformName);

        glUniform1i(location, input);
    }
    public void glSetVec4(string uniformName, Float4 input)
    {
        int location = glGetUniformLocation(shaderRef, uniformName);

        glUniform4f(location, input.x, input.y, input.z, input.w);
    }
    public void glSetVec3(string uniformName, Float3 input)
    {
        int location = glGetUniformLocation(shaderRef, uniformName);

        glUniform3f(location, input.x, input.y, input.z);
    }
    public void glSetVec2(string uniformName, Float2 input)
    {
        int location = glGetUniformLocation(shaderRef, uniformName);

        glUniform2f(location, input.x, input.y);
    }
    public void glSetFloat(string uniformName, float input)
    {
        int location = glGetUniformLocation(shaderRef, uniformName);

        glUniform1f(location, input);
    }


    public float[] MatrixToData(Matrix4x4 m) =>
    [
        m.M11, m.M12, m.M13, m.M14,
        m.M21, m.M22, m.M23, m.M24,
        m.M31, m.M32, m.M33, m.M34,
        m.M41, m.M42, m.M43, m.M44
    ];
}