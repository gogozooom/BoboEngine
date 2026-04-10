namespace BoboEngine;

public struct MeshVertex
{
    public int vertex_coord_index;
    public int texture_coord_index;
    public int normal_coord_index;

    public MeshVertex(int vertex_coord_index, int texture_coord_index, int normal_coord_index)
    {
        this.vertex_coord_index = vertex_coord_index;
        this.texture_coord_index = texture_coord_index;
        this.normal_coord_index = normal_coord_index;
    }
}