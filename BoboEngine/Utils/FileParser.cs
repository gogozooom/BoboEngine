namespace BoboEngine.Utils;

public static class FileParser
{
    public static DynamicData ParseJson(string data, string name = null)
    {
        DynamicData currentData = null;

        string buffer = "";
        string key = "";

        bool isFirstChar = true;

        bool expectingKey = true;
        bool expectingValue = false;
        bool expectingValueArray = false;

        int line = 1;

        foreach (var c in data.ToCharArray())
        {
            if (c == '\r' || c == '\n') { line++; continue; }
            if (c == ' ') continue; // Ignore whitespace

            if (c == '{' && isFirstChar)
            {
                currentData = new DynamicData(name, DataType.Array);
            }
            else if (c == '[')
            {
                if (expectingKey) throw new UnexpectedCharacterException(c, line);

                currentData = new DynamicData(key, DataType.ValueArray, null, currentData);

                expectingValueArray = true;
                expectingValue = true;
            }
            else if (c == '{')
            {
                if (expectingKey) throw new UnexpectedCharacterException(c, line);

                if (currentData && currentData.GetDataType() == DataType.ValueArray)
                {
                    currentData = new DynamicData(currentData.data.Count.ToString(), DataType.Array, null, currentData);
                }
                else
                {
                    currentData = new DynamicData(key, DataType.Array, null, currentData);
                    key = "";
                }

                expectingValueArray = false;
                expectingKey = true;
                expectingValue = false;
            }
            else if (c == ']' || c == '}')
            {
                if (expectingValue)
                {
                    // Value found
                    if (currentData.GetDataType() == DataType.ValueArray)
                    {
                        new DynamicData(currentData.data.Count.ToString(), DataType.Value, buffer, currentData);
                    }
                    else
                    {
                        new DynamicData(key, DataType.Value, buffer, currentData);
                    }
                    key = "";
                    buffer = "";
                }

                if (currentData.parent == null)
                {
                    //Engine.Log("Reached end of file");
                    return currentData;
                }
                currentData = currentData.parent;

                expectingValueArray = false;
                expectingValue = false;
                expectingKey = false;
            }
            else if (c == ',')
            {
                if (expectingKey) throw new UnexpectedCharacterException(c,line);

                if (expectingValue)
                {
                    // Value found
                    if (currentData.GetDataType() == DataType.ValueArray)
                    {
                        new DynamicData(currentData.data.Count.ToString(), DataType.Value, buffer, currentData);
                        expectingValue = true;
                        expectingKey = false;
                    }
                    else
                    {
                        new DynamicData(key, DataType.Value, buffer, currentData);
                        expectingValue = false;
                        expectingKey = true;
                    }
                    key = "";
                    buffer = "";
                }
                else if (currentData.GetDataType() != DataType.ValueArray)
                {
                    expectingKey = true;
                }
            }
            else if (c == ':' && buffer.Length > 1 && buffer.EndsWith('\"'))
            {
                if (!expectingKey) throw new UnexpectedCharacterException(c, line);
                if (buffer == "") throw new Exception("':' encountered before key was given!");

                // Key found
                key = buffer.Replace("\"", "");
                buffer = "";

                expectingKey = false;
                expectingValue = true;
            }
            else
            {
                buffer += c;
            }

            isFirstChar = false;
        }

        if (expectingValue)
        {
            currentData = new DynamicData(key, DataType.Value, buffer, currentData);
        }

        return currentData;
    }

    public class UnexpectedCharacterException(char c, int line) : Exception($"Unexpected '{c}' encountered on line: {line}");
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

        if (dataType == DataType.Value && rawData == null) Engine.LogWarning("DynamicData created as a value with no data!");
        if (parent) if (!parent.HasItem(name)) parent.Add(this);
    }

    public DataType GetDataType() => _dataType;
    public T GetValue<T>()
    {
        if (_dataType == DataType.Unknown)
        {
            Engine.LogWarning($"Cannot get value of an empty data list!");
            return default;
        }

        if (_dataType == DataType.Array)
        {
            Engine.LogWarning($"Property of '{name}' is an array! Please use GetItem()!");
            return default;
        }

        if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(_rawData.Trim('"'), typeof(string));

        if (typeof(T) == typeof(bool))
        {
            if (_dataType != DataType.Value)
            {
                Engine.LogError($"Type of '{name}' is not a Value! Cannot convert to '{typeof(bool)}'!");
                return default;
            }

            switch (_rawData)
            {
                case "false":
                    return (T)Convert.ChangeType(false, typeof(bool));

                case "true":
                    return (T)Convert.ChangeType(true, typeof(bool));

                default:
                    Engine.LogError($"Could not parse bool out of '{_rawData}' from '{name}'!");
                    return default;
            }
        }

        if (typeof(T) == typeof(float))
        {
            if (_dataType != DataType.Value)
            {
                Engine.LogError($"Type of '{name}' is not a Value! Cannot convert to '{typeof(float)}'!");
                return default;
            }

            if(!float.TryParse(_rawData, out var value))
            {
                Engine.LogError($"Could not parse float out of '{_rawData}' from '{name}'!");
                return default;
            }

            return (T)Convert.ChangeType(value, typeof(float));
        }

        if (typeof(T) == typeof(int))
        {
            if (_dataType != DataType.Value)
            {
                Engine.LogError($"Type of '{name}' is not a Value! Cannot convert to '{typeof(int)}'!");
                return default;
            }

            if (!int.TryParse(_rawData, out var value))
            {
                Engine.LogError($"Could not parse int out of '{_rawData}' from '{name}'!");
                return default;
            }

            return (T)Convert.ChangeType(value, typeof(int));
        }

        if (typeof(T) == typeof(Float3))
        {
            if(_dataType != DataType.ValueArray)
            {
                Engine.LogError($"Type of '{name}' is not a ValueArray! Cannot convert to '{typeof(Float3)}'!");
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
                Engine.LogError($"Type of '{name}' is not a ValueArray! Cannot convert to '{typeof(Float3)}'!");
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
                Engine.LogError($"Type of '{name}' is not a Value! Cannot convert to '{typeof(Axis)}'!");
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
                    Engine.LogError($"Could not parse axis out of '{_rawData}' from '{name}'!");
                    return default;
            }
        }

        Engine.LogError($"Type of '{name}' cannot be converted to type: '{typeof(T)}' as this data is of type: '{_dataType}'!");
        return default;
    }
    public string GetValue() => GetValue<String>();
    public void SetValue(string value)
    {
        if (_dataType == DataType.Unknown)
        {
            Engine.LogWarning($"Cannot set value of an empty data list!");
            return;
        }

        if (ValueIllegal(value))
        {
            Engine.LogWarning($"Value '{value}' contains illegal characters!");
            return;
        }

        if (!value.StartsWith('"')) value = '"' + value;
        if (!value.EndsWith('"')) value += '"';

        _rawData = value;
        data.Clear();
        _dataType = DataType.Value;
    }
    public void SetValue(DynamicData values)
    {
        SetDataType(values._dataType);

        _rawData = values._rawData;
        data.Clear();
        foreach (var value in values.Copy().data)
        {
            data.Add(value.Key, value.Value);
        }
    }
    public DynamicData GetItem(string id)
    {
        if(_dataType == DataType.Value)
        {
            Engine.LogWarning($"Property of '{name}' is not an array! Please use GetValue()!");
            return null;
        }

        if (!data.TryGetValue(id, out var result))
        {
            //Program.LogError($"Item '{id}' does not exist in property '{name}'!");
            return null;
        }

        return result;
    }
    public List<DynamicData> GetAllValues(bool includeArrays = false)
    {
        List<DynamicData> result = new();

        GetAllItems(this, ref result, includeArrays);

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
    public void Remove()
    {
        if (!parent)
        {
            Engine.LogWarning($"'{name}' Cannot remove root data!");
            return;
        }

        parent.RemoveItem(name);
    }
    public bool HasItem(string id)
    {
        if (_dataType == DataType.Value)
        {
            Engine.LogWarning($"Property of '{name}' is not an array!");
        }

        return data.ContainsKey(id);
    }
    public void Add(DynamicData d)
    {
        data.Add(d.name, d);
        d.parent = this;
    }

    public void Merge(DynamicData d)
    {
        if (name == d.name && _dataType == DataType.Value && d._dataType == DataType.Value)
        {
            SetValue(d);
            return;
        }

        if (_dataType == DataType.Array && d._dataType == DataType.Array)
        {
            foreach (var itemToMerge in d.GetImmediateChildren())
            {
                var conflictD = GetItem(itemToMerge.name);

                if (!conflictD)
                {
                    Add(itemToMerge.Copy());
                    continue;
                }

                // Base has item with same id as itemToMerge

                conflictD.Merge(itemToMerge);
            }
        }
    }

    public DynamicData Copy()
    {
        return FileParser.ParseJson(ToJson(true), name);
    }

    public string ToJson(bool includeName = false)
    {
        string dataString = "";

        if (parent && parent.GetDataType() != DataType.ValueArray || includeName) dataString += $"\"{name}\":";

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

    private static void GetAllItems(DynamicData data, ref List<DynamicData> allData, bool includeArrays = false)
    {
        foreach (var item in data.data.Values)
        {
            var type = item.GetDataType();

            if(type == DataType.Value && item.parent.GetDataType() != DataType.ValueArray || (includeArrays && type == DataType.Array)) // May want to remove Parent == ValueArray later...
            {
                allData.Add(item);
            }
            if(type == DataType.Array || type == DataType.ValueArray)
            {
                GetAllItems(item, ref allData, includeArrays);
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
                return $"{name} = '{_rawData}'";
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