using static OpenGL.GL;

namespace BoboEngine;

public class Mesh
{
    public Float3[] vertices;
    public FaceInfo[] faces;
    public Float3[] normals;
    //public Float3[] faceColors;
    public Float2[] textureCoords;

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

        List<Float3> vertices = new();
        List<FaceInfo> faces = new();
        List<Float3> normals = new();
        //List<Float3> faceColors = new();
        List<Float2> textureCoords = new();

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
                foreach (var f in FaceInfo.GetTriangulatedFaces(line))
                {
                    var item = f;

                    faces.Add(item);
                }
            }
        }

        this.vertices = vertices.ToArray();
        this.faces = faces.ToArray();
        //this.faceColors = faceColors.ToArray();
        this.normals = normals.ToArray();
        this.textureCoords = textureCoords.ToArray();

        return true;
    }
    public void LoadRawData(Float3[] vertices, FaceInfo[] faces = null, Float3[] normals = null, Float2[] textureCoords = null)
    {
        if (!IsEmpty())
        {
            Engine.LogWarning("Cannot load data! Data is already loaded!");
            return;
        }

        this.vertices = vertices;
        this.faces = faces ?? ([]);
        this.normals = normals ?? ([]);
        this.textureCoords = textureCoords ?? ([]);
    }
    public virtual bool IsEmpty()
    {
        if (vertices == null) return true;
        else if (vertices.Length == 0) return true;

        if (faces == null) return true;
        else if (faces.Length == 0) return true;

        return false;
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

        fixed (float* ptrVertices = &vertexData[0])
        {
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertexData.Length, ptrVertices, GL_STATIC_DRAW);
        }

        glBindPointers();

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);
    }
    protected virtual (float[] data, uint vertexBufferSize) glConvertToData()
    {
        var vertexData = new float[faces.Length * 24];

        for (int i = 0; i < faces.Length; i++)
        {
            var face = faces[i];

            var faceIndex = i * 24; // 8 * 3

            for (int vertexI = 0; vertexI < 3; vertexI++)
            {
                Float3 position = vertices[face.vertex_indexs[vertexI]];
                Float3 normal = face.normal_indexs.Count > vertexI ? normals[face.normal_indexs[vertexI]] : new();
                Float2 uv = textureCoords.Length > 0 ? textureCoords[face.texture_indexs[vertexI]] : new(0, 0);

                vertexData[faceIndex + 0] = position.x;
                vertexData[faceIndex + 1] = position.y;
                vertexData[faceIndex + 2] = position.z;

                vertexData[faceIndex + 3] = normal.x;
                vertexData[faceIndex + 4] = normal.y;
                vertexData[faceIndex + 5] = normal.z;

                vertexData[faceIndex + 6] = uv.x;
                vertexData[faceIndex + 7] = uv.y;

                faceIndex += 8;
            }
        }

        return (vertexData, (uint)faces.Length * 3);
    }
    protected virtual unsafe void glBindPointers()
    {
        // Position (x,y,z)
        glVertexAttribPointer(0, 3, GL_FLOAT, false, 8 * sizeof(float), (void*)0);
        glEnableVertexAttribArray(0);

        // Normals (x,y,z)
        glVertexAttribPointer(1, 3, GL_FLOAT, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
        glEnableVertexAttribArray(1);

        // Vertex Texture Coords (u,v)
        glVertexAttribPointer(2, 2, GL_FLOAT, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
        glEnableVertexAttribArray(2);
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

        vertices = null;
        faces = null;
        normals = null;
        textureCoords = null;

        vao = 0;
        vbo = 0;
    }
    #endregion


    public static implicit operator bool(Mesh o) => o != null;
}

public class FaceInfo
{
    public List<int> vertex_indexs = new();
    public List<int> texture_indexs = new();
    public List<int> normal_indexs = new();

    public FaceInfo(string objFaceElements)
    {
        string[] elements = objFaceElements.Split(' ')[1..];

        foreach (var vertexInfo in elements)
        {
            int[] indexes = vertexInfo.Split('/').Select(int.Parse).ToArray();

            if (indexes.Length >= 1)
            {
                vertex_indexs.Add(indexes[0] - 1);
            }

            if (indexes.Length >= 2)
            {
                texture_indexs.Add(indexes[1] - 1);
            }

            if (indexes.Length >= 3)
            {
                normal_indexs.Add(indexes[2] - 1);
            }

            //throw new FormatException("Could not parse OBJ, Face elements was incomplete!");
        }
    }

    /// <summary>
    /// <br/>
    /// "f vert/tex/norm ..." <br/>
    /// E.g. "f 1/1/1 2/2/2 3/3/3 4/4/4 5/5/5"
    /// </summary>
    public static FaceInfo[] GetTriangulatedFaces(string objFaceElements)
    {
        string[] elements = objFaceElements.Split(' ')[1..];

        List<FaceInfo> faces = new();

        if (elements.Length <= 3)
        {
            faces = [new(objFaceElements)];
        }
        else
        {
            for (int i = 2; i < elements.Length; i++)
            {
                faces.Add(new($"f {elements[0]} {elements[i - 1]} {elements[i]}"));
            }
        }

        return faces.ToArray();
    }
}