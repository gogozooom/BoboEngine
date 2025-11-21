using BoboEngine.Utils;

namespace Minecraft;
public static class BlockTypeManager
{
    public static Dictionary<string, BlockType> blockData { get; private set; } = new();

    public static void GenerateBlockData() // TODO: Move to json files
    {
        blockData.Clear();

        AddBlockData("minecraft:null");

        string[] testBlocks = [
            "models/block/dirt",
            "models/block/grass_block",
            "models/block/stone",
            "models/block/crafting_table",
            "models/block/oak_log",
            "models/block/oak_planks",
            "models/block/orange_wool",
            ];

        foreach (var blockV in testBlocks)
        {
            var d = MinecraftJsonManager.GetData(blockV);

            if (!d)
            {
                Program.LogWarning($"Could not find: '{blockV}'!");
                return;
            }

            var block = MinecraftJsonManager.GetFullyPopulatedData(d, "models/");

            AddBlockData(block);
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

    private static void AddBlockData(DynamicData data)
    {
        var textures = data.GetItem("textures");

        if(!textures) return;

        var upD = textures.GetItem("up");
        var downD = textures.GetItem("down");

        var topD = textures.GetItem("top");
        var bottomD = textures.GetItem("bottom");

        var northD = textures.GetItem("north");
        var eastD = textures.GetItem("east");
        var southD = textures.GetItem("south");
        var westD = textures.GetItem("west");

        var sideD = textures.GetItem("side");

        var upS = "";
        var downS = "";
        var northS = "";
        var eastS = "";
        var southS = "";
        var westS = "";

        if (upD)
        {
            upS = upD.GetValue<string>().Split(':')[^1];
            downS = downD.GetValue<string>().Split(':')[^1];
        }
        else
        {
            upS = topD.GetValue<string>().Split(':')[^1];
            downS = bottomD.GetValue<string>().Split(':')[^1];
        }

        if (sideD)
        {
            var sideS = sideD.GetValue<string>().Split(':')[^1];

            northS = sideS;
            eastS = sideS;
            southS = sideS;
            westS = sideS;
        }
        else
        {
            northS = northD.GetValue<string>().Split(':')[^1];
            eastS = eastD.GetValue<string>().Split(':')[^1];
            southS = southD.GetValue<string>().Split(':')[^1];
            westS = westD.GetValue<string>().Split(':')[^1];
        }



        var name = data.name.Split('/')[^1];

        AddBlockData("minecraft:"+name, name, eastS, westS, upS, downS, southS, northS);
    }
    private static void AddBlockData(string id) => blockData.Add(id, new BlockType());
    private static void AddBlockData(string id, string name, string textureID) => blockData.Add(id, new BlockType(id, name, textureID));
    private static void AddBlockData(string id, string name, string eastTexture, string westTexture, string upTexture, string downTexture, string southTexture, string northFolder) => blockData.Add(id, new BlockType(id, name, eastTexture, westTexture, upTexture, downTexture, southTexture, northFolder));
}

public struct BlockType
{
    public readonly string id;
    public readonly string name;

    public readonly string eastTexture;
    public readonly string westTexture;
    public readonly string upTexture;
    public readonly string downTexture;
    public readonly string southTexture;
    public readonly string northTexture;

    public BlockType()
    {
        id = "minecraft:null";
        name = "NULL";

        eastTexture = "NULL";
        westTexture = "NULL";
        upTexture = "NULL";
        downTexture = "NULL";
        southTexture = "NULL";
        northTexture = "NULL";
    }
    public BlockType(string id, string name, string rightTexture, string leftTexture, string topTexture, string bottomTexture, string backTexture, string frontTexture)
    {
        this.id = id;
        this.name = name;
        this.eastTexture = rightTexture;
        this.westTexture = leftTexture;
        this.upTexture = topTexture;
        this.downTexture = bottomTexture;
        this.southTexture = backTexture;
        this.northTexture = frontTexture;
    }

    public BlockType(string id, string name, string texture)
    {
        this.id = id;
        this.name = name;
        eastTexture = texture;
        westTexture = texture;
        upTexture = texture;
        downTexture = texture;
        southTexture = texture;
        northTexture = texture;
    }
}