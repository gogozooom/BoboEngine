using BoboEngine.Utils;

namespace Minecraft;
public static class BlockTypeManager
{
    public static Dictionary<string, BlockType> blockData { get; private set; } = new();

    public static void GenerateBlockData() // TODO: Move to json files
    {
        blockData.Clear();

        blockData.Add("minecraft:null", new());

        string[] testBlocks = [
            "models/block/dirt",
            "models/block/grass_block",
            "models/block/stone",
            "models/block/crafting_table",
            "models/block/oak_log",
            "models/block/oak_planks",
            "models/block/orange_wool",
            "models/block/oak_stairs",
            "models/block/hopper",
            "models/block/anvil",
            "models/block/rose_bush_bottom",
            "models/block/dandelion",
            "models/block/four_sea_pickles",
            ];

        foreach (var blockV in testBlocks)
        {
            var data = MinecraftJsonManager.GetData(blockV);

            if (!data)
            {
                Program.LogWarning($"Could not find: '{blockV}'!");
                continue;
            }

            AddBlockData(data);
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
        MinecraftJsonManager.FullyPopulateData(ref data, "models/");

        var name = data.name.Split('/')[^1];

        var id = "minecraft:" + name;

        blockData.Add(id, new(id, name, data));
    }
}

public struct BlockType
{
    public readonly string id;
    public readonly string name;

    public readonly DynamicData modelData;

    public static DynamicData nullData;

    public BlockType()
    {
        id = "minecraft:null";
        name = "NULL";

        if (!nullData) GenerateNullData();

        modelData = nullData;
    }


    public BlockType(string id, string name, DynamicData data)
    {
        this.id = id;
        this.name = name;
        this.modelData = data;
    }

    private static void GenerateNullData()
    {
        var data = "{{\"parent\": \"minecraft:block/cube_all\",\"textures\": {\"all\": \"minecraft:null\"}}";

        nullData = FileParser.ParseJson("NULL", data);

        MinecraftJsonManager.FullyPopulateData(ref nullData, "models/");
    }
}