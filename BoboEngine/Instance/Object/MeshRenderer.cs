using static OpenGL.GL;

namespace BoboEngine;
public class MeshRenderer : ObjectBehavior, IComparable<MeshRenderer>
{
    protected MeshFilter meshFilter;

    public Material material;

    public MeshRenderer()
    {
        material = new();
    }

    public override void Start()
    {
        base.Start();
        meshFilter = gameObject.RequireComponent<MeshFilter>();
    }

    public void glBind()
    {
        material.shader.glBind();

        material.texture?.glBindTexture();

        meshFilter.mesh.glBindVAO();
    }

    protected virtual void glSetMatrixes()
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
    }

    public void glDraw()
    {
        glSetMatrixes();

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
            case BlendMode.Invert:
                glBlendFuncSeparate(GL_ONE_MINUS_DST_COLOR, GL_ONE_MINUS_SRC_COLOR, GL_ONE, GL_ZERO);
                glEnable(GL_BLEND);
                break;
            case BlendMode.Disable:
                glDisable(GL_BLEND);
                break;
        }

        if (material.useDepth) glEnable(GL_DEPTH_TEST); else glDisable(GL_DEPTH_TEST);
        if (material.cullBackFaces) glEnable(GL_CULL_FACE); else glDisable(GL_CULL_FACE);

        meshFilter.mesh.glDraw();
    }

    public void glUnBind()
    {
        meshFilter.mesh.glUnBindVAO();

        material.texture?.glUnbindTexture();

        material.shader.glUnbind();
    }

    public virtual bool ShouldRender()
    {
        if (!meshFilter.mesh) return false;
        if (meshFilter.mesh.IsEmpty()) return false;
        if (material == null) return false;
        if (material.shader == null) return false;
        if (!material.shader.isLoaded) return false;

        return true;
    }

    public virtual int CompareTo(MeshRenderer other)
    {
        if (!other) return 1;

        return material.renderOrder.CompareTo(other.material.renderOrder);
    }
}
