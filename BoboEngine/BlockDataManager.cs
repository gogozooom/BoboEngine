public class BlockDataManager
{
    public Dictionary<string, BlockData> blockData { get; private set; } = new();

    public void GenerateBlockData()
    {
        blockData.Clear();

        blockData = new Dictionary<string, BlockData>
        {
            {
                "minecraft:dirt_block",
                new BlockData(
                "minecraft:dirt_block",
                "Dirt Block",
                "dirt")
            },
            {
                "minecraft:stone_block",
                new BlockData(
                "minecraft:stone_block",
                "Stone Block",
                "stone")
            },
        };
    }
}

public struct BlockData
{
    public string id = "minecraft:null";
    public string name = "NULL";

    public string rightTexture = "NULL";
    public string leftTexture = "NULL";
    public string topTexture = "NULL";
    public string bottomTexture = "NULL";
    public string backTexture = "NULL";
    public string frontTexture = "NULL";

    public BlockData(string id, string name, string rightTexture, string leftTexture, string topTexture, string bottomTexture, string backTexture, string frontTexture)
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

    public BlockData(string id, string name, string texture)
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