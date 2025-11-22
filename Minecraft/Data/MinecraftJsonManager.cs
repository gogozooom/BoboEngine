using BoboEngine.Utils;

namespace Minecraft;

public static class MinecraftJsonManager
{
    public static Dictionary<string, DynamicData> rawDataResults;

    public static void LoadBlockModelData()
    {
        rawDataResults = new();

        var assetsPath = Path.Combine(Program.ProgramDirectory, "Data\\assets\\minecraft");
        
        if (!Directory.Exists(assetsPath))
        {
            Program.LogError("No minecraft data supported! Please put minecraft data named as 'Data' in executable directory!");
            return;
        }

        var files = Directory.GetFiles(assetsPath, "*.json", SearchOption.AllDirectories);

        int t = files.Length;

        foreach (var file in files)
        {
            var fileName = file.Remove(0, assetsPath.Length + 1).Split('.')[0].Replace('\\', '/');
            var type = Path.GetExtension(file);

            if (IgnoreFile(file.Remove(0, assetsPath.Length + 1)))
            {
                Program.Log($"Ignoring '{fileName}' for the moment...");
                continue;
            }

            var data = File.ReadAllText(file);

            var results = FileParser.ParseJson(fileName, data, false);

            rawDataResults.Add(fileName, results);

            Program.LogMessage($"Loaded ({rawDataResults.Count}/{t}) --- '{fileName}{type}'");
        }
    }

    public static DynamicData GetData(string id)
    {
        if (!rawDataResults.TryGetValue(id, out var result))
        {
            Program.LogError($"No raw block model data for id '{id}'!");
            return null;
        }

        return result;
    }

    public static DynamicData FullyPopulateData(ref DynamicData data, string context = "")
    {
        while (data.HasItem("parent"))
        {
            var parentV = context + data.GetItem("parent").GetValue<string>().Split(':')[^1];

            var parentD = GetData(parentV);

            if (!parentD)
            {
                Program.LogWarning($"Parent '{parentV}' does not exist!");
                return data;
            }

            data.Merge(parentD);

            if (!parentD.HasItem("parent")) break;
        }

        data.RemoveItem("parent");

        PopulateReferences(ref data);

        return data;
    }

    public static DynamicData PopulateReferences(ref DynamicData data)
    {
        List<DynamicData> allValues = data.GetAllValues();

        Dictionary<string, DynamicData> independentValues = new();
        List<DynamicData> dependentValues = new();

        foreach (var value in allValues)
        {
            if (value.GetValue<string>().Contains("#"))
            {
                dependentValues.Add(value);
            }
            else
            {
                if (independentValues.ContainsKey(value.name))
                {
                    //Program.LogWarning($"Duplicate values '{value.name}'");
                    continue;
                }

                independentValues.Add(value.name, value);
            }
        }

        bool success = false;

        foreach (var value in dependentValues)
        {
            var indepS = value.GetValue<string>().Remove(0, 1); // Remove #

            if(!independentValues.TryGetValue(indepS, out var indepD))
            {
                //Program.LogWarning($"Unable to populate '#{indepS}' missing independent variable!");
                continue;
            }

            success = true;
            value.SetValue(indepD.GetValue<string>());
        }

        if (!success) return data;

        // Repeat
        return PopulateReferences(ref data);
    }

    private static bool IgnoreFile(string file)
    {
        return file.StartsWith("lang") 
            || file.StartsWith("shaders")
            || file.StartsWith("post_effect")
            || file.StartsWith("texts")
            || file.StartsWith("font")
            || file.StartsWith("waypoint_style");
    }
}