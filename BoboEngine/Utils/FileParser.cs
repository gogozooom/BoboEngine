using Microsoft.Win32.SafeHandles;

namespace BoboEngine.Utils;

public static class FileParser
{
    public static DynamicData ParseJson(string name, string data, bool includeRedundantRawData = true)
    {
        if(data == "{}")
        {
            Program.LogWarning($"Tried to load {name} which is empty!");

            return new DynamicData(name, DataType.Array, data);
        }

        var result = new DynamicData(name, DataType.Array, includeRedundantRawData ? data : null);

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

                dataResult = ParseJson(itemId, rawData);
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

                dataResult = ParseJson(itemId, rawData);
            }
            else
            {
                // Is Data

                var value = data.Split(",")[0];

                rawData = value;
                if (rawData.EndsWith(']')) rawData = rawData.Split(']')[0];
                if (rawData.EndsWith('}')) rawData = rawData.Split('}')[0];
                rawData = rawData.TrimEnd();

                data = data.Remove(0, value.Length).TrimStart();

                dataResult = new DynamicData(itemId, DataType.Value, rawData);
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
    public DynamicData parent;
    public readonly string name;

    public readonly Dictionary<string, DynamicData> data = new();

    /// <summary>
    /// Can either be the value when "<see cref="_dataType"/>" is equal to "DataType.Value" <br/>
    /// Or the full json data used to initialize the class in the first place
    /// </summary>
    private string _rawData;
    private DataType _dataType;
    public void SetDataType(DataType dataType) => _dataType = dataType;

    public DynamicData(string name, DataType dataType = DataType.Unknown, string rawData = null, DynamicData parent = null)
    {
        this.name = name;
        this.parent = parent;
        _rawData = rawData;
        _dataType = dataType;

        if (dataType == DataType.Value && rawData == null) Program.LogWarning("DynamicData created as a value with no data!");
    }

    public DataType GetDataType() => _dataType;
    public T GetValue<T>()
    {
        if (_dataType == DataType.Unknown)
        {
            Program.LogWarning($"Cannot get value of an empty data list!");
            return default;
        }

        if (_dataType == DataType.Array)
        {
            Program.LogWarning($"Property of '{name}' is an array! Please use GetItem()!");
            return default;
        }

        if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(_rawData.Trim('"'), typeof(string));

        if (typeof(T) == typeof(bool))
        {
            if (_dataType != DataType.Value)
            {
                Program.LogError($"Type of '{name}' is not a Value! Cannot convert to '{typeof(bool)}'!");
                return default;
            }

            switch (_rawData)
            {
                case "false":
                    return (T)Convert.ChangeType(false, typeof(bool));

                case "true":
                    return (T)Convert.ChangeType(true, typeof(bool));

                default:
                    Program.LogError($"Could not parse bool out of '{_rawData}' from '{name}'!");
                    return default;
            }
        }

        if (typeof(T) == typeof(float))
        {
            if (_dataType != DataType.Value)
            {
                Program.LogError($"Type of '{name}' is not a Value! Cannot convert to '{typeof(float)}'!");
                return default;
            }

            if(!float.TryParse(_rawData, out var value))
            {
                Program.LogError($"Could not parse float out of '{_rawData}' from '{name}'!");
                return default;
            }

            return (T)Convert.ChangeType(value, typeof(float));
        }

        if (typeof(T) == typeof(int))
        {
            if (_dataType != DataType.Value)
            {
                Program.LogError($"Type of '{name}' is not a Value! Cannot convert to '{typeof(int)}'!");
                return default;
            }

            if (!int.TryParse(_rawData, out var value))
            {
                Program.LogError($"Could not parse int out of '{_rawData}' from '{name}'!");
                return default;
            }

            return (T)Convert.ChangeType(value, typeof(int));
        }

        if (typeof(T) == typeof(Float3))
        {
            if(_dataType != DataType.ValueArray)
            {
                Program.LogError($"Type of '{name}' is not a ValueArray! Cannot convert to '{typeof(Float3)}'!");
                return default;
            }

            float x = 0;
            float y = 0;
            float z = 0;

            var xD = GetItem("0");
            var yD = GetItem("1");
            var zD = GetItem("2");

            if (xD) x = xD.GetValue<float>();
            if (yD) y = yD.GetValue<float>();
            if (zD) z = zD.GetValue<float>();

            return (T)Convert.ChangeType(new Float3(x, y, z), typeof(Float3));
        }

        if (typeof(T) == typeof(UVRect))
        {
            if (_dataType != DataType.ValueArray)
            {
                Program.LogError($"Type of '{name}' is not a ValueArray! Cannot convert to '{typeof(Float3)}'!");
                return default;
            }

            float uMin = 0;
            float vMin = 0;
            float uMax = 0;
            float vMax = 0;

            var uMD = GetItem("0");
            var vMD = GetItem("1");
            var uXD = GetItem("2");
            var vXD = GetItem("3");

            if (uMD) uMin = uMD.GetValue<float>();
            if (vMD) vMin = vMD.GetValue<float>();
            if (uXD) uMax = uXD.GetValue<float>();
            if (vXD) vMax = vXD.GetValue<float>();

            return (T)Convert.ChangeType(new UVRect(uMin, vMin, uMax, vMax), typeof(UVRect));
        }

        if (typeof(T) == typeof(Axis))
        {
            if (_dataType != DataType.Value)
            {
                Program.LogError($"Type of '{name}' is not a Value! Cannot convert to '{typeof(Axis)}'!");
                return default;
            }



            switch (_rawData.Trim('"'))
            {
                case "x":
                    return (T)Convert.ChangeType(Axis.Xaxis, typeof(Axis));
                case "y":
                    return (T)Convert.ChangeType(Axis.Yaxis, typeof(Axis));
                case "z":
                    return (T)Convert.ChangeType(Axis.Zaxis, typeof(Axis));
                default:
                    Program.LogError($"Could not parse axis out of '{_rawData}' from '{name}'!");
                    return default;
            }
        }

        Program.LogError($"Type of '{name}' cannot be converted to type: '{typeof(T)}' as this data is of type: '{_dataType}'!");
        return default;
    }
    public void SetValue(string value)
    {
        if (_dataType == DataType.Unknown)
        {
            Program.LogWarning($"Cannot set value of an empty data list!");
            return;
        }

        if (_dataType != DataType.Value)
        {
            Program.LogWarning($"Property of '{name}' is an array! Cannot set value!");
            return;
        }

        if (ValueIllegal(value))
        {
            Program.LogWarning($"Value '{value}' contains illegal characters!");
            return;
        }

        if (!value.StartsWith('"')) value = '"' + value;
        if (!value.EndsWith('"')) value += '"';

        _rawData = value;
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
    public List<DynamicData> GetAllValues()
    {
        List<DynamicData> result = new();

        GetAllItems(this, ref result);

        return result;
    }
    public DynamicData[] GetImmediateChildren() => data.Values.ToArray();
    public void RemoveItem(string id)
    {
        if (!HasItem(id))
        {
            //Program.LogWarning($"'{this}' does not contain '{id}'!");
            return;
        }

        data.Remove(id);
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
        d.parent = this;
    }
    public void Merge(DynamicData d)
    {
        var data = d.Copy();

        if(_dataType != DataType.Array)
        {
            Program.LogWarning("Cannot Merge value data type!");
            return;
        }

        foreach (var item in data.data.Values)
        {
            if (!HasItem(item.name))
            {
                PopulateData(item);
                continue;
            }

            // CONFLICT!!

            // Held by parent
            var conflictingItem = GetItem(item.name);

            // Held by merging child
            var targetDataType = item.GetDataType();

            if (conflictingItem.GetDataType() != targetDataType)
            {
                Program.LogWarning($"Item property '{item.name}' has a type miss-match! '{targetDataType}' != '{conflictingItem.GetDataType()}'!");
                continue;
            }

            switch (targetDataType)
            {
                case DataType.Value:

                    conflictingItem.SetValue(item.GetValue<string>());
                    break;

                case DataType.ValueArray:

                    // Split between adding and replacing with:
                    // "array" : [ {...}, {...} ]
                    // and:
                    // "pos"   : [ 0, 2, 1 ]
                    // ...

                    //Program.LogWarning($"Target type of '{targetDataType}' not implemented!");

                    break;
                case DataType.Array:
                    foreach (var v in item.data.Values)
                    {
                        conflictingItem.Merge(v);
                    }

                    break;
                default:
                    Program.LogWarning($"Cannot merge item '{item}' continuing...");
                    continue;
            }
        }
    }

    public DynamicData Copy()
    {
        return FileParser.ParseJson(name, ToJson(), false);
    }

    public string ToJson()
    {
        string dataString = "";

        if (parent && parent.GetDataType() != DataType.ValueArray) dataString += $"\"{name}\":";

        if (_dataType == DataType.Array)
        {
            dataString += "{";

            int i = 0;
            foreach (var item in data.Values)
            {
                dataString += item.ToJson();

                i++;

                if(i < data.Count) dataString += ",";
            }

            dataString += "}";
            return dataString;
        }
        else if(_dataType == DataType.ValueArray)
        {
            dataString += "[ ";

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
            dataString += _rawData;

            return dataString;
        }
    }

    private static void GetAllItems(DynamicData data, ref List<DynamicData> allData)
    {
        foreach (var item in data.data.Values)
        {
            var type = item.GetDataType();

            if(type == DataType.Value && item.parent.GetDataType() != DataType.ValueArray) // May want to remove this last part later...
            {
                allData.Add(item);
            }
            else if(type == DataType.Array || type == DataType.ValueArray)
            {
                GetAllItems(item, ref allData);
            }
        }
    }
    private static bool ValueIllegal(string v)
    {
        return v.Contains(',') || v.Contains('[') || v.Contains(']') || v.Contains('{') || v.Contains('}');
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

    public static implicit operator bool(DynamicData d) => !(d == null || d.GetDataType() == DataType.Unknown);
}

public enum DataType
{
    Unknown,
    Value,
    ValueArray,
    Array
}