using BoboEngine;
using BoboEngine.Utils;

namespace Minecraft;
public class Chunk : ObjectBehavior
{
    public Int3 chunkPosition { get; private set; }

    private BlockData[,,] blocks = new BlockData[16, 16, 16];

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
                    BlockData block = blocks[x, y, z];

                    if (block.block_id == "minecraft:air" || block.block_id == "minecraft:void") continue;

                    BlockType blockType = BlockTypeManager.GetBlockData(block.block_id);
                    Int3 localBlockPosition = new(x, y, z);

                    var elementData = blockType.modelData.GetItem("elements");

                    if (!elementData)
                    {
                        Program.LogWarning($"'{blockType}' does not have any model elements!");
                        continue;
                    }

                    DynamicData[] elements = elementData.GetImmediateChildren();

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
    private void GM_GenerateBlockMesh(BlockType blockType, Int3 localBlockPosition, ref List<Float3> vertices, ref List<FaceInfo> faces, ref List<Float2> textureCoords, int elementIndex, DynamicData elementData)
    {
        /* NOTE:
         * Any variable name immidiatly followed by a 'D' means Data
         *  e.g:
         *     var facesD == DynamicData facesData
         *     var fromD == DynamicData fromData
         *     var rescaleD == DynamicData rescaleData
         *     
         *  I have this simply to avoid clutter
         */

        var facesD = elementData.GetItem("faces");

        if (!facesD)
        {
            Program.LogWarning($"'{blockType}' has elements without any faces!");
            return;
        }

        Int3 worldBlockPosition = LocalPositionToWorld(localBlockPosition);

        // Create Vertice Bounds

        Float3 from = new(0, 0, 0);
        Float3 to = new(16, 16, 16);

        Float3 rotationOrigin = new(0, 0, 0);
        Axis rotationAxis = Axis.Yaxis;
        float rotationAngle = 0;
        bool rotationRescale = false;

        var fromD = elementData.GetItem("from");
        var toD = elementData.GetItem("to");
        var rotationD = elementData.GetItem("rotation");

        DynamicData originD = null;
        DynamicData axisD = null;
        DynamicData angleD = null;
        DynamicData rescaleD = null;


        if (fromD) from = fromD.GetValue<Float3>();
        if (toD) to = toD.GetValue<Float3>();
        if (rotationD)
        {
            originD = rotationD.GetItem("origin");

            if (originD) rotationOrigin = originD.GetValue<Float3>();


            axisD = rotationD.GetItem("axis");

            if (axisD) rotationAxis = axisD.GetValue<Axis>();


            angleD = rotationD.GetItem("angle");

            if (angleD) rotationAngle = angleD.GetValue<float>();


            rescaleD = rotationD.GetItem("rescale");

            if (rescaleD) rotationRescale = rescaleD.GetValue<bool>();
        }

        // Face Render Checks

        bool renderEastFace = false;
        bool renderWestFace = false;
        bool renderDownFace = false;
        bool renderUpFace = false;
        bool renderNorthFace = false;
        bool renderSouthFace = false;

        var eastFaceD = facesD.GetItem("east");
        var westFaceD = facesD.GetItem("west");
        var downFaceD = facesD.GetItem("down");
        var upFaceD = facesD.GetItem("up");
        var northFaceD = facesD.GetItem("north");
        var southFaceD = facesD.GetItem("south");

        int eastTextureID = 0;
        int westTextureID = 0;
        int downTextureID = 0;
        int upTextureID = 0;
        int northTextureID = 0;
        int southTextureID = 0;

        var eastTextureUVs  = new UVRect(0, 0, 16, 16);
        var westTextureUVs  = new UVRect(0, 0, 16, 16);
        var downTextureUVs  = new UVRect(0, 0, 16, 16);
        var upTextureUVs    = new UVRect(0, 0, 16, 16);
        var northTextureUVs = new UVRect(0, 0, 16, 16);
        var southTextureUVs = new UVRect(0, 0, 16, 16);

        float eastUVRotation = 0;
        float westUVRotation = 0;
        float downUVRotation = 0;
        float upUVRotation = 0;
        float northUVRotation = 0;
        float southUVRotation = 0;

        // Adjacent blocks
        var downBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, -1, 0));
        var upBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 1, 0));
        var northBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, -1));
        var southBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, 1));
        var westBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(-1, 0, 0));
        var eastBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(1, 0, 0));

        // Maybe put downBlock, upBlock, northBlock, etc... in a separate struct?

        GM_PopulateModelProperties(eastFaceD, ref renderEastFace, ref eastTextureUVs, ref eastTextureID, ref eastUVRotation,
            from, to,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(westFaceD, ref renderWestFace, ref westTextureUVs, ref westTextureID, ref westUVRotation,
            from, to,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(downFaceD, ref renderDownFace, ref downTextureUVs, ref downTextureID, ref downUVRotation,
            from, to,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(upFaceD, ref renderUpFace, ref upTextureUVs, ref upTextureID, ref upUVRotation,
            from, to,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(northFaceD, ref renderNorthFace, ref northTextureUVs, ref northTextureID, ref northUVRotation,
            from, to,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        GM_PopulateModelProperties(southFaceD, ref renderSouthFace, ref southTextureUVs, ref southTextureID, ref southUVRotation,
            from, to,
            downBlock, upBlock, northBlock, southBlock, westBlock, eastBlock);

        // Create Vertices

        from /= 16;
        to /= 16;
        rotationOrigin /= 16;

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

        foreach (var vert in blockVerts)
        {
            Float3 untransformedVert = new(vert.x == 0 ? from.x : to.x, vert.y == 0 ? from.y : to.y, vert.z == 0 ? from.z : to.z);

            Float3 transformedVert = untransformedVert;

            if (rotationD && rotationAngle != 0) // Avoid unnecessary calculation
            {
                if (originD) transformedVert -= rotationOrigin;

                BaseVectors basisVectors;

                switch (rotationAxis)
                {
                    case Axis.Xaxis:
                        basisVectors = BaseVectors.FromXRotation(rotationAngle);
                        break;
                    case Axis.Zaxis:
                        basisVectors = BaseVectors.FromZRotation(rotationAngle);
                        break;
                    default:
                        basisVectors = BaseVectors.FromYRotation(rotationAngle);
                        break;
                }

                Float3 scaler = new(1, 1, 1);

                if (rotationRescale)
                {
                    float mult = Maths.Sec(45f * Maths.TriangleWave(rotationAngle / 45f));

                    switch (rotationAxis)
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

                if (originD) transformedVert += rotationOrigin;
            }

            vertices.Add(transformedVert + localBlockPosition);
        }

        // Minecraft model UVs go:
        // from: 0, 0  (top left)
        // to:   16,16 (bottom right)

        // But 3D model UVs go:
        // from: 0, 0  (bottom left)
        // to:   16,16 (top right)

        eastTextureUVs = eastTextureUVs.FlipV();
        westTextureUVs = westTextureUVs.FlipV();
        downTextureUVs = downTextureUVs.FlipV();
        upTextureUVs = upTextureUVs.FlipV();
        northTextureUVs = northTextureUVs.FlipV();
        southTextureUVs = southTextureUVs.FlipV();

        List<(string, int)> squareFaces = new();

        if (renderWestFace) 
        {
            GM_GenerateModelFace(westUVRotation, westTextureUVs, westTextureID, [1, 2, 3, 4], 1,
                ref textureCoords, ref squareFaces, ref elementIndex);
        }

        if (renderEastFace)
        {
            GM_GenerateModelFace(eastUVRotation, eastTextureUVs, eastTextureID, [8, 5, 6, 7], 2,
                ref textureCoords, ref squareFaces, ref elementIndex);
        }

        if (renderDownFace)
        {
            GM_GenerateModelFace(downUVRotation, downTextureUVs, downTextureID, [1, 5, 8, 2], 3,
                ref textureCoords, ref squareFaces, ref elementIndex);
        }

        if (renderUpFace)
        {
            GM_GenerateModelFace(upUVRotation, upTextureUVs, upTextureID, [3, 7, 6, 4], 4,
                ref textureCoords, ref squareFaces, ref elementIndex);
        }

        if (renderNorthFace)
        {
            GM_GenerateModelFace(northUVRotation, northTextureUVs, northTextureID, [5, 1, 4, 6], 5,
                ref textureCoords, ref squareFaces, ref elementIndex);
        }

        if (renderSouthFace)
        {
            GM_GenerateModelFace(southUVRotation, southTextureUVs, southTextureID, [2, 8, 7, 3], 6,
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
    private void GM_PopulateModelProperties(DynamicData faceD, ref bool renderFace, ref UVRect uvs, ref int textureID, ref float rotation,
        Float3 from, Float3 to,
        BlockData downBlock, BlockData upBlock, BlockData northBlock, BlockData southBlock, BlockData westBlock, BlockData eastBlock)
    {
        if (faceD)
        {
            var cullfaceD = faceD.GetItem("cullface");

            if (cullfaceD)
            {
                string cullface = cullfaceD.GetValue<string>();

                string adjacentBlockId = "minecraft:air";

                switch (cullface)
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

                renderFace = adjacentBlockId == "minecraft:air"; // To be more sophisticated!
            }
            else renderFace = true;

            if (renderFace)
            {
                var textureD = faceD.GetItem("texture");

                if (textureD)
                {
                    textureID = TextureManager.GetTextureID(textureD.GetValue<string>());
                }

                var uvD = faceD.GetItem("uv");

                if (uvD)
                {
                    uvs = uvD.GetValue<UVRect>();
                }
                else
                {
                    Float3 fromF = 16 - from;

                    Float3 size = to - from;

                    Float3 sizeF = 16 - size;

                    switch (faceD.name)
                    {
                        case "east":

                            uvs = new UVRect(sizeF.z - from.z, sizeF.y - from.y, fromF.z, fromF.y);

                            break;
                        case "west":

                            uvs = new UVRect(from.z, sizeF.y - from.y, from.z + size.z, fromF.y);

                            break;
                        case "down":

                            uvs = new UVRect(from.x, sizeF.z - from.z, from.x + size.x, size.z + from.z);

                            break;
                        case "up":

                            uvs = new UVRect(from.x, from.z, size.x + from.x, size.z + from.z);

                            break;
                        case "north":

                            uvs = new UVRect(sizeF.x - from.x, sizeF.y - from.y, fromF.x, fromF.y);

                            break;
                        case "south":

                            uvs = new UVRect(from.x, sizeF.y - from.y, from.x + size.x, fromF.y);

                            break;
                    }
                }

                var rotateD = faceD.GetItem("rotation");

                if (rotateD)
                {
                    var rotateV = rotateD.GetValue<int>();

                    if (rotateV == 0 || rotateV == 90 || rotateV == 180 || rotateV == 270)
                    {
                        rotation = rotateV;
                    }
                    else
                    {
                        Program.LogWarning($"Invalid texture rotation value '{rotateV}'");
                    }
                }
            }
        }

        uvs /= 16;
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

    public BlockData GetBlockAtLocalPosition(Int3 localPosition)
    {
        if (!IsLocalPositionValid(localPosition))
        {
            Program.LogWarning("GetBlockAtPosition() Position out of range!");
            return new BlockData();
        }

        return blocks[localPosition.x, localPosition.y, localPosition.z];
    }

    public void SetBlockAtLocalPosition(Int3 localPosition, BlockData block)
    {
        if (!IsLocalPositionValid(localPosition))
        {
            Program.LogWarning("SetBlockAtLocalPosition() Cannot set block! Out of bounds!");
            return;
        }

        blocks[localPosition.x, localPosition.y, localPosition.z] = block;
        meshChanged = true;
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
public struct BlockData
{
    public string block_id;

    public BlockData()
    {
        block_id = "minecraft:air";
    }
    public BlockData(string block_id)
    {
        this.block_id = block_id;
    }

    public override string ToString()
    {
        return block_id;
    }
}
public struct BlockRaycastHit
{
    public BlockData blockHit;
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
    public BlockRaycastHit(BlockData blockHit, Int3 blockPosition, BlockFace blockFace, Float3 hitPosition)
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
public struct BlockFace
{
    public BlockDirection direction;

    public BlockFace(BlockDirection direction = BlockDirection.SELF)
    {
        this.direction = direction;
    }

    public Float3 GetNormal()
    {
        return GetNormal(direction);
    }

    public static Float3 GetNormal(BlockDirection direction)
    {
        switch (direction)
        {
            case BlockDirection.NORTH:
                return new(0, 0, -1);
            case BlockDirection.DOWN:
                return new(0, -1, 0);
            case BlockDirection.WEST:
                return new(-1, 0, 0);
            case BlockDirection.SOUTH:
                return new(0, 0, 1);
            case BlockDirection.UP:
                return new(0, 1, 0);
            case BlockDirection.EAST:
                return new(1, 0, 0);
            default:
                return new(0, 0, 0);
        }
    }

    public static BlockDirection Opposite(BlockDirection blockDirection)
    {
        switch (blockDirection)
        {
            case BlockDirection.UP:
                return BlockDirection.DOWN;
            case BlockDirection.DOWN:
                return BlockDirection.UP;
            case BlockDirection.NORTH:
                return BlockDirection.SOUTH;
            case BlockDirection.SOUTH:
                return BlockDirection.NORTH;
            case BlockDirection.EAST:
                return BlockDirection.WEST;
            case BlockDirection.WEST:
                return BlockDirection.EAST;
            default:
                return BlockDirection.SELF;
        }
    }

    public override string ToString()
    {
        return direction.ToString();
    }
}
public enum BlockDirection
{
    SELF,
    DOWN,
    EAST,
    NORTH,
    SOUTH,
    UP,
    WEST
}