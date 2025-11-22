using BoboEngine.Utils;

namespace Minecraft;
public static class BlockTypeManager
{
    public static Dictionary<string, BlockType> blockData { get; private set; } = new();

    public static void GenerateBlockData() // TODO: Move to json files
    {
        blockData.Clear();

        blockData.Add("minecraft:null", new());

        string[] watchList = [
            // Normal Blocks
            "models/block/dirt",
            "models/block/grass_block",

            // Broken Blocks
            "models/block/oak_log_horizontal", // Not horizontal?
            "models/block/lantern", // No Textures?
            "models/block/wildflowers_1", // Stem weird offset?

            // Fix animated textures
            "models/block/magma_block",
            "models/block/command_block",

            // Fix textures bigger than 16x16
            "models/block/cherry_shelf_inventory",

            "models/block/slime_block", // Incorrect rendering order?

            // Better Transparency Support
            "models/block/orange_stained_glass",
            "models/block/pink_stained_glass",
            "models/block/green_stained_glass",
            "models/block/yellow_stained_glass",
            ];

        /* Test Blocks
        foreach (var blockV in watchList)
        {
            var data = MinecraftJsonManager.GetData(blockV);

            if (!data)
            {
                Program.LogWarning($"Could not find: '{blockV}'!");
                continue;
            }

            AddBlockData(data);
        }
        /*/

        //* All Blocks
        foreach (var data in MinecraftJsonManager.allData.Values)
        {
            if(data.name.StartsWith("models/block"))

            AddBlockData(data);
        }
        //*/

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