using static OpenGL.GL;

namespace BoboEngine;
public abstract class RenderObject : ObjectBehavior, IComparable<RenderObject>
{
    /// <summary>
    /// Vertex Array Object Reference
    /// </summary>
    protected uint vao;
    /// <summary>
    /// Vertex Buffer Object Reference
    /// </summary>
    protected uint vbo;

    protected uint vertexBufferSize;

    public Material material;

    public abstract void InitalizeOpenGL();

    public void glBind()
    {
        material.shader.glBind();

        material.texture?.glBindTexture();

        glBindVAO();
    }

    public virtual void glDraw()
    {
        switch (material.transformMode)
        {
            case RenderTranformMode.Global:
                material.shader.glSetMatrix4x4("projection", WindowManager.cameraMatrixThisFrame);
                break;
            case RenderTranformMode.LocalTransform:
                material.shader.glSetMatrix4x4("projection", WindowManager.localCameraMatrixThisFrame);
                break;
            default:
                Engine.LogWarning($"[{this}] Render transform mode of: '{material.transformMode}' has not been implemented!");
                break;
        }

        material.shader.glSetMatrix4x4("model", transform.Matrix);

        material.GlBindShaderProperties();

        switch (material.blendMode)
        {
            case BlendMode.Normal:
                glBlendFuncSeparate(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ONE_MINUS_SRC_ALPHA);
                glEnable(GL_BLEND);
                break;
            case BlendMode.Blend:
                glBlendFuncSeparate(GL_SRC_ALPHA, GL_ONE, GL_ONE, GL_ZERO);
                glEnable(GL_BLEND);
                break;
            case BlendMode.Disable:
                glDisable(GL_BLEND);
                break;
        }

        if (material.useDepth) glEnable(GL_DEPTH_TEST); else glDisable(GL_DEPTH_TEST);
        if (material.cullBackFaces) glEnable(GL_CULL_FACE); else glDisable(GL_CULL_FACE);

        glDrawArrays(GL_TRIANGLES, 0, (int)vertexBufferSize);
    }

    public void glUnBind()
    {
        glUnBindVAO();

        material.texture?.glUnbindTexture();

        material.shader.glUnbind();
    }


    /// <summary>
    /// Bind "Vertex Buffer Object"
    /// </summary>
    protected void glBindVAO()
    {
        if (vao == 0) InitalizeOpenGL();

        glBindVertexArray(vao);
    }
    /// <summary>
    /// Unbind "Vertex Buffer Object"
    /// </summary>
    protected void glUnBindVAO()
    {
        glBindVertexArray(0);
    }

    public virtual bool ShouldRender() => true;

    public virtual int CompareTo(RenderObject other)
    {
        if (!other) return 1;

        return material.renderOrder.CompareTo(other.material.renderOrder);
    }
}
