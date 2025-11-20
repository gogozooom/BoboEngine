
using HidSharp.Reports;
using StbImageSharp;

namespace BoboEngine.Utils;

public static class FileParser
{
    public static DynamicData ReadFile(string filePath)
    {
        string data = File.ReadAllText(filePath);

        return ParseJson(Path.GetFileNameWithoutExtension(filePath), data);
    }

    private static DynamicData ParseJson(string name, string data, DynamicData parent = null)
    {
        var result = new DynamicData(name, DataType.Array, parent, data);

        while (data.Length > 0)
        {
            // Looking For Item

            if (data.StartsWith("}") || data.StartsWith("]"))
            {
                break;
            }

            string itemId;

            if(result.GetDataType() != DataType.ValueArray && (data.StartsWith('{') || data.StartsWith('\"')))
            {
                var valueFound = data.Split("\"");

                itemId = valueFound[1];

                data = data.Remove(0, valueFound[0].Length + valueFound[1].Length + 2).TrimEnd().Remove(0, 1).TrimStart();
            }
            else
            {
                result.SetDataType(DataType.ValueArray);
                itemId = result.data.Count.ToString();

                if(data.StartsWith('[')) data = data.Remove(0, 1).TrimStart();
            }

            // Looking For Value

            DynamicData dataResult;
            string rawData;

            if (data.StartsWith("{"))
            {
                // Is List

                rawData = "";

                int openBrackets = 0;
                int closedBrackets = 0;
                foreach (var c in data)
                {
                    rawData += c;

                    if (c == '{') openBrackets++;
                    if (c == '}') closedBrackets++;

                    if(openBrackets != 0 && openBrackets == closedBrackets) break;
                }

                data = data.Remove(0, rawData.Length).Trim();

                if (data.StartsWith(',')) data = data.Remove(0, 1).TrimStart();

                dataResult = ParseJson(itemId, rawData, result);
            }
            else if (data.StartsWith("["))
            {
                // Number Array

                rawData = "";

                int openBrackets = 0;
                int closedBrackets = 0;
                foreach (var c in data)
                {
                    rawData += c;

                    if (c == '[') openBrackets++;
                    if (c == ']') closedBrackets++;

                    if (openBrackets != 0 && openBrackets == closedBrackets) break;
                }

                data = data.Remove(0, rawData.Length).Trim();

                dataResult = ParseJson(itemId, rawData, result);
            }
            else
            {
                // Is Data

                var value = data.Split(",")[0];

                rawData = value;
                if (rawData.EndsWith(']')) rawData = rawData.Split(']')[0];
                rawData = rawData.TrimEnd();

                data = data.Remove(0, value.Length).TrimStart();

                dataResult = new DynamicData(itemId, DataType.Value, result, rawData);
            }

            if (data.StartsWith(","))
                data = data.Remove(0, 1).TrimStart();

            result.PopulateData(dataResult);
        }
    
        return result;
    }
}

public class DynamicData
{
    public readonly string name;

    public readonly DynamicData parent;
    public readonly Dictionary<string, DynamicData> data = new();

    private string _rawData;
    private DataType _dataType;
    public void SetDataType(DataType dataType) => _dataType = dataType;

    public DynamicData(string name, DataType dataType = DataType.Unknown, DynamicData parent = null, string rawData = null)
    {
        this.name = name;
        this.parent = parent;
        _rawData = rawData;
        _dataType = dataType;
    }

    public DataType GetDataType() => _dataType;
    public T GetValue<T>()
    {
        if (_dataType == DataType.Unknown)
        {
            Program.LogWarning($"Cannot get value of an empty data list!");
            return default;
        }

        if (_dataType != DataType.Value)
        {
            Program.LogWarning($"Property of '{name}' is an array! Please use GetItem()!");
            return default;
        }

        switch (_dataType)
        {
            default:
                if (typeof(T) == typeof(string))
                    return (T)Convert.ChangeType(_rawData.Trim('"'), typeof(T));
                else break;
        }

        Program.LogError($"Type of '{name}' cannot be converted to type: '{typeof(T)}' as this data is of type: '{_dataType}'!");
        return default;
    }
    public DynamicData GetItem(string id)
    {
        if(data.Count == 0)
        {
            Program.LogWarning($"Property of '{name}' is not an array! Please use GetValue()!");
            return null;
        }

        if (!data.TryGetValue(id, out var result))
        {
            //Program.LogError($"Item '{id}' does not exist in property '{name}'!");
            return null;
        }

        return result;
    }
    public bool HasItem(string id)
    {
        if (data.Count == 0)
        {
            Program.LogWarning($"Property of '{name}' is not an array!");
        }

        return data.ContainsKey(id);
    }
    public void PopulateData(DynamicData d)
    {
        data.Add(d.name, d);
    }

    public string ToJson()
    {
        if (_dataType == DataType.Array)
        {
            string dataString = "";
            if (parent != null)
            {
                dataString += $"\"{name}\":";
            }
            dataString += "{\n";

            int i = 0;
            foreach (var item in data.Values)
            {
                dataString += item.ToJson();

                i++;

                if(i < data.Count) dataString += ",";
                dataString += "\n";
            }

            dataString += "}";
            return dataString;
        }
        else if(_dataType == DataType.ValueArray)
        {
            string dataString = $"\"{name}\": [ ";

            int i = 0;
            foreach (var item in data.Values)
            {
                dataString += item.ToJson();

                i++;

                if (i < data.Count) { dataString += ",";
                if (item.GetDataType() == DataType.Array) dataString += "";} // += "\n"

                dataString += " ";
            }

            dataString += "]";
            return dataString;
        }
        else
        {
            string dataString = "";

            if (parent && parent.GetDataType() != DataType.ValueArray) dataString += $"\"{name}\":";

            dataString += _rawData;

            return dataString;
        }
    }

    public override string ToString()
    {
        switch (_dataType)
        {
            case DataType.Value:
                return $"{name} = '{_rawData}')";
            case DataType.ValueArray:
                return $"{name}<{_dataType}>({data.Count})";
            case DataType.Array:
                return $"{name}<{_dataType}>({data.Count})";
            default:
                return $"!{name}<{_dataType}>";
        }
    }

    public static implicit operator bool(DynamicData d) => d.GetDataType() != DataType.Unknown;
}

public enum DataType
{
    Unknown,
    Value,
    ValueArray,
    Array
}