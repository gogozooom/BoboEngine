using BoboEngine;
using BoboEngine.Shaders;
using ConsoleCommand;

public static class WorldDataManager
{
    public static Shader meshShader = new Shader(TextureAtlasManager.blockTextureAtlas);

    public static Dictionary<Int3, Chunk> chunks = new();

    public static void GenerateTestChunk()
    {
        var position = new Int3(0, 0, 0);

        var chunk = new Chunk(position);

        chunks.Add(position, chunk);
        chunk.SetBlockAtLocalPosition(new(0, 0, 0), new("minecraft:dirt"));

        chunk.GenerateMesh();
    }

    public static Chunk GetChunkInPosition(Int3 position)
    {
        Int3 chunkPos = new((int)MathF.Floor(position.x / 16f), (int)MathF.Floor(position.y / 16f), (int)MathF.Floor(position.z / 16f));

        if (!chunks.ContainsKey(chunkPos))
        {
            // Revert later
            chunks.Add(chunkPos, new Chunk(chunkPos));
            //return null;
        }

        return chunks[chunkPos];
    }
    public static BlockData GetBlockAtPosition(Int3 position)
    {
        var chunk = GetChunkInPosition(position);

        var localPosition = chunk.WorldPositionToLocal(position);

        return chunk.GetBlockAtLocalPosition(localPosition);
    }

    public static void SetBlock(string block_id, Int3 position)
    {
        var chunk = GetChunkInPosition(position);

        if(chunk == null)
        {
            Program.Log("Chunk not loaded yet!");
            return;
        }

        chunk.SetBlockAtLocalPosition(chunk.WorldPositionToLocal(position), new(block_id));
    }

    [Command("SetBlock", "['block_id', #x, #y, #z]")]
    public static void SetBlock(string block_id, int x, int y, int z)
    {
        SetBlock(block_id, new(x, y, z));

        GetChunkInPosition(new(x,y,z)).GenerateMesh();

        Program.Log("Set block to: " + block_id);
    }

    [Command("Fill", "['block_id', #x1, #y1, #z1, #x2, #y2, #z2]")]
    public static void FillBlocks(string block_id, int x1, int y1, int z1, int x2, int y2, int z2)
    {
        int minX = x1;
        int maxX = x2;

        if(x1 > x2)
        {
            minX = x2;
            maxX = x1;
        }

        int minY = y1;
        int maxY = y2;

        if(y1 > y2)
        {
            minY = y2;
            maxY = y1;
        }

        int minZ = z1;
        int maxZ = z2;

        if(z1 > z2)
        {
            minZ = z2;
            maxZ = z1;
        }

        List<Chunk> chunks = new List<Chunk>();

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    var chunk = GetChunkInPosition(new(x, y, z));

                    if (chunk == null) continue;

                    if (!chunks.Contains(chunk))
                    {
                        chunks.Add(chunk);
                    }

                    chunk.SetBlockAtLocalPosition(chunk.WorldPositionToLocal(new(x, y, z)), new(block_id));
                }
            }
        }

        foreach (var chunk in chunks)
        {
            chunk.GenerateMesh();
        }
    }

    [Command("ClearChunk", "[#x, #y, #z]")]
    public static void ClearChunk(int x, int y, int z)
    {
        var chunk = GetChunkInPosition(new(x, y, z));

        if (chunk == null)
        {
            Program.LogError("Chunk not loaded");
            return;
        }

        chunk.ClearMesh();
    }

    [Command("FRChunk", "[#x, #y, #z] Force Refresh Chunk")]
    public static void ForceRefresh(int x, int y, int z)
    {
        var chunk = GetChunkInPosition(new(x, y, z));

        if (chunk == null)
        {
            Program.LogError("Chunk not loaded");
            return;
        }

        chunk.GenerateMesh();
    }
}

public class Chunk
{
    public readonly Int3 chunkPosition;

    private BlockData[,,] blocks = new BlockData[16, 16, 16];

    public Chunk(Int3 position)
    {
        chunkPosition = position;

        ClearMesh();
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
        _connectedMesh?.DeleteMesh();
    }

    private Mesh _connectedMesh;

    public void GenerateMesh() // TODO: Fire this only when both: The block data has changed AND on late update!
    {
        if (!_connectedMesh)
        {
            _connectedMesh = new GameObject("Chunk: " + chunkPosition, true).AddComponent<Mesh>();
            _connectedMesh.shader = WorldDataManager.meshShader;
        }

        _connectedMesh.DeleteMesh();

        List<Float3> vertices = new();
        List<FaceInfo> faces = new();
        List<Float3> normals = new();
        List<Float2> textureCoords = new();

        int blockIndex = 0;

        for (int x = 0; x < blocks.GetLength(0); x++)
        {
            for (int y = 0; y < blocks.GetLength(1); y++)
            {
                for (int z = 0; z < blocks.GetLength(2); z++)
                {
                    BlockData block = blocks[x, y, z];

                    if (block.block_id == "minecraft:air" || block.block_id == "minecraft:void") continue;

                    Int3 blockWorldPosition = new Int3(x, y, z) + (chunkPosition * 16);
                    BlockType blockType = BlockTypeManager.GetBlockData(block.block_id);

                    vertices.Add(new Float3(0, 0, 0) + blockWorldPosition);
                    vertices.Add(new Float3(0, 0, 1) + blockWorldPosition);
                    vertices.Add(new Float3(0, 1, 1) + blockWorldPosition);
                    vertices.Add(new Float3(0, 1, 0) + blockWorldPosition);
                    vertices.Add(new Float3(1, 0, 0) + blockWorldPosition);
                    vertices.Add(new Float3(1, 1, 0) + blockWorldPosition);
                    vertices.Add(new Float3(1, 1, 1) + blockWorldPosition);
                    vertices.Add(new Float3(1, 0, 1) + blockWorldPosition);

                    //* Not needed for now
                    // Right Face
                    normals.Add(new(-1, 0, 0));
                    // Left Face
                    normals.Add(new(1, 0, 0));
                    // Bottom Face
                    normals.Add(new(0, -1, 0));
                    // Top Face
                    normals.Add(new(0, 1, 0));
                    // Back Face
                    normals.Add(new(0, 0, -1));
                    // Front Face
                    normals.Add(new(0, 0, 1));
                    //*/

                    // Right Face
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.rightTexture, new(0, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.rightTexture, new(1, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.rightTexture, new(1, 1)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.rightTexture, new(0, 1)));

                    // Left Face
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.leftTexture, new(0, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.leftTexture, new(1, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.leftTexture, new(1, 1)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.leftTexture, new(0, 1)));

                    // Bottom Face
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.bottomTexture, new(0, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.bottomTexture, new(1, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.bottomTexture, new(1, 1)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.bottomTexture, new(0, 1)));

                    // Top Face
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.topTexture, new(0, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.topTexture, new(1, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.topTexture, new(1, 1)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.topTexture, new(0, 1)));

                    // Back Face
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.backTexture, new(0, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.backTexture, new(1, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.backTexture, new(1, 1)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.backTexture, new(0, 1)));

                    // Front Face
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.frontTexture, new(0, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.frontTexture, new(1, 0)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.frontTexture, new(1, 1)));
                    textureCoords.Add(TextureAtlasManager.GetUVPositionOnTexture(blockType.frontTexture, new(0, 1)));

                    var vO = blockIndex * 8;
                    var tO = blockIndex * 24;

                    List<string> squareFaces =
                    [
                        $"f {1 + vO}/{1 + tO}/1 {2 + vO}/{2 + tO}/1 {3 + vO}/{3 + tO}/1 {4 + vO}/{4 + tO}/1", // Right Face
                        $"f {8 + vO}/{5 + tO}/2 {5 + vO}/{6 + tO}/2 {6 + vO}/{7 + tO}/2 {7 + vO}/{8 + tO}/2", // Left Face
                        $"f {1 + vO}/{9 + tO}/3 {5 + vO}/{10 + tO}/3 {8 + vO}/{11 + tO}/3 {2 + vO}/{12 + tO}/3", // Bottom Face
                        $"f {3 + vO}/{13 + tO}/4 {7 + vO}/{14 + tO}/4 {6 + vO}/{15 + tO}/4 {4 + vO}/{16 + tO}/4", // Top Face
                        $"f {5 + vO}/{21 + tO}/5 {1 + vO}/{22 + tO}/5 {4 + vO}/{23 + tO}/5 {6 + vO}/{24 + tO}/5", // Back Face
                        $"f {2 + vO}/{17 + tO}/6 {8 + vO}/{18 + tO}/6 {7 + vO}/{19 + tO}/6 {3 + vO}/{20 + tO}/6", // Front Face
                    ];

                    foreach (var face in squareFaces)
                    {
                        foreach (var f in FaceInfo.GetTriangulatedFaces(face))
                        {
                            faces.Add(f);
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
    }

    public Int3 WorldPositionToLocal(Int3 worldPosition) => worldPosition - (chunkPosition * 16);

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
        block_id = "minecraft:void";
    }
    public BlockData(string block_id)
    {
        this.block_id = block_id;
    }
}