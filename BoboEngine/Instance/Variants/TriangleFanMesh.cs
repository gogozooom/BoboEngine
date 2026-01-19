using static OpenGL.GL;

namespace BoboEngine;

public class TriangleFanMesh : Mesh
{
    Float4[] colors;

    public void LoadRawData(Float3[] vertices, Float4[] colors)
    {
        this.vertices = vertices;
        this.colors = colors;
    }

    protected override float[] glConvertToData()
    {
        int dataPointCount = 7;
        var vertexData = new float[vertices.Length * dataPointCount];

        var index = 0;
        foreach (var vertex in vertices)
        {
            Float3 position = vertices[index];
            Float4 color = colors == null ? new Float4(1,1,1,1) : colors[index];

            vertexData[index * dataPointCount + 0] = position.x;
            vertexData[index * dataPointCount + 1] = position.y;
            vertexData[index * dataPointCount + 2] = position.z;

            vertexData[index * dataPointCount + 3] = color.r;
            vertexData[index * dataPointCount + 4] = color.g;
            vertexData[index * dataPointCount + 5] = color.b;
            vertexData[index * dataPointCount + 6] = color.a;

            index += 1;
        }

        vertexBufferSize = (uint)vertices.Length;

        return vertexData;
    }

    protected override unsafe void glBindPointers()
    {
        // Position (x,y,z)
        glVertexAttribPointer(0, 3, GL_FLOAT, false, 7 * sizeof(float), (void*)0);
        glEnableVertexAttribArray(0);

        // Color (r,g,b,a)
        glVertexAttribPointer(1, 4, GL_FLOAT, false, 7 * sizeof(float), (void*)(3 * sizeof(float)));
        glEnableVertexAttribArray(1);
    }

    public override void glDraw()
    {
        glDrawArrays(GL_TRIANGLE_FAN, 0, (int)vertexBufferSize);
    }

    public override bool IsEmpty()
    {
        if (vertices == null) return true;
        else if (vertices.Length == 0) return true;

        return false;
    }
}
