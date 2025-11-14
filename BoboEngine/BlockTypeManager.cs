namespace BoboEngine;
public static class BlockTypeManager
{
    public static Dictionary<string, BlockType> blockData { get; private set; } = new();

    public static BlockType GetBlockData(string id)
    {
        if (blockData.ContainsKey(id))
        {
            return blockData[id];
        }

        return blockData["minecraft:null"];
    }
    public static void GenerateBlockData() // TODO: Move to json files
    {
        blockData.Clear();

        AddBlockData("minecraft:null");

        AddBlockData(
            "minecraft:dirt",
            "Dirt",

            "dirt"
            );

        AddBlockData(
            "minecraft:stone",
            "Stone",

            "stone"
            );

        AddBlockData(
            "minecraft:grass_block",
            "Grass Block",

            "grass_block",
            "grass_block",
            "grass_block_top",
            "dirt",
            "grass_block",
            "grass_block"
            );
    }

    private static void AddBlockData(string id) => blockData.Add(id, new BlockType());
    private static void AddBlockData(string id, string name, string textureID) => blockData.Add(id, new BlockType(id, name, textureID));
    private static void AddBlockData(string id, string name, string rightTexture, string leftTexture, string topTexture, string bottomTexture, string backTexture, string frontTexture) => blockData.Add(id, new BlockType(id, name, rightTexture, leftTexture, topTexture, bottomTexture, backTexture, frontTexture));
}

public struct BlockType
{
    public readonly string id;
    public readonly string name;

    public readonly string rightTexture;
    public readonly string leftTexture;
    public readonly string topTexture;
    public readonly string bottomTexture;
    public readonly string backTexture;
    public readonly string frontTexture;

    public BlockType()
    {
        id = "minecraft:null";
        name = "NULL";

        rightTexture = "NULL";
        leftTexture = "NULL";
        topTexture = "NULL";
        bottomTexture = "NULL";
        backTexture = "NULL";
        frontTexture = "NULL";
    }
    public BlockType(string id, string name, string rightTexture, string leftTexture, string topTexture, string bottomTexture, string backTexture, string frontTexture)
    {
        this.id = id;
        this.name = name;
        this.rightTexture = rightTexture;
        this.leftTexture = leftTexture;
        this.topTexture = topTexture;
        this.bottomTexture = bottomTexture;
        this.backTexture = backTexture;
        this.frontTexture = frontTexture;
    }

    public BlockType(string id, string name, string texture)
    {
        this.id = id;
        this.name = name;
        rightTexture = texture;
        leftTexture = texture;
        topTexture = texture;
        bottomTexture = texture;
        backTexture = texture;
        frontTexture = texture;
    }
}