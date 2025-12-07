using BoboEngine;
using BoboEngine.Shaders;
using ConsoleCommand;

namespace Minecraft;
public static class WorldDataManager
{
    public static Material meshMaterial;

    public static Dictionary<Int3, Chunk> chunks = new();

    public static void GenerateTestChunk()
    {
        if (meshMaterial == null)
        {
            string shaderID = "worldShader";
            ShaderManager.EnsureShader(shaderID, "Shader/worldShader.vert", "Shader/worldShader.frag");

            meshMaterial = new Material(shaderID, TextureManager.blockTextureArray);
        }

        var chunk = CreateChunkObject(new(0,0,0));

        int x = 0;
        int z = 0;
        float rowCount = MathF.Sqrt(BlockTypeManager.blockData.Count);

        foreach (var block in BlockTypeManager.blockData)
        {
            SetBlock(new(x * 2, 0, z * 2), new(block.Key));

            x++;

            if(x >= rowCount)
            {
                z++;
                x = 0;
            }
        }

        FillBlocks(new(-25, -1, -25), new(25, -1, 25), "minecraft:grass_block");
        FillBlocks(new(-25, -10, -25), new(25, -2, 25), "minecraft:dirt");
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
    public static WorldBlockData GetBlockAtPosition(Int3 position)
    {
        var chunk = GetChunkInPosition(position);

        var localPosition = chunk.WorldPositionToLocal(position);

        return chunk.GetBlockAtLocalPosition(localPosition);
    }
    public static WorldBlockData GetBlockAtPosition(Float3 position) => GetBlockAtPosition(Float3ToBlockPos(position));
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

        WorldBlockData currentBlock = GetBlockAtPosition(blockPos);

        while (currentBlock.block_id == "minecraft:air")
        {
            // Point Marched Unit Square
            var c = positionMarched - blockPos;

            // Unit Square Points
            Float3 v1 = new(0, 0, 0); // Down - North - West
            Float3 v2 = new(1, 0, 0); // Down - North - East
            Float3 v3 = new(1, 0, 1); // Down - South - East
            Float3 v4 = new(0, 0, 1); // Down - South - West
            Float3 v5 = new(0, 1, 1); // Up   - South - West
            Float3 v6 = new(1, 1, 1); // Up   - South - East
            Float3 v7 = new(1, 1, 0); // Up   - North - East
            Float3 v8 = new(0, 1, 0); // Up   - North - West

            // Edge Normals
            Float3 n1 = Float3.CrossProduct(v1 - c, v2 - c); // Down  - South
            Float3 n2 = Float3.CrossProduct(v2 - c, v3 - c); // Down  - East
            Float3 n3 = Float3.CrossProduct(v3 - c, v4 - c); // Down  - North
            Float3 n4 = Float3.CrossProduct(v4 - c, v1 - c); // Down  - West

            Float3 n5 = Float3.CrossProduct(v4 - c, v5 - c); // South - West
            Float3 n6 = Float3.CrossProduct(v3 - c, v6 - c); // South - East
            Float3 n7 = Float3.CrossProduct(v2 - c, v7 - c); // North - East
            Float3 n8 = Float3.CrossProduct(v1 - c, v8 - c); // North - West

            Float3 n9 = Float3.CrossProduct(v5 - c, v6 - c); // Up    - South
            Float3 n10 = Float3.CrossProduct(v6 - c, v7 - c); // Up    - East
            Float3 n11 = Float3.CrossProduct(v7 - c, v8 - c); // Up    - North
            Float3 n12 = Float3.CrossProduct(v8 - c, v5 - c); // Up    - West

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

            if (r4 <= s && r5 >= -s && r8 <= s && r12 <= s)
            {
                nextBlockDirections.Add(BlockDirection.WEST);
            }
            else if (r2 <= s && r6 <= s && r7 >= -s && r10 <= s)
            {
                nextBlockDirections.Add(BlockDirection.EAST);
            }

            // r1  - r2  - r3  - r4  - r5  - r6  - r7  - r8  - r9  - r10 - r11 - r12

            if (r3 <= s && r5 <= s && r6 >= -s && r9 <= s)
            {
                nextBlockDirections.Add(BlockDirection.SOUTH);
            }
            else if (r1 <= s && r7 <= s && r8 >= -s && r11 <= s)
            {
                nextBlockDirections.Add(BlockDirection.NORTH);
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
                case BlockDirection.SOUTH:
                    pointToMarchTo = Maths.PlanePointIntersection(c, raycastDirection, Axis.Zaxis, 1) + blockPos;
                    blockPos += new Int3(0, 0, 1);
                    break;
                case BlockDirection.NORTH:
                    pointToMarchTo = Maths.PlanePointIntersection(c, raycastDirection, Axis.Zaxis) + blockPos;
                    blockPos += new Int3(0, 0, -1);
                    break;
                case BlockDirection.EAST:
                    pointToMarchTo = Maths.PlanePointIntersection(c, raycastDirection, Axis.Xaxis, 1) + blockPos;
                    blockPos += new Int3(1, 0, 0);
                    break;
                case BlockDirection.WEST:
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

    public static void SetBlock(Int3 position, string block_id)
    {
        var chunk = GetChunkInPosition(position);

        if(chunk == null)
        {
            Program.Log("Chunk not loaded yet!");
            return;
        }

        chunk.SetBlockAtLocalPosition(chunk.WorldPositionToLocal(position), new(block_id));
    }

    public static void FillBlocks(Int3 p1, Int3 p2, string block_id)
    {
        int minX = p1.x;
        int maxX = p2.x;

        if (p1.x > p2.x)
        {
            minX = p2.x;
            maxX = p1.x;
        }

        int minY = p1.y;
        int maxY = p2.y;

        if (p1.y > p2.y)
        {
            minY = p2.y;
            maxY = p1.y;
        }

        int minZ = p1.z;
        int maxZ = p2.z;

        if (p1.z > p2.z)
        {
            minZ = p2.z;
            maxZ = p1.z;
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

    [Command("SetBlock", "[#x, #y, #z, 'block_id']")]
    public static void SetBlock(int x, int y, int z, string block_id)
    {
        if (!block_id.Contains(':')) block_id = "minecraft:" + block_id;

        SetBlock(new(x, y, z), block_id);

        Program.Log("Set block to: " + block_id);
    }

    [Command("Fill", "[#x1, #y1, #z1, #x2, #y2, #z2, 'block_id']")]
    public static void FillBlocks(int x1, int y1, int z1, int x2, int y2, int z2, string block_id)
    {
        if (!block_id.Contains(':')) block_id = "minecraft:" + block_id;

        FillBlocks(new Int3(x1, y1, z1), new Int3(x2, y2, z2), block_id);
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