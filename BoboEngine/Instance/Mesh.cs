using static OpenGL.GL;

namespace BoboEngine;
public class Mesh : ObjectBehavior, IComparable<Mesh>
{
    /// <summary>
    /// Vertex Array Object Reference
    /// </summary>
    private uint vao;
    /// <summary>
    /// Vertex Buffer Object Reference
    /// </summary>
    private uint vbo;

    public uint vertexBufferSize { get; private set; }

    public Float3[] vertices;
    public FaceInfo[] faces;
    public Float3[] normals;
    public Float3[] faceColors;
    public Float2[] textureCoords;

    public Material material;

    public Mesh()
    {
        material = new();
    }

    public bool LoadObjFile(string filePath, int textureID = 0)
    {
        // -- Error Checks --
        if (string.IsNullOrEmpty(filePath))
        {
            Program.LogError($"Please specify a file to load!");
            return false;
        }
        if (!File.Exists(filePath))
        {
            Program.LogError($"Could find model file '{filePath}'");
            return false;
        }

        string[] fileNameL = filePath.Split('.');

        string fileName = fileNameL[fileNameL.Length - 1];

        if (fileName != "obj")
        {
            Program.LogError($"'{fileName}' file type unsupported! Please supply an .obj file!");
            return false;
        }

        List<Float3> vertices = new();
        List<FaceInfo> faces = new();
        List<Float3> normals = new();
        List<Float3> faceColors = new();
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
            else if(line.StartsWith("vt ")) // Vertex Texture Chords
            {
                float[] axes = line[3..].Split(' ').Select(float.Parse).ToArray(); // This one too????
                textureCoords.Add(new(axes[0], axes[1]));
            }
            else if (line.StartsWith("f ")) // Face Info
            {
                foreach (var f in FaceInfo.GetTriangulatedFaces(line))
                {
                    var item = f;

                    item.texture_id = textureID;
                    faces.Add(item);
                }
            }
        }

        this.vertices = vertices.ToArray();
        this.faces = faces.ToArray();
        this.faceColors = faceColors.ToArray();
        this.normals = normals.ToArray();
        this.textureCoords = textureCoords.ToArray();

        return true;
    }
    public void LoadRawData(Float3[] vertices, FaceInfo[] faces, Float3[] normals, Float2[] textureCoords)
    {
        if (!IsEmpty())
        {
            Program.LogWarning("Cannot load data! Data is already loaded!");
            return;
        }

        this.vertices = vertices;
        this.faces = faces;
        this.normals = normals;
        this.textureCoords = textureCoords;
    }

    #region OpenGL Stuff
    public unsafe void BindOpenGL()
    {
        if (IsEmpty())
        {
            Program.LogWarning("Mesh Empty! Skipping OpenGl Binding");
            return;
        }

        if (!WindowManager.Initialized)
        {
            Program.LogError("Cannot bind open gl without a window!");
            return;
        }

        //Program.Log($"Binding OpenGL on mesh '{gameObject}'");

        // OpenGl Time

        var vertexData = new float[faces.Length * 27];

        for (int i = 0; i < faces.Length; i++)
        {
            var face = faces[i];

            var faceIndex = i * 27;

            for (int vertexI = 0; vertexI < 3; vertexI++)
            {
                Float3 position = vertices[face.vertex_indexs[vertexI]];
                Float3 normal = normals[face.normal_indexs[vertexI]];
                Float2 uv = new(0, 0);

                if (textureCoords.Length > 0)
                    uv = textureCoords[face.texture_indexs[vertexI]];

                vertexData[faceIndex + 0] = position.x;
                vertexData[faceIndex + 1] = position.y;
                vertexData[faceIndex + 2] = position.z;

                vertexData[faceIndex + 3] = normal.x;
                vertexData[faceIndex + 4] = normal.y;
                vertexData[faceIndex + 5] = normal.z;

                vertexData[faceIndex + 6] = uv.x;
                vertexData[faceIndex + 7] = uv.y;
                vertexData[faceIndex + 8] = face.texture_id;

                faceIndex += 9;
            }
        }

        // Vertex | Color | UV
        /*
        vertexData = [
            -0.5f, -0.5f, 0.0f,   1,1,1, -1f,  -1f,
            0.5f, -0.5f, 0.0f,    1,1,1,  0f, 0.5f,
            0.0f,  0.5f, 0.0f,    1,1,1,  0.5f, 1f,
        ];
        //*/

        /*
        vertexData = [
                -0.5f,  0.5f, 1.0f,     1.0f, 0.0f, 0.0f, // top left
                 0.5f,  0.5f, 1.0f,     1.0f, 0.0f, 0.0f, // top right
                -0.5f, -0.5f, 1.0f,     0.0f, 1.0f, 0.0f, // bottom left
                                                   
                 0.5f,  0.5f, 1.0f,     1.0f, 0.0f, 0.0f, // top right
                 0.5f, -0.5f, 1.0f,     1.0f, 1.0f, 0.0f, // bottom right
                -0.5f, -0.5f, 1.0f,     0.0f, 1.0f, 0.0f, // bottom left
            ];
        //*/

        /* Log Vertex Data
        for (int g = 0; g < vertexData.Length / 8; g++)
        {
            Program.Log("Pos: " + vertexData[g * 8 + 0] + ", " + vertexData[g * 8 + 1] + ", " + vertexData[g * 8 + 2]);
            Program.Log("Col: " + vertexData[g * 8 + 3] + ", " + vertexData[g * 8 + 4] + ", " + vertexData[g * 8 + 5]);
            Program.Log("UV: " + vertexData[g * 8 + 6] + ", " + vertexData[g * 8 + 7]);
        }
        //*/

        vertexBufferSize = (uint)vertexData.Length;

        vao = glGenVertexArray();
        vbo = glGenBuffer();

        glBindVertexArray(vao);
        glBindBuffer(GL_ARRAY_BUFFER, vbo);

        fixed (float* ptrVertices = &vertexData[0])
        {
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertexData.Length, ptrVertices, GL_STATIC_DRAW);
        }

        // Position (x,y,z)
        glVertexAttribPointer(0, 3, GL_FLOAT, false, 9 * sizeof(float), (void*)0);
        glEnableVertexAttribArray(0);

        // Normals (x,y,z)
        glVertexAttribPointer(1, 3, GL_FLOAT, false, 9 * sizeof(float), (void*)(3 * sizeof(float)));
        glEnableVertexAttribArray(1);

        // Vertex Texture Coords (u,v,i)
        glVertexAttribPointer(2, 3, GL_FLOAT, false, 9 * sizeof(float), (void*)(6 * sizeof(float)));
        glEnableVertexAttribArray(2);

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);

        //Program.Log($"[{this}] BindOpenGL Success");
    }

    /// <summary>
    /// Bind "Vertex Buffer Object"
    /// </summary>
    public void BindVAO()
    {
        if (vao == 0) BindOpenGL();

        glBindVertexArray(vao);
    }
    /// <summary>
    /// Unbind "Vertex Buffer Object"
    /// </summary>
    public void UnBindVAO()
    {
        glBindVertexArray(0);
    }
    #endregion

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
        faceColors = null;
        textureCoords = null;

        vao = 0;
        vbo = 0;
    }
    public bool IsEmpty()
    {
        if (vertices == null) return true;
        else if (vertices.Length == 0) return true;

        if (faces == null) return true;
        else if (faces.Length == 0) return true;

        // Face colors not nessesary 

        return false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Program.Log($"'{this}' Destroy!");

        DeleteMesh();
    }

    public int CompareTo(Mesh other)
    {
        if (!other) return 1;

        return material.renderOrder.CompareTo(other.material.renderOrder);
    }
}

public struct FaceInfo
{
    public List<int> vertex_indexs = new();
    public List<int> texture_indexs = new();
    public List<int> normal_indexs = new();
    public int texture_id;
    public Float3 faceColor;

    public FaceInfo(string objFaceElements)
    {
        string[] elements = objFaceElements.Split(' ')[1..];

        foreach (var vertexInfo in elements)
        {
            int[] indexes = vertexInfo.Split('/').Select(int.Parse).ToArray();

            if (indexes.Length < 3)
            {
                throw new FormatException("Could not parse OBJ, Face elements was incomplete!");
            }

            vertex_indexs.Add(indexes[0] - 1);
            texture_indexs.Add(indexes[1] - 1);
            normal_indexs.Add(indexes[2] - 1);
        }

        faceColor = Float3.random;
    }

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