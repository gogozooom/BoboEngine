using BoboEngine.Shaders;

namespace BoboEngine;
public class Mesh : ObjectBehavior
{
    public Float3[] vertices;
    public FaceInfo[] faces;
    public Float3[] normals;
    public Float3[] faceColors;
    public Float2[] textureCoords;

    public Shader shader;

    public Mesh()
    {

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
                vertices.Add(new(-axes[0], axes[1], axes[2])); // Flip x to make positive x go right
            }
            else if (line.StartsWith("vn ")) // Face Normals
            {
                float[] axes = line[3..].Split(' ').Select(float.Parse).ToArray(); // Have to start with a 3???
                normals.Add(new(axes[0], axes[1]));
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

        return true;
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