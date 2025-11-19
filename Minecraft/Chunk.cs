using BoboEngine;
using BoboEngine.Shaders;

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
        List<Float2> textureCoords = [
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),

            ];
        List<Float3> normals = [
            new(-1, 0, 0),
            new(-1, 0, 0),
            new( 0,-1, 0),
            new( 0, 1, 0),
            new( 0, 0,-1),
            new( 0, 0, 1),
            ];


        int blockIndex = 0;

        for (int x = 0; x < blocks.GetLength(0); x++)
        {
            for (int y = 0; y < blocks.GetLength(1); y++)
            {
                for (int z = 0; z < blocks.GetLength(2); z++)
                {
                    BlockData block = blocks[x, y, z];

                    if (block.block_id == "minecraft:air" || block.block_id == "minecraft:void") continue;

                    Int3 localBlockPosition = new(x, y, z);
                    Int3 worldBlockPosition = LocalPositionToWorld(localBlockPosition);

                    BlockType blockType = BlockTypeManager.GetBlockData(block.block_id);

                    var rightBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(-1, 0, 0));
                    var leftBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(1, 0, 0));
                    var bottomBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, -1, 0));
                    var topBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 1, 0));
                    var backBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, -1));
                    var frontBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, 1));

                    bool renderRightFace = rightBlock.block_id == "minecraft:air";
                    bool renderLeftFace = leftBlock.block_id == "minecraft:air";
                    bool renderBottomFace = bottomBlock.block_id == "minecraft:air";
                    bool renderTopFace = topBlock.block_id == "minecraft:air";
                    bool renderBackFace = backBlock.block_id == "minecraft:air";
                    bool renderFrontFace = frontBlock.block_id == "minecraft:air";


                    vertices.Add(new Float3(0, 0, 0) + localBlockPosition);
                    vertices.Add(new Float3(0, 0, 1) + localBlockPosition);
                    vertices.Add(new Float3(0, 1, 1) + localBlockPosition);
                    vertices.Add(new Float3(0, 1, 0) + localBlockPosition);
                    vertices.Add(new Float3(1, 0, 0) + localBlockPosition);
                    vertices.Add(new Float3(1, 1, 0) + localBlockPosition);
                    vertices.Add(new Float3(1, 1, 1) + localBlockPosition);
                    vertices.Add(new Float3(1, 0, 1) + localBlockPosition);

                    List<(string, int)> squareFaces = new();


                    if (renderRightFace)
                    {
                        // Right Face

                        var vO = blockIndex * 8;

                        squareFaces.Add(($"f {1 + vO}/{1}/1 " +
                                          $"{2 + vO}/{2}/1 " +
                                          $"{3 + vO}/{3}/1 " +
                                          $"{4 + vO}/{4}/1",
                                          TextureManager.GetTextureID(blockType.rightTexture)
                                          ));
                    }

                    if (renderLeftFace)
                    {
                        // Left Face

                        var vO = blockIndex * 8;

                        squareFaces.Add(($"f {8 + vO}/{1}/2 " +
                                          $"{5 + vO}/{2}/2 " +
                                          $"{6 + vO}/{3}/2 " +
                                          $"{7 + vO}/{4}/2",
                                          TextureManager.GetTextureID(blockType.leftTexture)
                                          ));
                    }

                    if (renderBottomFace)
                    {
                        // Bottom Face

                        var vO = blockIndex * 8;

                        squareFaces.Add(($"f {1 + vO}/{1}/3 " +
                                          $"{5 + vO}/{2}/3 " +
                                          $"{8 + vO}/{3}/3 " +
                                          $"{2 + vO}/{4}/3",
                                          TextureManager.GetTextureID(blockType.bottomTexture)
                                          ));
                    }

                    if (renderTopFace)
                    {
                        // Top Face

                        var vO = blockIndex * 8;

                        squareFaces.Add(($"f {3 + vO}/{1}/4 " +
                                          $"{7 + vO}/{2}/4 " +
                                          $"{6 + vO}/{3}/4 " +
                                          $"{4 + vO}/{4}/4",
                                          TextureManager.GetTextureID(blockType.topTexture)
                                          ));
                    }

                    if (renderBackFace)
                    {
                        // Back Face

                        var vO = blockIndex * 8;

                        squareFaces.Add(($"f {5 + vO}/{1}/5 " +
                                          $"{1 + vO}/{2}/5 " +
                                          $"{4 + vO}/{3}/5 " +
                                          $"{6 + vO}/{4}/5",
                                          TextureManager.GetTextureID(blockType.backTexture)
                                          ));
                    }

                    if (renderFrontFace)
                    {
                        // Front Face

                        var vO = blockIndex * 8;

                        squareFaces.Add(($"f {2 + vO}/{1}/6 " +
                                          $"{8 + vO}/{2}/6 " +
                                          $"{7 + vO}/{3}/6 " +
                                          $"{3 + vO}/{4}/6",
                                          TextureManager.GetTextureID(blockType.rightTexture)
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

                    blockIndex++;
                }
            }
        }

        _connectedMesh.LoadRawData(vertices.ToArray(), faces.ToArray(), normals.ToArray(), textureCoords.ToArray());
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
            case BlockDirection.DOWN:
                return new(0, -1, 0);
            case BlockDirection.EAST:
                return new(-1, 0, 0);
            case BlockDirection.NORTH:
                return new(0, 0, 1);
            case BlockDirection.SOUTH:
                return new(0, 0, -1);
            case BlockDirection.UP:
                return new(0, 1, 0);
            case BlockDirection.WEST:
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