using static OpenGL.GL;

namespace BoboEngine;

public class TriangleFanMesh : Mesh
{
    VertexAttribute<Float4> Colors => vertexData.GetAttribute<Float4>("colors");

    public override void glDraw()
    {
        glDrawArrays(GL_TRIANGLE_FAN, 0, (int)vertexBufferSize);
    }
}
