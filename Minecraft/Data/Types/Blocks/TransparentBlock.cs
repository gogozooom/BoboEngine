using BoboEngine;
using BoboEngine.Utils;

namespace Minecraft.Blocks;

public class TransparentBlock : BaseBlock
{
    public TransparentBlock() : base() { }
    public TransparentBlock(string id, string name, DynamicData data) : base( id, name, data) { }

    public override void GenerateMesh(ref int elementIndex, Int3 worldBlockPosition, Int3 localBlockPosition, ref List<Float3> GenerateMesh_vertices, ref List<FaceInfo> GenerateMesh_faces, ref List<Float2> GenerateMesh_textureCoords)
    {
        foreach (var element in model.elements)
        {
            GM_GenerateBlockMesh(this, worldBlockPosition, localBlockPosition, ref GenerateMesh_vertices, ref GenerateMesh_faces, ref GenerateMesh_textureCoords, elementIndex, element);

            elementIndex++;
        }
    }

    public override bool WillRenderSide(BaseBlock adjasentBlock)
    {
        if (base.WillRenderSide(adjasentBlock)) return true; // Universal Rules

        if (adjasentBlock.GetType() == typeof(CubeBlock)) return false;
        if (adjasentBlock.id == id) return false;

        return true;
    }
}
