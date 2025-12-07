using BoboEngine;
using BoboEngine.Utils;

namespace Minecraft;

public class BlockType
{
    public readonly string id;
    public readonly string name;

    public BlockModel model;

    public BlockType()
    {
        id = "minecraft:null";
        name = "NULL";

        if (!nullData) GenerateNullData();

        model = BlockModel.LoadFromJson(nullData);
    }


    public BlockType(string id, string name, DynamicData data)
    {
        this.id = id;
        this.name = name;

        model = BlockModel.LoadFromJson(data);
    }

    private static DynamicData nullData;
    private static void GenerateNullData()
    {
        var data = "{{\"parent\": \"minecraft:block/cube_all\",\"textures\": {\"all\": \"minecraft:null\"}}";

        nullData = FileParser.ParseJson("NULL", data);

        MinecraftJsonManager.FullyPopulateData(ref nullData, "models/");
    }

    public bool CanCullSide(BlockFace face)
    {
        bool result = false;

        foreach (var element in model.elements)
        {
            if (element.from == Float3.zero && element.to == Float3.one)
                return true;
        }

        return result;
    }

    public override string ToString()
    {
        return id;
    }
}