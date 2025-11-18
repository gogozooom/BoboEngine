using System.Numerics;
using static OpenGL.GL;

namespace BoboEngine.Shaders;

public class Shader
{
    // TODO: put in material class
    public Texture texture;
    public bool cullBackFaces = true;

    public string vertexFilepath;
    public string fragmentFilepath;

    private uint shaderRef;

    private string _vertexCode;
    private string _fragmentCode;

    public Shader(string vertexCode, string fragmentCode)
    {
        _vertexCode = vertexCode;
        _fragmentCode = fragmentCode;
        cullBackFaces = true;
    }
    public Shader(Texture texture = null, string _vertexFilepath = "Shader/modelShader.vert", string _fragmentFilepath = "Shader/modelShader.frag")
    {
        vertexFilepath = _vertexFilepath;
        fragmentFilepath = _fragmentFilepath;

        _vertexCode = "";
        _fragmentCode = "";
        
        try
        {
            _vertexCode = File.ReadAllText(vertexFilepath);
            _fragmentCode = File.ReadAllText(fragmentFilepath);

        }
        catch (Exception e)
        {
            Console.WriteLine("Could not read shader files, program will exit\n\n{0}", e);
            Environment.Exit(0);
            return;
        }

        this.texture = texture;
        cullBackFaces = true;
    }

    public void CreateShader()
    {
        if (!WindowManager.Initialized)
        {
            Program.LogError("Cannot CreateShader without a window!");
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

    public void Bind()
    {
        if (shaderRef == 0) CreateShader();

        glUseProgram(shaderRef);
    }

    public void Unbind()
    {
        glUseProgram(0);
    }

    public void Delete()
    {
        glDeleteShader(shaderRef);
    }

    public void SetMatrix4x4(string uniformName, Matrix4x4 input)
    {
        int location = glGetUniformLocation(shaderRef, uniformName);

        glUniformMatrix4fv(location, 1, false, MatrixToData(input));
    }

    public void SetVec2(string uniformName, Float2 input)
    {
        int location = glGetUniformLocation(shaderRef, uniformName);

        glUniform2f(location, input.x, input.y);
    }

    public float[] MatrixToData(Matrix4x4 m) =>
    [
        m.M11, m.M12, m.M13, m.M14,
        m.M21, m.M22, m.M23, m.M24,
        m.M31, m.M32, m.M33, m.M34,
        m.M41, m.M42, m.M43, m.M44
    ];
}