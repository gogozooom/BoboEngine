namespace BoboEngine.UI;

public class UIRenderer : MeshRenderer
{
    public UVTransform uvTransform { get; private set; }



    public override void Start()
    {
        base.Start();
        uvTransform = gameObject.RequireComponent<UVTransform>();
        meshFilter = gameObject.GetComponent<MeshFilter>();

        meshFilter.mesh ??= GetDefaultMesh();
    }

    private Mesh defaultMesh;
    private Mesh GetDefaultMesh()
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
        material.shader.glSetMatrix4x4("model", transform.Matrix * uvTransform.Matrix);
    }
}
