namespace BoboEngine.UI;

public class UIRenderer : MeshRenderer
{
    public UITransform uiTransform { get; private set; }

    public override void Start()
    {
        base.Start();
        uiTransform = gameObject.RequireComponent<UITransform>();

        meshFilter.mesh ??= GetDefaultMesh();
        material ??= new Material("defaultUI", renderOrder: 1000);
    }

    private static Mesh defaultMesh;
    private static Mesh GetDefaultMesh()
    {
        if (!defaultMesh)
        {
            defaultMesh = new Mesh();
            defaultMesh.LoadObjFile(Engine.GetLocalModelPath("Square"));
        }

        return defaultMesh;
    }

    protected override void glSetMatrixes()
    {
        /* Not Used Yet...
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
        */
        material.shader.glSetMatrix4x4("model", transform.Matrix * uiTransform.Matrix);
        material.shader.glSetVec4("u_color", material.color);
        material.shader.glSetFloat("u_time", Time.time);
    }
}
