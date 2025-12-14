using BoboEngine;

namespace Minecraft;
public class Chunk : ObjectBehavior
{
    public Int3 chunkPosition { get; private set; }

    private WorldBlockData[,,] _blocks = new WorldBlockData[16, 16, 16];

    private bool _meshChanged = false;
    private bool _newMeshGenerated = false;
    private bool _generatingMesh = false;
    private int _meshGenerationTaskID = 0;

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

    public override void LateUpdate()
    {
        if (_newMeshGenerated)
        {
            _connectedMesh.DeleteMesh();
            _connectedMesh.LoadRawData(GenerateMesh_vertices.ToArray(), GenerateMesh_faces.ToArray(), GenerateMesh_normals.ToArray(), GenerateMesh_textureCoords.ToArray());
            GenerateMesh_vertices = new();
            GenerateMesh_faces = new();
            GenerateMesh_textureCoords = new();

            _newMeshGenerated = false;
        }

        if (_meshChanged)
        {
            _meshChanged = false;
            Task.Run(GenerateMesh);
        }
    }

    public void ClearMesh()
    {
        for (int x = 0; x < _blocks.GetLength(0); x++)
        {
            for (int y = 0; y < _blocks.GetLength(1); y++)
            {
                for (int z = 0; z < _blocks.GetLength(2); z++)
                {
                    _blocks[x, y, z] = new();
                }
            }
        }

        _meshChanged = true;
    }

    private Mesh _connectedMesh;

    private List<Float3> GenerateMesh_vertices;
    private List<FaceInfo> GenerateMesh_faces;
    private List<Float2> GenerateMesh_textureCoords;
    private List<Float3> GenerateMesh_normals = [
        BlockFace.GetNormal(BlockDirection.WEST),
            BlockFace.GetNormal(BlockDirection.EAST),
            BlockFace.GetNormal(BlockDirection.DOWN),
            BlockFace.GetNormal(BlockDirection.UP),
            BlockFace.GetNormal(BlockDirection.NORTH),
            BlockFace.GetNormal(BlockDirection.SOUTH),
            ];

    /// <summary>
    /// Updates the visual mesh
    /// </summary>
    private void GenerateMesh()
    {
        int taskID = ++_meshGenerationTaskID;
        float timeStart = Time.time;

        while (_generatingMesh)
        {
            if(Time.time - timeStart > 60)
            {
                Program.LogError($"Mesh generation time out! '{this}'");
                return;
            }
        }

        _generatingMesh = true;

        GenerateMesh_vertices = new();
        GenerateMesh_faces = new();
        GenerateMesh_textureCoords = new();

        int elementIndex = 0;

        for (int x = 0; x < _blocks.GetLength(0); x++)
        {
            for (int y = 0; y < _blocks.GetLength(1); y++)
            {
                for (int z = 0; z < _blocks.GetLength(2); z++)
                {
                    // Outdated Mesh Data Check
                    if (taskID != _meshGenerationTaskID)
                    {
                        _generatingMesh = false;
                        return;
                    }

                    WorldBlockData block = _blocks[x, y, z];

                    if (block.block_id == "minecraft:air" || block.block_id == "minecraft:void") continue;

                    Int3 localBlockPosition = new(x, y, z);
                    BaseBlock blockType = BlockTypeManager.GetBlockType(block.block_id);

                    blockType.GenerateMesh(ref elementIndex, LocalPositionToWorld(localBlockPosition), localBlockPosition, ref GenerateMesh_vertices, ref GenerateMesh_faces, ref GenerateMesh_textureCoords);
                }
            }
        }

        _generatingMesh = false;
        _newMeshGenerated = true;
    }

    public void ForceRegenerateChunk() => _meshChanged = true;

    public WorldBlockData GetBlockAtLocalPosition(Int3 localPosition)
    {
        if (!IsLocalPositionValid(localPosition))
        {
            Program.LogWarning("GetBlockAtPosition() Position out of range!");
            return new WorldBlockData();
        }

        return _blocks[localPosition.x, localPosition.y, localPosition.z];
    }

    public void SetBlockAtLocalPosition(Int3 localPosition, WorldBlockData block)
    {
        if (!IsLocalPositionValid(localPosition))
        {
            Program.LogWarning("SetBlockAtLocalPosition() Cannot set block! Out of bounds!");
            return;
        }

        _blocks[localPosition.x, localPosition.y, localPosition.z] = block;
        _meshChanged = true;

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

            chunk._meshChanged = true;
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