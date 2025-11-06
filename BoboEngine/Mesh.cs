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
    private uint vertexBufferSize;

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

        Delete();
    }

    public unsafe bool LoadObjFile(string filePath)
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
                vertices.Add(new(-axes[0], axes[1], axes[2])); // Flip x to make positive x go right
            }
            else if (line.StartsWith("vn ")) // Face Normals
            {
                float[] axes = line[3..].Split(' ').Select(float.Parse).ToArray(); // Have to start with a 3???
                normals.Add(new(-axes[0], axes[1], axes[2]));
            }
            else if(line.StartsWith("vt ")) // Vertex Texture Chords
            {
                float[] axes = line[3..].Split(' ').Select(float.Parse).ToArray(); // This one too???
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

        // OpenGl Time

        var vertexData = new float[faces.Count * 18];

        for (int i = 0; i < this.faces.Length; i++)
        {
            var face = this.faces[i];

            var faceIndex = i * 18;

            Float3 a = vertices[face.vertex_indexs[0]];

            vertexData[faceIndex + 0] = a.x;
            vertexData[faceIndex + 1] = a.y;
            vertexData[faceIndex + 2] = a.z;

            vertexData[faceIndex + 3] = face.faceColor.r;
            vertexData[faceIndex + 4] = face.faceColor.g;
            vertexData[faceIndex + 5] = face.faceColor.b;

            Float3 b = vertices[face.vertex_indexs[1]];

            faceIndex += 6;

            vertexData[faceIndex + 0] = b.x;
            vertexData[faceIndex + 1] = b.y;
            vertexData[faceIndex + 2] = b.z;

            vertexData[faceIndex + 3] = face.faceColor.r;
            vertexData[faceIndex + 4] = face.faceColor.g;
            vertexData[faceIndex + 5] = face.faceColor.b;

            Float3 c = vertices[face.vertex_indexs[2]];

            faceIndex += 6;

            vertexData[faceIndex + 0] = c.x;
            vertexData[faceIndex + 1] = c.y;
            vertexData[faceIndex + 2] = c.z;

            vertexData[faceIndex + 3] = face.faceColor.r;
            vertexData[faceIndex + 4] = face.faceColor.g;
            vertexData[faceIndex + 5] = face.faceColor.b;
        }

        for (int g = 0; g < vertexData.Length / 6; g++)
        {
            Program.Log("Pos: " + vertexData[g * 6 + 0] + ", " + vertexData[g * 6 + 1] + ", " + vertexData[g * 6 + 2]);
            Program.Log("Col: " + vertexData[g * 6 + 3] + ", " + vertexData[g * 6 + 4] + ", " + vertexData[g * 6 + 5]);
        }

        shader = new Shader("Shader/modelShader.vert", "Shader/modelShader.frag");

        // Vertex | Color
        /*
        var _vertexData = new[] {
            -0.5f, -0.5f, 0.0f,     1,1,1,
            0.5f, -0.5f, 0.0f,      1,1,1,
            0.0f,  0.5f, 0.0f,      1,1,1,
        };
        */

        /*
        float[] _vertexData = {
                -0.5f,  0.5f, 1.0f,     1.0f, 0.0f, 0.0f, // top left
                 0.5f,  0.5f, 1.0f,     1.0f, 0.0f, 0.0f, // top right
                -0.5f, -0.5f, 1.0f,     0.0f, 1.0f, 0.0f, // bottom left
                                                   
                 0.5f,  0.5f, 1.0f,     1.0f, 0.0f, 0.0f, // top right
                 0.5f, -0.5f, 1.0f,     1.0f, 1.0f, 0.0f, // bottom right
                -0.5f, -0.5f, 1.0f,     0.0f, 1.0f, 0.0f, // bottom left
            };
        */


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
        glVertexAttribPointer(0, 3, GL_FLOAT, false, 6 * sizeof(float), (void*)0);
        glEnableVertexAttribArray(0);

        // Color (r,g,b)
        glVertexAttribPointer(1, 3, GL_FLOAT, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));
        glEnableVertexAttribArray(1);

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);

        return true;
    }

    public void BindVAO()
    {
        glBindVertexArray(vao);
    }
    public void UnBindVAO()
    {
        glBindVertexArray(0);
    }
    public uint GetVertexBufferSize()
    {
        return vertexBufferSize;
    }
    public void Delete()
    {
        glDeleteBuffer(vbo);
        glDeleteVertexArray(vao);

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