namespace Minecraft;
public static class BlockTypeManager
{
    public static Dictionary<string, BlockType> blockData { get; private set; } = new();

    public static void GenerateBlockData() // TODO: Move to json files
    {
        blockData.Clear();

        AddBlockData("minecraft:null");

        foreach (var item in TextureManager.blockTextureIds)
        {
            AddBlockData("minecraft:" + item.Key, item.Key, item.Key);
        }

        /*
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
            "minecraft:cobblestone",
            "Cobblestone",

            "cobblestone"
            );

        AddBlockData(
            "minecraft:iron_ore",
            "Iron Ore",

            "iron_ore"
            );

        AddBlockData(
            "minecraft:oak_planks",
            "Oak Planks",

            "oak_planks"
            );


        AddBlockData(
            "minecraft:oak_log",
            "Oak Log",

            "oak_log",
            "oak_log",
            "oak_log_top",
            "oak_log_top",
            "oak_log",
            "oak_log"
            );

        AddBlockData(
            "minecraft:grass_block",
            "Grass Block",

            "grass_block_side",
            "grass_block_side",
            "grass_block_top",
            "dirt",
            "grass_block_side",
            "grass_block_side"
            );

        AddBlockData(
            "minecraft:crafting_table",
            "Crafting Table",

            "crafting_table_side",
            "crafting_table_side",
            "crafting_table_top",
            "crafting_table_top",
            "crafting_table_front",
            "crafting_table_front"
            );
        */
    }
    
    public static BlockType GetBlockData(string id)
    {
        if (id != null && blockData.ContainsKey(id))
        {
            return blockData[id];
        }

        return blockData["minecraft:null"];
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