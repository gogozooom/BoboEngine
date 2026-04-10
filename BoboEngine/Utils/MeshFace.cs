namespace BoboEngine;

public struct MeshFace
{
    public List<MeshVertex> vertices = new();

    /// <summary>
    /// <br/>
    /// "f vert/tex/norm vert/tex/norm ..." <br/>
    /// E.g. "f 1/1/1 2/2/2 3/3/3 4/4/4 5/5/5"
    /// </summary>
    public MeshFace(string objFaceElements)
    {
        string[] elements = objFaceElements.Split(' ')[1..];

        foreach (var vertexInfo in elements)
        {
            int[] indexes = vertexInfo.Split('/').Select(int.Parse).ToArray();

            int vertex_coord_index = -1;
            int texture_coord_index = -1;
            int normal_coord_index = -1;

            if (indexes.Length >= 1)
            {
                vertex_coord_index = indexes[0] - 1; // - 1 to convert to 0 indexed list format
            }

            if (indexes.Length >= 2)
            {
                texture_coord_index = indexes[1] - 1; // - 1 to convert to 0 indexed list format
            }

            if (indexes.Length >= 3)
            {
                normal_coord_index = indexes[2] - 1; // - 1 to convert to 0 indexed list format
            }

            vertices.Add(new MeshVertex(vertex_coord_index, texture_coord_index, normal_coord_index));
        }
    } 

    public MeshFace(List<MeshVertex> vertices)
    {
        this.vertices = vertices;
    }

    public readonly MeshFace[] GetFaceTriangulated() // Supply multiple techniques maybe?
    {
        List<MeshFace> faces = new();

        if (vertices.Count <= 3)
        {
            faces = [this];
        }
        else
        {
            for (int i = 2; i < vertices.Count; i++)
            {
                faces.Add(new MeshFace([vertices[0], vertices[i - 1], vertices[i]]));
            }
        }

        return faces.ToArray();
    }
}