using BoboEngine.Utils;
using System.Security.AccessControl;

namespace Minecraft;

public static class BlockModelManager
{
    public static Dictionary<string, BlockModel> blockModelData;

    public static Dictionary<string, DynamicData> rawDataResults;

    public static void LoadBlockModelData()
    {
        blockModelData = new();
        rawDataResults = new();

        var blockModelPath = Path.Combine(Program.ProgramDirectory, "Data\\assets\\minecraft\\models\\block");

        var r = FileParser.ReadFile(Path.Combine(blockModelPath, "custom_fence_inventory.json"));

        var r2 = r.ToJson();

        foreach (var file in Directory.GetFiles(blockModelPath, "*.json"))
        {
            var results = FileParser.ReadFile(Path.Combine(Program.ProgramDirectory, file));

            rawDataResults.Add(Path.GetFileNameWithoutExtension(file), results);
        }

        ReadRawDataResult("dirt");
    }

    public static DynamicData ReadRawDataResult(string id)
    {
        if (!rawDataResults.TryGetValue(id, out var result))
        {
            Program.LogError($"No raw block model data for id '{id}'!");
            return null;
        }

        return result;
    }
}

public class BlockModel
{

}