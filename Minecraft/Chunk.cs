using BoboEngine;
using BoboEngine.Utils;

namespace Minecraft;
public class Chunk : ObjectBehavior
{
    public Int3 chunkPosition { get; private set; }

    private WorldBlockData[,,] blocks = new WorldBlockData[16, 16, 16];

    private bool meshChanged = false;

    public void InitChunk(Int3 position)
    {
        chunkPosition = position;
        transform.position = chunkPosition * 16;

        ClearMesh();
    }

    public override void Start()
    {
        _connectedMesh = gameObject.GetComponent<Mesh>();

        if (!_connectedMesh)
        {
            _connectedMesh = gameObject.AddComponent<Mesh>();
            _connectedMesh.material = WorldDataManager.meshMaterial;
        }
    }
    public override void Update()
    {
        if (meshChanged)
        {
            GenerateMesh();
            meshChanged = false;
        }
    }

    public void ClearMesh()
    {
        for (int x = 0; x < blocks.GetLength(0); x++)
        {
            for (int y = 0; y < blocks.GetLength(1); y++)
            {
                for (int z = 0; z < blocks.GetLength(2); z++)
                {
                    blocks[x, y, z] = new();
                }
            }
        }

        meshChanged = true;
    }

    private Mesh _connectedMesh;

    /// <summary>
    /// Updates the visual mesh
    /// </summary>
    public void GenerateMesh()
    {
        _connectedMesh.DeleteMesh();

        List<Float3> vertices = new();
        List<FaceInfo> faces = new();
        List<Float2> textureCoords = new();
        List<Float3> normals = [
            BlockFace.GetNormal(BlockDirection.WEST),
            BlockFace.GetNormal(BlockDirection.EAST),
            BlockFace.GetNormal(BlockDirection.DOWN),
            BlockFace.GetNormal(BlockDirection.UP),
            BlockFace.GetNormal(BlockDirection.NORTH),
            BlockFace.GetNormal(BlockDirection.SOUTH),
            ];


        int elementIndex = 0;

        for (int x = 0; x < blocks.GetLength(0); x++)
        {
            for (int y = 0; y < blocks.GetLength(1); y++)
            {
                for (int z = 0; z < blocks.GetLength(2); z++)
                {
                    WorldBlockData block = blocks[x, y, z];

                    if (block.block_id == "minecraft:air" || block.block_id == "minecraft:void") continue;

                    BlockType blockType = BlockTypeManager.GetBlockType(block.block_id);
                    Int3 localBlockPosition = new(x, y, z);

                    BlockModelElement[] elements = blockType.model.elements;

                    if (elements.Length == 0)
                    {
                        Program.LogWarning($"'{blockType}' does not have any model elements!");
                        continue;
                    }

                    // Flipped order to allow block overlays to render over
                    for (int i = elements.Length - 1; i >= 0; i--)
                    {
                        GM_GenerateBlockMesh(blockType, localBlockPosition, ref vertices, ref faces, ref textureCoords, elementIndex, elements[i]);

                        elementIndex++;
                    }
                }
            }
        }

        _connectedMesh.LoadRawData(vertices.ToArray(), faces.ToArray(), normals.ToArray(), textureCoords.ToArray());
    }

    /// <summary>
    /// Only to be used by <see cref="GenerateMesh"/>
    /// </summary>
    private void GM_GenerateBlockMesh(BlockType blockType, Int3 localBlockPosition, ref List<Float3> vertices, ref List<FaceInfo> faces, ref List<Float2> textureCoords, int elementIndex, BlockModelElement element)
    {
        if (!element.HasFaceData())
        {
            Program.LogWarning($"'{blockType}' has elements without any faces!");
            return;
        }

        Int3 worldBlockPosition = LocalPositionToWorld(localBlockPosition);

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

        // Adjacent blocks
        var downBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, -1, 0));
        var upBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 1, 0));
        var northBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, -1));
        var southBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, 1));
        var westBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(-1, 0, 0));
        var eastBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(1, 0, 0));

        // Maybe put downBlock, upBlock, northBlock, etc... in a separate struct?

        GM_PopulateModelProperties(element.east, ref renderEastFace, ref eastTextureID,
            element,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(element.west, ref renderWestFace, ref westTextureID,
            element,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(element.down, ref renderDownFace, ref downTextureID,
            element,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(element.up, ref renderUpFace, ref upTextureID,
            element,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(element.north, ref renderNorthFace, ref northTextureID,
            element,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(element.south, ref renderSouthFace, ref southTextureID,
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
                        basisVectors = BaseVectors.FromYRotation(rotation.angle);
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
    private void GM_PopulateModelProperties(BlockModelFace face, ref bool renderFace, ref int textureID,
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

                renderFace = !BlockTypeManager.GetBlockType(adjacentBlockId).CanCullSide(new BlockFace(face.cullface));
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
    private void GM_GenerateModelFace(float uvRotation, UVRect uvs, int textureID, int[] vertIndices, int normalIndex, 
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

    public WorldBlockData GetBlockAtLocalPosition(Int3 localPosition)
    {
        if (!IsLocalPositionValid(localPosition))
        {
            Program.LogWarning("GetBlockAtPosition() Position out of range!");
            return new WorldBlockData();
        }

        return blocks[localPosition.x, localPosition.y, localPosition.z];
    }

    public void SetBlockAtLocalPosition(Int3 localPosition, WorldBlockData block)
    {
        if (!IsLocalPositionValid(localPosition))
        {
            Program.LogWarning("SetBlockAtLocalPosition() Cannot set block! Out of bounds!");
            return;
        }

        blocks[localPosition.x, localPosition.y, localPosition.z] = block;
        meshChanged = true;

        Int3[] adjacentBlocks = [
            new Int3(-1, 0, 0),
            new Int3(1, 0, 0),
            new Int3(0, -1, 0),
            new Int3(0, 1, 0),
            new Int3(0, 0, -1),
            new Int3(0, 0, 1)
            ];

        foreach (var adjacentBlockPos in adjacentBlocks)
        {
            Chunk chunk = WorldDataManager.GetChunkInPosition(LocalPositionToWorld(localPosition) + adjacentBlockPos);

            if (chunk == null) continue;
            if (chunk == this) continue;

            chunk.meshChanged = true;
        }
    }

    public Int3 WorldPositionToLocal(Int3 worldPosition) => worldPosition - (chunkPosition * 16);
    public Int3 LocalPositionToWorld(Int3 localPosition) => localPosition + (chunkPosition * 16);

    public bool IsLocalPositionValid(Int3 localPosition)
    {
        if (localPosition.x < 0 || localPosition.x >= 16)
        {
            return false;
        }
        if (localPosition.y < 0 || localPosition.y >= 16)
        {
            return false;
        }
        if (localPosition.z < 0 || localPosition.z >= 16)
        {
            return false;
        }

        return true;
    }
}

public struct BlockRaycastHit
{
    public WorldBlockData blockHit;
    public BlockFace blockFace;
    public Float3 hitPosition;
    public Int3 blockPosition;

    public BlockRaycastHit()
    {
        blockHit = new();
        blockPosition = new();
        blockFace = new();
        hitPosition = new();
    }
    public BlockRaycastHit(WorldBlockData blockHit, Int3 blockPosition, BlockFace blockFace, Float3 hitPosition)
    {
        this.blockHit = blockHit;
        this.blockPosition = blockPosition;
        this.blockFace = blockFace;
        this.hitPosition = hitPosition;
    }

    public static implicit operator bool(BlockRaycastHit hit) => hit.blockHit.block_id != "minecraft:air";

    public override string ToString()
    {
        return $"[{blockHit}] '{blockFace}' '{hitPosition}'";
    }
}