using BoboEngine;
using BoboEngine.Utils;

namespace Minecraft;

public abstract class BaseBlock
{
    public readonly string id;
    public readonly string name;

    public BlockModel model;

    #region Init
    protected BaseBlock()
    {
        id = "minecraft:null";
        name = "NULL";

        model = BlockModel.LoadFromJson(nullData);
    }
    protected BaseBlock(string id, string name, DynamicData data)
    {
        this.id = id;
        this.name = name;

        if(data == null)
        {
            BlockModel.LoadFromJson(nullData);
            return;
        }

        MinecraftJsonManager.FullyPopulateData(ref data, "models/");

        model = BlockModel.LoadFromJson(data);
    }

    public static DynamicData nullData => GetNullData();
    private static DynamicData _nullData;
    private static DynamicData GetNullData()
    {
        if(_nullData == null) GenerateNullData();

        return _nullData;
    }
    private static void GenerateNullData()
    {
        var data = "{{\"parent\": \"minecraft:block/cube_all\",\"textures\": {\"all\": \"minecraft:null\"}}";

        _nullData = FileParser.ParseJson("NULL", data);

        MinecraftJsonManager.FullyPopulateData(ref _nullData, "models/");
    }
    #endregion

    #region Chunk Mesh Generation
    /// <summary>
    /// This is to only be used by <see cref="Chunk"/>.cs
    /// </summary>
    public abstract void GenerateMesh(ref int elementIndex, Int3 worldBlockPosition, Int3 localBlockPosition, ref List<Float3> GenerateMesh_vertices, ref List<FaceInfo> GenerateMesh_faces, ref List<Float2> GenerateMesh_textureCoords);

    /// <summary>
    /// Only to be used by <see cref="GenerateMesh"/>
    /// </summary>
    protected void GM_GenerateBlockMesh(BaseBlock blockType, Int3 worldBlockPosition, Int3 localBlockPosition, ref List<Float3> vertices, ref List<FaceInfo> faces, ref List<Float2> textureCoords, int elementIndex, BlockModelElement element)
    {
        if (!element.HasFaceData())
        {
            Program.LogWarning($"'{blockType}' has elements without any faces!");
            return;
        }

        // Face Render Checks

        bool renderEastFace = false;
        bool renderWestFace = false;
        bool renderDownFace = false;
        bool renderUpFace = false;
        bool renderNorthFace = false;
        bool renderSouthFace = false;

        int eastTextureID = 0;
        int westTextureID = 0;
        int downTextureID = 0;
        int upTextureID = 0;
        int northTextureID = 0;
        int southTextureID = 0;

        // Adjacent blocks TODO: (Can be moved out of element!)
        var downBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, -1, 0));
        var upBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 1, 0));
        var northBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, -1));
        var southBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, 1));
        var westBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(-1, 0, 0));
        var eastBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(1, 0, 0));

        // Maybe put downBlock, upBlock, northBlock, etc... in a separate struct?

        GM_PopulateModelProperties(element.east, blockType, ref renderEastFace, ref eastTextureID,
            element,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(element.west, blockType, ref renderWestFace, ref westTextureID,
            element,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(element.down, blockType, ref renderDownFace, ref downTextureID,
            element,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(element.up, blockType, ref renderUpFace, ref upTextureID,
            element,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(element.north, blockType, ref renderNorthFace, ref northTextureID,
            element,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(element.south, blockType, ref renderSouthFace, ref southTextureID,
            element,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        // Create Vertices


        Float3[] blockVerts = [
            new Float3(0, 0, 0), // 1
            new Float3(0, 0, 1), // 2
            new Float3(0, 1, 1), // 3
            new Float3(0, 1, 0), // 4
            new Float3(1, 0, 0), // 5
            new Float3(1, 1, 0), // 6
            new Float3(1, 1, 1), // 7
            new Float3(1, 0, 1)  // 8
            ];

        BlockModelRotation rotation = element.rotation;

        foreach (var vert in blockVerts)
        {
            Float3 untransformedVert = new(vert.x == 0 ? element.from.x : element.to.x, vert.y == 0 ? element.from.y : element.to.y, vert.z == 0 ? element.from.z : element.to.z);

            Float3 transformedVert = untransformedVert;

            if (rotation) // Avoid unnecessary calculation
            {
                transformedVert -= rotation.origin;

                BaseVectors basisVectors;

                switch (rotation.axis)
                {
                    case Axis.Xaxis:
                        basisVectors = BaseVectors.FromXRotation(rotation.angle);
                        break;
                    case Axis.Zaxis:
                        basisVectors = BaseVectors.FromZRotation(rotation.angle);
                        break;
                    default:
                        basisVectors = BaseVectors.FromYRotation(-rotation.angle);
                        break;
                }

                Float3 scaler = new(1, 1, 1);

                if (rotation.rescale)
                {
                    float mult = Maths.Sec(45f * Maths.TriangleWave(rotation.angle / 45f));

                    switch (rotation.axis)
                    {
                        case Axis.Xaxis:
                            scaler = new(1, mult, mult);
                            break;
                        case Axis.Zaxis:
                            scaler = new(mult, mult, 1);
                            break;
                        default:
                            scaler = new(mult, 1, mult);
                            break;
                    }
                }

                transformedVert = basisVectors.TransformVector(transformedVert) * scaler;

                transformedVert += rotation.origin;
            }

            vertices.Add(transformedVert + localBlockPosition);
        }

        List<(string, int)> squareFaces = new();

        if (renderWestFace)
        {
            GM_GenerateModelFace(element.west.rotation, element.west.uv, westTextureID, [1, 2, 3, 4], 1,
                ref textureCoords, ref squareFaces, ref elementIndex);
        }

        if (renderEastFace)
        {
            GM_GenerateModelFace(element.east.rotation, element.east.uv, eastTextureID, [8, 5, 6, 7], 2,
                ref textureCoords, ref squareFaces, ref elementIndex);
        }

        if (renderDownFace)
        {
            GM_GenerateModelFace(element.down.rotation, element.down.uv, downTextureID, [1, 5, 8, 2], 3,
                ref textureCoords, ref squareFaces, ref elementIndex);
        }

        if (renderUpFace)
        {
            GM_GenerateModelFace(element.up.rotation, element.up.uv, upTextureID, [3, 7, 6, 4], 4,
                ref textureCoords, ref squareFaces, ref elementIndex);
        }

        if (renderNorthFace)
        {
            GM_GenerateModelFace(element.north.rotation, element.north.uv, northTextureID, [5, 1, 4, 6], 5,
                ref textureCoords, ref squareFaces, ref elementIndex);
        }

        if (renderSouthFace)
        {
            GM_GenerateModelFace(element.south.rotation, element.south.uv, southTextureID, [2, 8, 7, 3], 6,
                ref textureCoords, ref squareFaces, ref elementIndex);
        }

        foreach (var face in squareFaces)
        {
            foreach (var f in FaceInfo.GetTriangulatedFaces(face.Item1))
            {
                var faceItem = f;

                faceItem.texture_id = face.Item2;
                faces.Add(faceItem);
            }
        }
    }

    /// <summary>
    /// Only to be used by <see cref="GenerateMesh"/>
    /// </summary>
    protected void GM_PopulateModelProperties(BlockModelFace face, BaseBlock blockType, ref bool renderFace, ref int textureID,
        BlockModelElement element,
        WorldBlockData downBlock, WorldBlockData upBlock, WorldBlockData northBlock, WorldBlockData southBlock, WorldBlockData westBlock, WorldBlockData eastBlock)
    {
        if (face)
        {
            if (face.cullface != null)
            {
                string adjacentBlockId = "minecraft:air";

                switch (face.cullface)
                {
                    case "down":
                        adjacentBlockId = downBlock.block_id;
                        break;
                    case "up":
                        adjacentBlockId = upBlock.block_id;
                        break;
                    case "north":
                        adjacentBlockId = northBlock.block_id;
                        break;
                    case "south":
                        adjacentBlockId = southBlock.block_id;
                        break;
                    case "west":
                        adjacentBlockId = westBlock.block_id;
                        break;
                    case "east":
                        adjacentBlockId = eastBlock.block_id;
                        break;
                }

                renderFace = blockType.WillRenderSide(BlockTypeManager.GetBlockType(adjacentBlockId));
            }
            else renderFace = true;

            if (renderFace)
            {
                textureID = TextureManager.GetTextureID(face.texture);
            }
        }
    }

    /// <summary>
    /// Only to be used by <see cref="GenerateMesh"/>
    /// </summary>
    protected void GM_GenerateModelFace(float uvRotation, UVRect uvs, int textureID, int[] vertIndices, int normalIndex,
        ref List<Float2> textureCoords, ref List<(string, int)> squareFaces, ref int elementIndex)
    {
        switch (uvRotation)
        {
            default:
                textureCoords.Add(new(uvs.uMin, uvs.vMin));
                textureCoords.Add(new(uvs.uMax, uvs.vMin));
                textureCoords.Add(new(uvs.uMax, uvs.vMax));
                textureCoords.Add(new(uvs.uMin, uvs.vMax));
                break;
            case 90f:
                textureCoords.Add(new(uvs.uMax, uvs.vMin));
                textureCoords.Add(new(uvs.uMax, uvs.vMax));
                textureCoords.Add(new(uvs.uMin, uvs.vMax));
                textureCoords.Add(new(uvs.uMin, uvs.vMin));
                break;
            case 180f:
                textureCoords.Add(new(uvs.uMax, uvs.vMax));
                textureCoords.Add(new(uvs.uMin, uvs.vMax));
                textureCoords.Add(new(uvs.uMin, uvs.vMin));
                textureCoords.Add(new(uvs.uMax, uvs.vMin));
                break;
            case 270f:
                textureCoords.Add(new(uvs.uMin, uvs.vMax));
                textureCoords.Add(new(uvs.uMin, uvs.vMin));
                textureCoords.Add(new(uvs.uMax, uvs.vMin));
                textureCoords.Add(new(uvs.uMax, uvs.vMax));
                break;
        }

        var uvO = textureCoords.Count();

        var vO = elementIndex * 8;

        squareFaces.Add(($"f {vertIndices[0] + vO}/{uvO - 3}/{normalIndex} " +
                          $"{vertIndices[1] + vO}/{uvO - 2}/{normalIndex} " +
                          $"{vertIndices[2] + vO}/{uvO - 1}/{normalIndex} " +
                          $"{vertIndices[3] + vO}/{uvO}/{normalIndex}",
                          textureID
                          ));
    }

    public virtual bool WillRenderSide(BaseBlock adjasentBlock)
    {
        if (adjasentBlock.id == "minecraft:air") return true;

        return false;
    }

    #endregion

    public override string ToString()
    {
        return id;
    }
}