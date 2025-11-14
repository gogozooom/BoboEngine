using BoboEngine.Shaders;
using static OpenGL.GL;

namespace BoboEngine;
public class Mesh : ObjectBehavior
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

    public Shader shader;

    public Mesh()
    {

    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Program.Log($"'{this}' Destroy!");

        DeleteMesh();
    }

    public bool LoadObjFile(string filePath)
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
                foreach (var item in FaceInfo.GetTriangulatedFaces(line))
                {
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

    public void LoadCube(BlockType blockData)
    {
        LoadCubeBaseMesh();

        Float2[] newTextureCoords = new Float2[textureCoords.Length];

        int i = 0;
        foreach (var cords in textureCoords)
        {
            if(i < 4) // Right Face
            {
                newTextureCoords[i++] = TextureAtlasManager.GetUVPositionOnTexture(blockData.rightTexture, cords);
            }
            else if (i < 8) // Left Face
            {
                newTextureCoords[i++] = TextureAtlasManager.GetUVPositionOnTexture(blockData.leftTexture, cords);
            }
            else if (i < 12) // Bottom Face
            {
                newTextureCoords[i++] = TextureAtlasManager.GetUVPositionOnTexture(blockData.bottomTexture, cords);
            }
            else if (i < 16) // Top Face
            {
                newTextureCoords[i++] = TextureAtlasManager.GetUVPositionOnTexture(blockData.topTexture, cords);
            }
            else if (i < 20) // Back Face
            {
                newTextureCoords[i++] = TextureAtlasManager.GetUVPositionOnTexture(blockData.backTexture, cords);
            }
            else // Front Face
            {
                newTextureCoords[i++] = TextureAtlasManager.GetUVPositionOnTexture(blockData.frontTexture, cords);
            }
        }

        textureCoords = newTextureCoords;
    }

    private void LoadCubeBaseMesh()
    {
        vertices =
        [
            new(0, 0, 0),
            new(0, 0, 1),
            new(0, 1, 1),
            new(0, 1, 0),
            new(1, 0, 0),
            new(1, 1, 0),
            new(1, 1, 1),
            new(1, 0, 1),
        ];

        normals =
        [
            // Right Face
            new(-1, 0, 0),
            // Left Face
            new(1, 0, 0),
            // Bottom Face
            new(0, -1, 0),
            // Top Face
            new(0, 1, 0),
            // Back Face
            new(0, 0, -1),
            // Front Face
            new(0, 0, 1),
        ];
        faceColors =
        [
            Float3.one,
            Float3.one,
            Float3.one,
            Float3.one,
            Float3.one,
            Float3.one,
        ];

        textureCoords =
        [
            // Right Face
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),

            // Left Face
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),

            // Bottom Face
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),

            // Top Face
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),
            
            // Back Face
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),

            // Front Face
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),

        ];

        List<string> squareFaces =
        [
            "f 1/1/1 2/2/1 3/3/1 4/4/1", // Right Face
            "f 8/5/2 5/6/2 6/7/2 7/8/2", // Left Face
            "f 1/9/3 5/10/3 8/11/3 2/12/3", // Bottom Face
            "f 3/13/4 7/14/4 6/15/4 4/16/4", // Top Face
            "f 5/21/6 1/22/6 4/23/6 6/24/6", // Back Face
            "f 2/17/5 8/18/5 7/19/5 3/20/5", // Front Face
        ];

        var faces = new List<FaceInfo>();

        foreach (var face in squareFaces)
        {
            foreach (var item in FaceInfo.GetTriangulatedFaces(face))
            {
                faces.Add(item);
            }
        }

        this.faces = faces.ToArray();
    }
    private void LoadPlaneBaseMesh()
    {
        vertices =
        [
            new(-0.5f, 0, -0.5f),
            new(-0.5f, 0, 0.5f),
            new(0.5f, 0, 0.5f),
            new(0.5f, 0, -0.5f),
        ];

        normals =
        [
            new(0, 1, 0),
        ];

        faceColors =
        [
            Float3.one,
            Float3.one,
        ];

        textureCoords =
        [
            new(0, 1),
            new(0, 0),
            new(1, 0),
            new(1, 1),
        ];

        var faces = new List<FaceInfo>();
        foreach (var item in FaceInfo.GetTriangulatedFaces("f 1/1/1 2/2/1 3/3/1 4/4/1"))
        {
            faces.Add(item);
        }

        this.faces = faces.ToArray();
    }

    #region OpenGL Stuff
    public unsafe void BindOpenGL()
    {
        if (IsEmpty())
        {
            Program.LogWarning("Mesh Empty! Skipping OpenGl Binding");
            return;
        }

        Program.Log($"Binding OpenGL on mesh '{gameObject}'");

        if (!WindowManager.Initialized)
        {
            Program.LogError("Cannot bind open gl without a window!");
            return;
        }

        // OpenGl Time

        var vertexData = new float[faces.Length * 24];

        for (int i = 0; i < faces.Length; i++)
        {
            var face = faces[i];

            var faceIndex = i * 24;

            Float3 a = vertices[face.vertex_indexs[0]];
            Float2 aT = textureCoords[face.texture_indexs[0]];

            vertexData[faceIndex + 0] = a.x;
            vertexData[faceIndex + 1] = a.y;
            vertexData[faceIndex + 2] = a.z;

            vertexData[faceIndex + 3] = face.faceColor.r;
            vertexData[faceIndex + 4] = face.faceColor.g;
            vertexData[faceIndex + 5] = face.faceColor.b;

            vertexData[faceIndex + 6] = aT.x;
            vertexData[faceIndex + 7] = aT.y;

            Float3 b = vertices[face.vertex_indexs[1]];
            Float2 bT = textureCoords[face.texture_indexs[1]];

            faceIndex += 8;

            vertexData[faceIndex + 0] = b.x;
            vertexData[faceIndex + 1] = b.y;
            vertexData[faceIndex + 2] = b.z;

            vertexData[faceIndex + 3] = face.faceColor.r;
            vertexData[faceIndex + 4] = face.faceColor.g;
            vertexData[faceIndex + 5] = face.faceColor.b;

            vertexData[faceIndex + 6] = bT.x;
            vertexData[faceIndex + 7] = bT.y;

            Float3 c = vertices[face.vertex_indexs[2]];
            Float2 cT = textureCoords[face.texture_indexs[2]];

            faceIndex += 8;

            vertexData[faceIndex + 0] = c.x;
            vertexData[faceIndex + 1] = c.y;
            vertexData[faceIndex + 2] = c.z;

            vertexData[faceIndex + 3] = face.faceColor.r;
            vertexData[faceIndex + 4] = face.faceColor.g;
            vertexData[faceIndex + 5] = face.faceColor.b;

            vertexData[faceIndex + 6] = cT.x;
            vertexData[faceIndex + 7] = cT.y;
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
        glVertexAttribPointer(0, 3, GL_FLOAT, false, 8 * sizeof(float), (void*)0);
        glEnableVertexAttribArray(0);

        // Color (r,g,b)
        glVertexAttribPointer(1, 3, GL_FLOAT, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
        glEnableVertexAttribArray(1);

        // Vertex Texture Coords (u,v)
        glVertexAttribPointer(2, 2, GL_FLOAT, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
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
}

public struct FaceInfo
{
    public List<int> vertex_indexs = new();
    public List<int> texture_indexs = new();
    public List<int> normal_indexs = new();
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