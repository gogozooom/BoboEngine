using static OpenGL.GL;

namespace BoboEngine;

public class Mesh
{
    public VertexData vertexData;
    public VertexMapper vertexMap;

    public VertexAttribute<Float3> Vertices => vertexData.GetAttribute<Float3>("vertices");
    public VertexAttribute<Float2> TextureCoords => vertexData.GetAttribute<Float2>("textureCoords");
    public VertexAttribute<Float3> Normals => vertexData.GetAttribute<Float3>("normals");

    public bool LoadObjFile(string filePath)
    {
        // -- Error Checks --
        if (string.IsNullOrEmpty(filePath))
        {
            Engine.LogError($"Please specify a file to load!");
            return false;
        }
        if (!File.Exists(filePath))
        {
            Engine.LogError($"Could find model file '{filePath}'");
            return false;
        }

        string[] fileNameL = filePath.Split('.');

        string fileName = fileNameL[fileNameL.Length - 1];

        if (fileName != "obj")
        {
            Engine.LogError($"'{fileName}' file type unsupported! Please supply an .obj file!");
            return false;
        }

        vertexData = new();
        vertexMap = new();

        var vertices = vertexData.AddAttribute<Float3>("vertices");
        var textureCoords = vertexData.AddAttribute<Float2>("textureCoords");
        var normals = vertexData.AddAttribute<Float3>("normals");

        string[] data = File.ReadAllLines(filePath);

        foreach (var line in data)
        {
            if (line.StartsWith("v ")) // Vertex Info
            {
                float[] axes = line[2..].Split(' ').Select(float.Parse).ToArray();
                vertices.Add(new(axes[0], axes[1], axes[2])); // Flip x to make positive x go right
            }
            else if (line.StartsWith("vn ")) // Face Normals
            {
                float[] axes = line[3..].Split(' ').Select(float.Parse).ToArray(); // Have to start with a 3???
                normals.Add(new(axes[0], axes[1], axes[2]));
            }
            else if (line.StartsWith("vt ")) // Vertex Texture Chords
            {
                float[] axes = line[3..].Split(' ').Select(float.Parse).ToArray(); // This one too????
                textureCoords.Add(new(axes[0], axes[1]));
            }
            else if (line.StartsWith("f ")) // Face Info
            {
                vertexMap.Add(line);
            }
        }

        return true;
    }
    public void LoadRawData(VertexData data, VertexMapper mapper)
    {
        if (!IsEmpty())
        {
            Engine.LogWarning("Cannot load data! Data is already loaded!");
            return;
        }

        vertexData = data;
        vertexMap = mapper;
    }
    public virtual bool IsEmpty()
    {
        return vertexData == null || vertexData.IsEmpty() || vertexMap.IsEmpty();
    }

    #region OpenGL
    protected unsafe void InitalizeOpenGL()
    {
        if (IsEmpty())
        {
            Engine.LogWarning("Mesh Empty! Skipping OpenGl Binding");
            return;
        }

        if (!WindowManager.Initialized)
        {
            Engine.LogError("Cannot bind open gl without a window!");
            return;
        }

        // OpenGl Time

        (var vertexData, vertexBufferSize) = glConvertToData();

        vao = glGenVertexArray();
        vbo = glGenBuffer();

        glBindVertexArray(vao);
        glBindBuffer(GL_ARRAY_BUFFER, vbo);

        fixed (byte* ptrVertices = &vertexData[0])
        {
            glBufferData(GL_ARRAY_BUFFER, vertexData.Length, ptrVertices, GL_STATIC_DRAW);
        }

        glBindPointers();

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);
    }
    protected virtual (byte[] data, uint vertexBufferSize) glConvertToData()
    {
        var triangulatedMap = vertexMap.Triangulated();
        
        return (triangulatedMap.glConvertToData(vertexData), (uint)triangulatedMap.Count());
    }
    protected virtual void glBindPointers()
    {
        vertexData.glBindPointers();
    }
    public virtual void glDraw()
    {
        glDrawArrays(GL_TRIANGLES, 0, (int)vertexBufferSize);
    }
    /// <summary>
    /// Vertex Array Object Reference
    /// </summary>

    protected uint vao;
    /// <summary>
    /// Vertex Buffer Object Reference
    /// </summary>
    protected uint vbo;
    protected uint vertexBufferSize;

    /// <summary>
    /// Bind "Vertex Buffer Object"
    /// </summary>
    public void glBindVAO()
    {
        if (vao == 0) InitalizeOpenGL();

        glBindVertexArray(vao);
    }
    /// <summary>
    /// Unbind "Vertex Buffer Object"
    /// </summary>
    public void glUnBindVAO()
    {
        glBindVertexArray(0);
    }
    public void DeleteMesh()
    {
        if (vao != 0)
        {
            glDeleteBuffer(vbo);
            glDeleteVertexArray(vao);
        }

        vertexData = null;
        vertexMap = new();

        vao = 0;
        vbo = 0;
    }
    #endregion


    public static implicit operator bool(Mesh o) => o != null;
}