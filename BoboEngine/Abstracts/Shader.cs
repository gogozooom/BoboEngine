using static OpenGL.GL;

namespace BoboEngine.Shaders;

public class Shader
{
    public string vertexFilepath;
    public string fragmentFilepath;

    private uint programID;

    public Shader(string _vertexFilepath, string _fragmentFilepath)
    {
        vertexFilepath = _vertexFilepath;
        fragmentFilepath = _fragmentFilepath;

        string _vertexCode = "";
        string _fragmentCode = "";

        try
        {
            _vertexCode = File.ReadAllText(vertexFilepath);
            _fragmentCode = File.ReadAllText(fragmentFilepath);

        } catch (Exception e)
        {
            Console.WriteLine("Could not read shader files, program will exit\n\n{0}", e);
            Environment.Exit(0);
            return;
        }

        CreateShader(_vertexCode, _fragmentCode);
    }

    ~Shader()
    {
        Delete();
    }

    private void CreateShader(string _vertexCode, string _fragmentCode)
    {
        programID = glCreateProgram();
        uint _vs = CompileShader(_vertexCode, GL_VERTEX_SHADER);
        uint _fs = CompileShader(_fragmentCode, GL_FRAGMENT_SHADER);

        glAttachShader(programID, _vs);
        glAttachShader(programID, _fs);

        glLinkProgram(programID);

        glDetachShader(programID, _vs);
        glDetachShader(programID, _fs);

        glDeleteShader(_vs);
        glDeleteShader(_fs);
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
        glUseProgram(programID);
    }

    public void Unbind()
    {
        glUseProgram(0);
    }

    public void Delete()
    {
        glDeleteShader(programID);
    }

    //public abstract Float3 PixelColor(Float2 pixelCoord, Float2 texCoord, Float3 normal, float depth, Transform transform = null);
}
