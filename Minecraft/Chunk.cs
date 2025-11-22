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

    public void GenerateMesh() // TODO: Fire this only when both: The block data has changed AND on late update!
    {
        _connectedMesh.DeleteMesh();

        List<Float3> vertices = new();
        List<FaceInfo> faces = new();
        List<Float2> textureCoords = new();
        List<Float3> normals = [
            new(-1, 0, 0),
            new(-1, 0, 0),
            new( 0,-1, 0),
            new( 0, 1, 0),
            new( 0, 0,-1),
            new( 0, 0, 1),
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

                    DynamicData[] elements = elementData.GetImmediateChildren();

                    if (!elementData)
                    {
                        Program.LogWarning($"'{blockType}' does not have any model elements!");
                        continue;
                    }

                    // Flipped order to allow block overlays to render over
                    for (int i = elements.Length - 1; i >= 0; i--)
                    {
                        GenerateBlockMesh(blockType, localBlockPosition, ref vertices, ref faces, ref textureCoords, elementIndex, elements[i]);

                        elementIndex++;
                    }
                }
            }
        }

        _connectedMesh.LoadRawData(vertices.ToArray(), faces.ToArray(), normals.ToArray(), textureCoords.ToArray());
    }
    private void GenerateBlockMesh(BlockType blockType, Int3 localBlockPosition, ref List<Float3> vertices, ref List<FaceInfo> faces, ref List<Float2> textureCoords, int elementIndex, DynamicData elementData)
    {
        var facesD = elementData.GetItem("faces");

        if (!facesD)
        {
            Program.LogWarning($"'{blockType}' has elements without any faces!");
            return;
        }

        Int3 worldBlockPosition = LocalPositionToWorld(localBlockPosition);


        // Adjacent blocks
        var westBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(-1, 0, 0));
        var eastBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(1, 0, 0));
        var downBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, -1, 0));
        var upBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 1, 0));
        var northBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, -1));
        var southBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, 1));

        // Create Vertice Bounds

        Float3 from = new(0, 0, 0);
        Float3 to = new(16, 16, 16);

        var fromD = elementData.GetItem("from");
        var toD = elementData.GetItem("to");

        if (fromD) from = fromD.GetValue<Float3>();
        if (toD) to = toD.GetValue<Float3>();

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

        // For Loop to avoid tedious code duplication
        // Yes I know, a function serves the same purpose, shut up... (I'll do that in refactoring later...)
        for (int i = 0; i < 6; i++)
        {
            DynamicData faceD;

            // Get faceD
            switch (i)
            {
                case 0:
                    faceD = eastFaceD;
                    break;
                case 1:
                    faceD = westFaceD;
                    break;
                case 2:
                    faceD = downFaceD;
                    break;
                case 3:
                    faceD = upFaceD;
                    break;
                case 4:
                    faceD = northFaceD;
                    break;
                default:
                    faceD = southFaceD;
                    break;
            }

            UVRect uvs = new UVRect(0, 0, 16, 16);
            bool renderFace = false;
            int textureID = 0;
            float rotation = 0;

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

                        switch (i)
                        {
                            case 0: // East

                                uvs = new UVRect(sizeF.z - from.z, sizeF.y - from.y, fromF.z, fromF.y);

                                break;
                            case 1: // West

                                uvs = new UVRect(from.z, sizeF.y - from.y, from.z + size.z, fromF.y);

                                break;
                            case 2: // Down

                                uvs = new UVRect(from.x, sizeF.z - from.z, from.x + size.x, size.z + from.z);

                                break;
                            case 3: // Up

                                uvs = new UVRect(from.x, from.z, size.x + from.x, size.z + from.z);

                                break;
                            case 4: // North

                                uvs = new UVRect(sizeF.x - from.x, sizeF.y - from.y, fromF.x, fromF.y);

                                break;
                            default: // South

                                uvs = new UVRect(from.x, sizeF.y - from.y, from.x + size.x, fromF.y);

                                break;
                        }
                    }

                    var rotateD = faceD.GetItem("rotation");

                    if (rotateD)
                    {
                        var rotateV = rotateD.GetValue<int>();

                        if(rotateV == 0 || rotateV == 90 || rotateV == 180 || rotateV == 270)
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

            // Apply results
            switch (i)
            {
                case 0:
                    renderEastFace = renderFace;
                    eastTextureID = textureID;
                    eastTextureUVs = uvs;
                    eastUVRotation = rotation;
                    break;
                case 1:
                    renderWestFace = renderFace;
                    westTextureID = textureID;
                    westTextureUVs = uvs;
                    westUVRotation = rotation;
                    break;
                case 2:
                    renderDownFace = renderFace;
                    downTextureID = textureID;
                    downTextureUVs = uvs;
                    downUVRotation = rotation;
                    break;
                case 3:
                    renderUpFace = renderFace;
                    upTextureID = textureID;
                    upTextureUVs = uvs;
                    upUVRotation = rotation;
                    break;
                case 4:
                    renderNorthFace = renderFace;
                    northTextureID = textureID;
                    northTextureUVs = uvs;
                    northUVRotation = rotation;
                    break;
                case 5:
                    renderSouthFace = renderFace;
                    southTextureID = textureID;
                    southTextureUVs = uvs;
                    southUVRotation = rotation;
                    break;
            }
        }


        // Create Vertices

        from /= 16;
        to /= 16;

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
            Float3 transformedVert = new(vert.x == 0 ? from.x : to.x, vert.y == 0 ? from.y : to.y, vert.z == 0 ? from.z : to.z);

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
            // West Face

            switch (westUVRotation)
            {
                default:
                    textureCoords.Add(new(westTextureUVs.uMin, westTextureUVs.vMin));
                    textureCoords.Add(new(westTextureUVs.uMax, westTextureUVs.vMin));
                    textureCoords.Add(new(westTextureUVs.uMax, westTextureUVs.vMax));
                    textureCoords.Add(new(westTextureUVs.uMin, westTextureUVs.vMax));
                    break;
                case 90f:
                    textureCoords.Add(new(westTextureUVs.uMax, westTextureUVs.vMin));
                    textureCoords.Add(new(westTextureUVs.uMax, westTextureUVs.vMax));
                    textureCoords.Add(new(westTextureUVs.uMin, westTextureUVs.vMax));
                    textureCoords.Add(new(westTextureUVs.uMin, westTextureUVs.vMin));
                    break;
                case 180f:
                    textureCoords.Add(new(westTextureUVs.uMax, westTextureUVs.vMax));
                    textureCoords.Add(new(westTextureUVs.uMin, westTextureUVs.vMax));
                    textureCoords.Add(new(westTextureUVs.uMin, westTextureUVs.vMin));
                    textureCoords.Add(new(westTextureUVs.uMax, westTextureUVs.vMin));
                    break;
                case 270f:
                    textureCoords.Add(new(westTextureUVs.uMin, westTextureUVs.vMax));
                    textureCoords.Add(new(westTextureUVs.uMin, westTextureUVs.vMin));
                    textureCoords.Add(new(westTextureUVs.uMax, westTextureUVs.vMin));
                    textureCoords.Add(new(westTextureUVs.uMax, westTextureUVs.vMax));
                    break;
            }

            var uvO = textureCoords.Count();

            var vO = elementIndex * 8;

            squareFaces.Add(($"f {1 + vO}/{uvO - 3}/1 " +
                              $"{2 + vO}/{uvO - 2}/1 " +
                              $"{3 + vO}/{uvO - 1}/1 " +
                              $"{4 + vO}/{uvO}/1",
                              westTextureID
                              ));
        }

        if (renderEastFace)
        {
            // East Face

            switch (eastUVRotation)
            {
                default:
                    textureCoords.Add(new(eastTextureUVs.uMin, eastTextureUVs.vMin));
                    textureCoords.Add(new(eastTextureUVs.uMax, eastTextureUVs.vMin));
                    textureCoords.Add(new(eastTextureUVs.uMax, eastTextureUVs.vMax));
                    textureCoords.Add(new(eastTextureUVs.uMin, eastTextureUVs.vMax));
                    break;
                case 90f:
                    textureCoords.Add(new(eastTextureUVs.uMax, eastTextureUVs.vMin));
                    textureCoords.Add(new(eastTextureUVs.uMax, eastTextureUVs.vMax));
                    textureCoords.Add(new(eastTextureUVs.uMin, eastTextureUVs.vMax));
                    textureCoords.Add(new(eastTextureUVs.uMin, eastTextureUVs.vMin));
                    break;
                case 180f:
                    textureCoords.Add(new(eastTextureUVs.uMax, eastTextureUVs.vMax));
                    textureCoords.Add(new(eastTextureUVs.uMin, eastTextureUVs.vMax));
                    textureCoords.Add(new(eastTextureUVs.uMin, eastTextureUVs.vMin));
                    textureCoords.Add(new(eastTextureUVs.uMax, eastTextureUVs.vMin));
                    break;
                case 270f:
                    textureCoords.Add(new(eastTextureUVs.uMin, eastTextureUVs.vMax));
                    textureCoords.Add(new(eastTextureUVs.uMin, eastTextureUVs.vMin));
                    textureCoords.Add(new(eastTextureUVs.uMax, eastTextureUVs.vMin));
                    textureCoords.Add(new(eastTextureUVs.uMax, eastTextureUVs.vMax));
                    break;
            }

            var uvO = textureCoords.Count();

            var vO = elementIndex * 8;

            squareFaces.Add(($"f {8 + vO}/{uvO - 3}/2 " +
                              $"{5 + vO}/{uvO - 2}/2 " +
                              $"{6 + vO}/{uvO - 1}/2 " +
                              $"{7 + vO}/{uvO}/2",
                              eastTextureID
                              ));
        }

        if (renderDownFace)
        {
            // Down Face

            switch (downUVRotation)
            {
                default:
                    textureCoords.Add(new(downTextureUVs.uMin, downTextureUVs.vMin));
                    textureCoords.Add(new(downTextureUVs.uMax, downTextureUVs.vMin));
                    textureCoords.Add(new(downTextureUVs.uMax, downTextureUVs.vMax));
                    textureCoords.Add(new(downTextureUVs.uMin, downTextureUVs.vMax));
                    break;
                case 90f:
                    textureCoords.Add(new(downTextureUVs.uMax, downTextureUVs.vMin));
                    textureCoords.Add(new(downTextureUVs.uMax, downTextureUVs.vMax));
                    textureCoords.Add(new(downTextureUVs.uMin, downTextureUVs.vMax));
                    textureCoords.Add(new(downTextureUVs.uMin, downTextureUVs.vMin));
                    break;
                case 180f:
                    textureCoords.Add(new(downTextureUVs.uMax, downTextureUVs.vMax));
                    textureCoords.Add(new(downTextureUVs.uMin, downTextureUVs.vMax));
                    textureCoords.Add(new(downTextureUVs.uMin, downTextureUVs.vMin));
                    textureCoords.Add(new(downTextureUVs.uMax, downTextureUVs.vMin));
                    break;
                case 270f:
                    textureCoords.Add(new(downTextureUVs.uMin, downTextureUVs.vMax));
                    textureCoords.Add(new(downTextureUVs.uMin, downTextureUVs.vMin));
                    textureCoords.Add(new(downTextureUVs.uMax, downTextureUVs.vMin));
                    textureCoords.Add(new(downTextureUVs.uMax, downTextureUVs.vMax));
                    break;
            }

            var uvO = textureCoords.Count();

            var vO = elementIndex * 8;

            squareFaces.Add(($"f {1 + vO}/{uvO - 3}/3 " +
                              $"{5 + vO}/{uvO - 2}/3 " +
                              $"{8 + vO}/{uvO - 1}/3 " +
                              $"{2 + vO}/{uvO}/3",
                              downTextureID
                              ));
        }

        if (renderUpFace)
        {
            // Up Face

            switch (upUVRotation)
            {
                default:
                    textureCoords.Add(new(upTextureUVs.uMin, upTextureUVs.vMin));
                    textureCoords.Add(new(upTextureUVs.uMax, upTextureUVs.vMin));
                    textureCoords.Add(new(upTextureUVs.uMax, upTextureUVs.vMax));
                    textureCoords.Add(new(upTextureUVs.uMin, upTextureUVs.vMax));
                    break;
                case 90f:
                    textureCoords.Add(new(upTextureUVs.uMax, upTextureUVs.vMin));
                    textureCoords.Add(new(upTextureUVs.uMax, upTextureUVs.vMax));
                    textureCoords.Add(new(upTextureUVs.uMin, upTextureUVs.vMax));
                    textureCoords.Add(new(upTextureUVs.uMin, upTextureUVs.vMin));
                    break;
                case 180f:
                    textureCoords.Add(new(upTextureUVs.uMax, upTextureUVs.vMax));
                    textureCoords.Add(new(upTextureUVs.uMin, upTextureUVs.vMax));
                    textureCoords.Add(new(upTextureUVs.uMin, upTextureUVs.vMin));
                    textureCoords.Add(new(upTextureUVs.uMax, upTextureUVs.vMin));
                    break;
                case 270f:
                    textureCoords.Add(new(upTextureUVs.uMin, upTextureUVs.vMax));
                    textureCoords.Add(new(upTextureUVs.uMin, upTextureUVs.vMin));
                    textureCoords.Add(new(upTextureUVs.uMax, upTextureUVs.vMin));
                    textureCoords.Add(new(upTextureUVs.uMax, upTextureUVs.vMax));
                    break;
            }

            var uvO = textureCoords.Count();

            var vO = elementIndex * 8;

            squareFaces.Add(($"f {3 + vO}/{uvO - 3}/4 " +
                              $"{7 + vO}/{uvO - 2}/4 " +
                              $"{6 + vO}/{uvO - 1}/4 " +
                              $"{4 + vO}/{uvO}/4",
                              upTextureID
                              ));
        }

        if (renderNorthFace)
        {
            // North Face

            switch (northUVRotation)
            {
                default:
                    textureCoords.Add(new(northTextureUVs.uMin, northTextureUVs.vMin));
                    textureCoords.Add(new(northTextureUVs.uMax, northTextureUVs.vMin));
                    textureCoords.Add(new(northTextureUVs.uMax, northTextureUVs.vMax));
                    textureCoords.Add(new(northTextureUVs.uMin, northTextureUVs.vMax));
                    break;
                case 90f:
                    textureCoords.Add(new(northTextureUVs.uMax, northTextureUVs.vMin));
                    textureCoords.Add(new(northTextureUVs.uMax, northTextureUVs.vMax));
                    textureCoords.Add(new(northTextureUVs.uMin, northTextureUVs.vMax));
                    textureCoords.Add(new(northTextureUVs.uMin, northTextureUVs.vMin));
                    break;
                case 180f:
                    textureCoords.Add(new(northTextureUVs.uMax, northTextureUVs.vMax));
                    textureCoords.Add(new(northTextureUVs.uMin, northTextureUVs.vMax));
                    textureCoords.Add(new(northTextureUVs.uMin, northTextureUVs.vMin));
                    textureCoords.Add(new(northTextureUVs.uMax, northTextureUVs.vMin));
                    break;
                case 270f:
                    textureCoords.Add(new(northTextureUVs.uMin, westTextureUVs.vMax));
                    textureCoords.Add(new(northTextureUVs.uMin, northTextureUVs.vMin));
                    textureCoords.Add(new(northTextureUVs.uMax, northTextureUVs.vMin));
                    textureCoords.Add(new(northTextureUVs.uMax, northTextureUVs.vMax));
                    break;
            }

            var uvO = textureCoords.Count();

            var vO = elementIndex * 8;

            squareFaces.Add(($"f {5 + vO}/{uvO - 3}/5 " +
                              $"{1 + vO}/{uvO - 2}/5 " +
                              $"{4 + vO}/{uvO - 1}/5 " +
                              $"{6 + vO}/{uvO}/5",
                              northTextureID
                              ));
        }

        if (renderSouthFace)
        {
            // South Face

            switch (southUVRotation)
            {
                default:
                    textureCoords.Add(new(southTextureUVs.uMin, southTextureUVs.vMin));
                    textureCoords.Add(new(southTextureUVs.uMax, southTextureUVs.vMin));
                    textureCoords.Add(new(southTextureUVs.uMax, southTextureUVs.vMax));
                    textureCoords.Add(new(southTextureUVs.uMin, southTextureUVs.vMax));
                    break;
                case 90f:
                    textureCoords.Add(new(southTextureUVs.uMax, southTextureUVs.vMin));
                    textureCoords.Add(new(southTextureUVs.uMax, southTextureUVs.vMax));
                    textureCoords.Add(new(southTextureUVs.uMin, southTextureUVs.vMax));
                    textureCoords.Add(new(southTextureUVs.uMin, southTextureUVs.vMin));
                    break;
                case 180f:
                    textureCoords.Add(new(southTextureUVs.uMax, southTextureUVs.vMax));
                    textureCoords.Add(new(southTextureUVs.uMin, southTextureUVs.vMax));
                    textureCoords.Add(new(southTextureUVs.uMin, southTextureUVs.vMin));
                    textureCoords.Add(new(southTextureUVs.uMax, southTextureUVs.vMin));
                    break;
                case 270f:
                    textureCoords.Add(new(southTextureUVs.uMin, southTextureUVs.vMax));
                    textureCoords.Add(new(southTextureUVs.uMin, southTextureUVs.vMin));
                    textureCoords.Add(new(southTextureUVs.uMax, southTextureUVs.vMin));
                    textureCoords.Add(new(southTextureUVs.uMax, southTextureUVs.vMax));
                    break;
            }

            var uvO = textureCoords.Count();

            var vO = elementIndex * 8;

            squareFaces.Add(($"f {2 + vO}/{uvO - 3}/6 " +
                              $"{8 + vO}/{uvO - 2}/6 " +
                              $"{7 + vO}/{uvO - 1}/6 " +
                              $"{3 + vO}/{uvO}/6",
                              southTextureID
                              ));
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