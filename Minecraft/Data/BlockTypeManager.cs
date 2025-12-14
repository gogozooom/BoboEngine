using BoboEngine.Utils;
using Minecraft.Blocks;

namespace Minecraft;
public static class BlockTypeManager
{
    public static Dictionary<string, BaseBlock> blockData { get; private set; } = new();

    public static void GenerateBlockData()
    {
        blockData.Clear();

        BaseBlock[] blocks = [ // TODO: Move to json files
            new CubeBlock(), // Null
            new CubeBlock("minecraft:air", "Air", MinecraftJsonManager.GetData("models/block/air")),
            new CubeBlock("minecraft:dirt", "Dirt", MinecraftJsonManager.GetData("models/block/dirt")),
            new CubeBlock("minecraft:grass_block", "Grass Block", MinecraftJsonManager.GetData("models/block/grass_block")),
            new CubeBlock("minecraft:wildflowers", "Wild Flowers", MinecraftJsonManager.GetData("models/block/wildflowers_1")),
            new TransparentBlock("minecraft:orange_stained_glass", "Orange Stained Glass", MinecraftJsonManager.GetData("models/block/orange_stained_glass")),
            new TransparentBlock("minecraft:pink_stained_glass", "Pink Stained Glass", MinecraftJsonManager.GetData("models/block/pink_stained_glass")),
            new TransparentBlock("minecraft:green_stained_glass", "Green Stained Glass", MinecraftJsonManager.GetData("models/block/green_stained_glass")),
            new TransparentBlock("minecraft:yellow_stained_glass", "Yellow Stained Glass", MinecraftJsonManager.GetData("models/block/yellow_stained_glass"))
            ];

        foreach (var block in blocks)
        {
            blockData.Add(block.id, block);
        }

        /* Old Method

        blockData.Add("minecraft:null", new CubeBlock());

        string[] watchList = [
            // Normal Blocks
            "models/block/air",
            "models/block/dirt",
            "models/block/grass_block",
            "models/block/rose_bush_bottom",
            "models/block/rose_bush_top",
            "models/block/anvil",
            "models/block/crafting_table",

            "models/block/oak_button",
            "models/block/oak_button_inventory",
            
            // Broken Blocks
            "models/block/oak_log_horizontal", // Not horizontal?
            "models/block/lantern", // No Textures?

            // Fix animated textures
            "models/block/magma_block",
            "models/block/command_block",

            // Fix textures bigger than 16x16
            "models/block/cherry_shelf_inventory",

            // Better Transparency Support
            "models/block/orange_stained_glass",
            "models/block/pink_stained_glass",
            "models/block/green_stained_glass",
            "models/block/yellow_stained_glass",
            
            ];

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
        */
    }

    private static void AddBlockData(DynamicData data)
    {
        MinecraftJsonManager.FullyPopulateData(ref data, "models/");

        var name = data.name.Split('/')[^1];

        var id = "minecraft:" + name;

        blockData.Add(id, new CubeBlock(id, name, data));
    }
    public static BaseBlock GetBlockType(string id)
    {
        if (id != null && blockData.ContainsKey(id))
        {
            return blockData[id];
        }

        return blockData["minecraft:null"];
    }
}