using BoboEngine;
using BoboEngine.Shaders;
using ConsoleCommand;

public static class WorldDataManager
{
    public static Shader meshShader = new Shader(TextureAtlasManager.blockTextureAtlas);

    public static Dictionary<Int3, Chunk> chunks = new();

    public static void GenerateTestChunk()
    {
        var chunk = CreateChunkObject(new(0,0,0));

        int i = 0;
        foreach (var block in BlockTypeManager.blockData)
        {
            if (block.Key == "minecraft:null") continue;

            chunk.SetBlockAtLocalPosition(new(i * 2, 0, 0), new(block.Key));

            i++;
        }
    }

    private static Chunk CreateChunkObject(Int3 chunkPos)
    {
        var newChunk = new GameObject("Chunk " + chunkPos).AddComponent<Chunk>();

        newChunk.InitChunk(chunkPos);

        chunks.Add(chunkPos, newChunk);

        return newChunk;
    }
    public static Chunk GetChunkInPosition(Int3 position)
    {
        Int3 chunkPos = new((int)MathF.Floor(position.x / 16f), (int)MathF.Floor(position.y / 16f), (int)MathF.Floor(position.z / 16f));

        if (!chunks.ContainsKey(chunkPos))
        {
            // Revert later
            CreateChunkObject(chunkPos);
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
    public static BlockData GetBlockAtPosition(Float3 position) => GetBlockAtPosition(Float3ToBlockPos(position));
    public static Int3 Float3ToBlockPos(Float3 position) => (Int3)new Float3(
                position.x - Maths.Mod(position.x, 1),
                position.y - Maths.Mod(position.y, 1),
                position.z - Maths.Mod(position.z, 1));

    public static BlockRaycastHit Raycast(Float3 raycastPosition, Float3 raycastDirection, float maxRayDistance)
    {
        raycastDirection = raycastDirection.Normalized();
        float distanceTraveled = 0;

        BlockDirection blockDirectionHit = BlockDirection.SELF;
        var positionMarched = raycastPosition;

        /* Possible Optimization
        List<BlockDirection> possibleMarchingDirections = new();

        #region Populate possible marching directions

        if (raycastDirection.x > 0)
        {
            possibleMarchingDirections.Add(BlockDirection.WEST);
        }
        else if (raycastDirection.x < 0)
        {
            possibleMarchingDirections.Add(BlockDirection.EAST);
        }

        if (raycastDirection.y > 0)
        {
            possibleMarchingDirections.Add(BlockDirection.UP);
        }
        else if (raycastDirection.y < 0)
        {
            possibleMarchingDirections.Add(BlockDirection.DOWN);
        }

        if(raycastDirection.z > 0)
        {
            possibleMarchingDirections.Add(BlockDirection.NORTH);
        }
        else if(raycastDirection.z < 0)
        {
            possibleMarchingDirections.Add(BlockDirection.SOUTH);
        }


        #endregion
        */

        Int3 blockPos = Float3ToBlockPos(raycastPosition);

        BlockData currentBlock = GetBlockAtPosition(blockPos);

        while (currentBlock.block_id == "minecraft:air")
        {
            // Point Marched Unit Square
            var c = positionMarched - blockPos;

            // Unit Square Points
            Float3 v1 = new(0, 0, 0); // Down - South - East
            Float3 v2 = new(1, 0, 0); // Down - South - West
            Float3 v3 = new(1, 0, 1); // Down - North - West
            Float3 v4 = new(0, 0, 1); // Down - North - East
            Float3 v5 = new(0, 1, 1); // Up   - North - East
            Float3 v6 = new(1, 1, 1); // Up   - North - West
            Float3 v7 = new(1, 1, 0); // Up   - South - West
            Float3 v8 = new(0, 1, 0); // Up   - South - East

            // Edge Normals
            Float3 n1 = Float3.CrossProduct(v1 - c, v2 - c); // Down  - South
            Float3 n2 = Float3.CrossProduct(v2 - c, v3 - c); // Down  - West
            Float3 n3 = Float3.CrossProduct(v3 - c, v4 - c); // Down  - North
            Float3 n4 = Float3.CrossProduct(v4 - c, v1 - c); // Down  - East

            Float3 n5 = Float3.CrossProduct(v4 - c, v5 - c); // North - East
            Float3 n6 = Float3.CrossProduct(v3 - c, v6 - c); // North - West
            Float3 n7 = Float3.CrossProduct(v2 - c, v7 - c); // South - West
            Float3 n8 = Float3.CrossProduct(v1 - c, v8 - c); // South - East

            Float3 n9 = Float3.CrossProduct(v5 - c, v6 - c); // Up    - North
            Float3 n10 = Float3.CrossProduct(v6 - c, v7 - c); // Up    - West
            Float3 n11 = Float3.CrossProduct(v7 - c, v8 - c); // Up    - South
            Float3 n12 = Float3.CrossProduct(v8 - c, v5 - c); // Up    - East

            // Dot Results
            float r1 = Float3.Dot(n1, raycastDirection);
            float r2 = Float3.Dot(n2, raycastDirection);
            float r3 = Float3.Dot(n3, raycastDirection);
            float r4 = Float3.Dot(n4, raycastDirection);
            float r5 = Float3.Dot(n5, raycastDirection);
            float r6 = Float3.Dot(n6, raycastDirection);

            float r7  = Float3.Dot(n7,  raycastDirection);
            float r8  = Float3.Dot(n8,  raycastDirection);
            float r9  = Float3.Dot(n9,  raycastDirection);
            float r10 = Float3.Dot(n10, raycastDirection);
            float r11 = Float3.Dot(n11, raycastDirection);
            float r12 = Float3.Dot(n12, raycastDirection);

            /*
            Program.Log($"""
                Results:
                    r1  = '{0f.CompareTo(r1)}'
                    r2  = '{0f.CompareTo(r2)}'
                    r3  = '{0f.CompareTo(r3)}'
                    r4  = '{0f.CompareTo(r4)}'
                    r5  = '{0f.CompareTo(r5)}'
                    r6  = '{0f.CompareTo(r6)}'
                """);
            //*/

            List<BlockDirection> nextBlockDirections = new();

            float s = 0.00001f;

            // r1  - r2  - r3  - r4  - r5  - r6  - r7  - r8  - r9  - r10 - r11 - r12

            if (r1 >= -s && r2 >= -s && r3 >= -s && r4 >= -s)
            {
                nextBlockDirections.Add(BlockDirection.DOWN);
            }
            else if (r9 >= -s && r10 >= -s && r11 >= -s && r12 >= -s)
            {
                nextBlockDirections.Add(BlockDirection.UP);
            }

            // r1  - r2  - r3  - r4  - r5  - r6  - r7  - r8  - r9  - r10 - r11 - r12

            // Supposed to be WEST
            // Neg - Neg - Pos - Pos - Pos - Neg - Pos - Pos - Pos - Neg - Neg - Pos

            // Supposed to be EAST
            // Pos - Pos - Pos - Neg - Pos - Pos - Neg - Neg - Neg - Neg - Neg - Neg

            if (r4 <= s && r5 >= -s && r8 <= s && r12 <= s)
            {
                nextBlockDirections.Add(BlockDirection.EAST);
            }
            else if (r2 <= s && r6 <= s && r7 >= -s && r10 <= s)
            {
                nextBlockDirections.Add(BlockDirection.WEST);
            }

            // r1  - r2  - r3  - r4  - r5  - r6  - r7  - r8  - r9  - r10 - r11 - r12

            // Supposed to be SOUTH
            // Neg - Neg - Pos - Pos - Pos - Neg - Neg - Pos - Pos - Neg - Neg - Pos
            // Neg - Neg - Pos - Pos - Neg - Neg - Neg - Pos - Pos - Neg - Neg - Pos
            // Neg - Neg - Pos - Pos - Neg - Neg - Neg - Pos - Pos - Neg - Neg - Pos
            // Neg - Neg - Pos - Neg - Pos - Neg - Neg - Pos - Pos - Pos - Neg - Pos
            // Neg - Pos - Pos - Pos - Pos - Pos - Neg - Pos - Pos - Pos - Neg - Neg

            // NOT SOUTH
            // Neg - Neg - Pos - Pos - Neg - Neg - Pos - Pos - Pos - Neg - Neg - Pos
            // Neg - Neg - Pos - Neg - Pos - Neg - Neg - Pos - Pos - Pos - Pos - Pos
            // Neg - Pos - Pos - Neg - Pos - Pos - Neg - Pos - Pos - Pos - Pos - Pos
            // Neg - Neg - Pos - Neg - Pos - Neg - Neg - Pos - Pos - Pos - Pos - Pos

            if (r3 <= s && r5 <= s && r6 >= -s && r9 <= s)
            {
                nextBlockDirections.Add(BlockDirection.NORTH);
            }
            else if (r1 <= s && r7 <= s && r8 >= -s && r11 <= s)
            {
                nextBlockDirections.Add(BlockDirection.SOUTH);
            }


            /* DEBUGGING
            var Values = (r1 < 0 ? "Neg" : "Pos") + " - " +
                         (r2 < 0 ? "Neg" : "Pos") + " - " +
                         (r3 < 0 ? "Neg" : "Pos") + " - " +
                         (r4 < 0 ? "Neg" : "Pos") + " - " +
                         (r5 < 0 ? "Neg" : "Pos") + " - " +
                         (r6 < 0 ? "Neg" : "Pos") + " - " +
                         (r7 < 0 ? "Neg" : "Pos") + " - " +
                         (r8 < 0 ? "Neg" : "Pos") + " - " +
                         (r9 < 0 ? "Neg" : "Pos") + " - " +
                         (r10 < 0 ? "Neg" : "Pos") + " - " +
                         (r11 < 0 ? "Neg" : "Pos") + " - " +
                         (r12 < 0 ? "Neg" : "Pos");

            if (nextBlockDirections.Count == 0)
                Program.Log("NULL!");
            else
            {
                string message = "";

                foreach (var item in nextBlockDirections)
                {
                    message += item + " - ";
                }
                Program.Log(message);
            }
            */

            if (nextBlockDirections.Count == 0)
            {
                Program.LogError("No possible next directions! This should be impossible!");
                break;
            }

            BlockDirection nextBlockDirection = BlockDirection.NORTH;

            float closest = -10;

            // TODO: If possible marching directions is implemented, this may be redundant
            foreach (var direction in nextBlockDirections)
            {
                float dot = Float3.Dot(raycastDirection, new BlockFace(direction).GetNormal());

                if(dot > closest)
                {
                    closest = dot;
                    nextBlockDirection = direction;
                }
            }

            Float3 pointToMarchTo = Float3.zero;

            switch (nextBlockDirection)
            {
                case BlockDirection.UP:
                    pointToMarchTo = Maths.PlanePointIntersection(c, raycastDirection, Axis.Yaxis, 1) + blockPos;
                    blockPos += new Int3(0, 1, 0);
                    break;
                case BlockDirection.DOWN:
                    pointToMarchTo = Maths.PlanePointIntersection(c, raycastDirection, Axis.Yaxis) + blockPos;
                    blockPos += new Int3(0, -1, 0);
                    break;
                case BlockDirection.NORTH:
                    pointToMarchTo = Maths.PlanePointIntersection(c, raycastDirection, Axis.Zaxis, 1) + blockPos;
                    blockPos += new Int3(0, 0, 1);
                    break;
                case BlockDirection.SOUTH:
                    pointToMarchTo = Maths.PlanePointIntersection(c, raycastDirection, Axis.Zaxis) + blockPos;
                    blockPos += new Int3(0, 0, -1);
                    break;
                case BlockDirection.WEST:
                    pointToMarchTo = Maths.PlanePointIntersection(c, raycastDirection, Axis.Xaxis, 1) + blockPos;
                    blockPos += new Int3(1, 0, 0);
                    break;
                case BlockDirection.EAST:
                    pointToMarchTo = Maths.PlanePointIntersection(c, raycastDirection, Axis.Xaxis) + blockPos;
                    blockPos += new Int3(-1, 0, 0);
                    break;
            }

            float distance = (pointToMarchTo - positionMarched).Length;

            distanceTraveled += distance;

            if (distanceTraveled > maxRayDistance)
            {
                positionMarched = raycastPosition + raycastDirection * maxRayDistance;
                break;
            }

            positionMarched = pointToMarchTo;

            currentBlock = GetBlockAtPosition(blockPos);
            blockDirectionHit = BlockFace.Opposite(nextBlockDirection);
        }

        return new BlockRaycastHit(currentBlock, blockPos, new BlockFace(blockDirectionHit), positionMarched);
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

    [Command("SetBlock", "[#x, #y, #z, 'block_id']")]
    public static void SetBlock(int x, int y, int z, string block_id)
    {
        SetBlock(block_id, new(x, y, z));

        Program.Log("Set block to: " + block_id);
    }

    [Command("Fill", "[#x1, #y1, #z1, #x2, #y2, #z2, 'block_id']")]
    public static void FillBlocks(int x1, int y1, int z1, int x2, int y2, int z2, string block_id)
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

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
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
    }
}

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
            _connectedMesh.shader = WorldDataManager.meshShader;
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

                    var rightBlock  = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(-1, 0, 0));
                    var leftBlock   = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(1, 0, 0));
                    var bottomBlock = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, -1, 0));
                    var topBlock    = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 1, 0));
                    var backBlock   = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, -1));
                    var frontBlock  = WorldDataManager.GetBlockAtPosition(worldBlockPosition + new Int3(0, 0, 1));

                    bool renderRightFace  = rightBlock.block_id == "minecraft:air";
                    bool renderLeftFace   = leftBlock.block_id == "minecraft:air";
                    bool renderBottomFace = bottomBlock.block_id == "minecraft:air";
                    bool renderTopFace    = topBlock.block_id == "minecraft:air";
                    bool renderBackFace   = backBlock.block_id == "minecraft:air";
                    bool renderFrontFace  = frontBlock.block_id == "minecraft:air";


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
                                          TextureAtlasManager.GetTextureID(blockType.rightTexture)
                                          ));
                    }

                    if(renderLeftFace)
                    {
                        // Left Face

                        var vO = blockIndex * 8;

                        squareFaces.Add(($"f {8 + vO}/{1}/2 " +
                                          $"{5 + vO}/{2}/2 " +
                                          $"{6 + vO}/{3}/2 " +
                                          $"{7 + vO}/{4}/2",
                                          TextureAtlasManager.GetTextureID(blockType.leftTexture)
                                          ));
                    }

                    if(renderBottomFace)
                    {
                        // Bottom Face

                        var vO = blockIndex * 8;

                        squareFaces.Add(($"f {1 + vO}/{1}/3 " +
                                          $"{5 + vO}/{2}/3 " +
                                          $"{8 + vO}/{3}/3 " +
                                          $"{2 + vO}/{4}/3",
                                          TextureAtlasManager.GetTextureID(blockType.bottomTexture)
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
                                          TextureAtlasManager.GetTextureID(blockType.topTexture)
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
                                          TextureAtlasManager.GetTextureID(blockType.backTexture)
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
                                          TextureAtlasManager.GetTextureID(blockType.rightTexture)
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