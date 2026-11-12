using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using static BoboEngine.VertexData;
using static OpenGL.GL;

namespace BoboEngine;

public class VertexData
{
    public readonly List<string> keys = [];
    public readonly ArrayList attributes = [];

    public VertexAttribute<T> GetAttribute<T>(string key)
    {
        var indexFound = keys.IndexOf(key);

        if (indexFound == -1) return null;

        var attributeFound = attributes[indexFound];

        if (attributeFound.GetType() != typeof(VertexAttribute<T>)) throw new AttributeTypeMissMatchException($"Tried to get existing attribute '{key}' of different type! '{typeof(VertexAttribute<T>)}' != '{attributeFound.GetType}'");

        return (VertexAttribute<T>)attributeFound;
    }
    public VertexAttribute<T> AddAttribute<T>(string key, int shaderIndex = -1)
    {
        if (VertexAttribute<T>.Type() == GLType.UNKNOWN) throw new InvalidTypeException($"Type of '{typeof(T)}' is unsupported!");

        var foundIndex = keys.IndexOf(key);

        if (foundIndex != -1)
        {
            var existing = attributes[foundIndex];
            if (existing.GetType() != typeof(VertexAttribute<T>))
            {
                throw new ExistingAttributeMissMatchException($"An attribute with key '{key}' already exists, but has a different type! '{typeof(VertexAttribute<T>)}' != '{existing.GetType}'");
            }

            return (VertexAttribute<T>)existing;
        }

        VertexAttribute<T> attribute = new((uint)(shaderIndex >= 0 ? shaderIndex : attributes.Count)); // TODO: make smarter auto shaderIndex mapping

        keys.Add(key);
        attributes.Add(attribute);

        return attribute;
    }

    public void PopulateAttribute<T>(string key, IEnumerable<T> values) => GetAttribute<T>(key).Populate(values);
    public void glBindPointers()
    {
#pragma warning disable CS8605 // Unboxing a possibly null value.
        int stride = 0;
        List<int> pointers = [];
        foreach (object attribute in attributes)
        {
            pointers.Add(stride);
            stride += (int)attribute.GetType().GetMethod("glStride").Invoke(attribute, []);
        }

        int i = 0;
        foreach (object attribute in attributes)
        {
            attribute.GetType().GetMethod("glBindPointers").Invoke(attribute, [stride, pointers[i++]]);
        }
#pragma warning restore CS8605 // Unboxing a possibly null value.
    }
    public enum GLType
    {
        UNKNOWN,
        Bool,
        Byte,
        SByte,
        Int,
        Int2,
        Int3,
        Int4,
        UInt,
        Short,
        UShort,
        Float,
        Float2,
        Float3,
        Float4,
        Double,
        Double2,
        Double3
    }

    public bool IsEmpty()
    {
        return attributes.Count == 0;
    }

    public class InvalidTypeException(string message) : Exception(message) { }
    public class AttributeTypeMissMatchException(string message) : Exception(message) { }
    public class ExistingAttributeMissMatchException(string message) : Exception(message) { }
}
public class VertexAttribute<T>(uint shaderIndex)
{
    public readonly uint shaderIndex = shaderIndex;
    public readonly List<T> values = [];

    public int glType()
    {
        return Type() switch
        {
            GLType.SByte => GL_BYTE,
            GLType.Short => GL_SHORT,
            GLType.UInt => GL_UNSIGNED_INT,
            GLType.UShort => GL_UNSIGNED_SHORT,
            GLType.Bool or GLType.Byte => GL_UNSIGNED_BYTE,
            GLType.Int or GLType.Int2 or GLType.Int3 or GLType.Int4 => GL_INT,
            GLType.Float or GLType.Float2 or GLType.Float3 or GLType.Float4 => GL_FLOAT,
            GLType.Double or GLType.Double2 or GLType.Double3 => GL_DOUBLE,
            _ => -1,
        };
    }

    public int glItemSize()
    {
        return Type() switch
        {
            GLType.Bool or GLType.Byte or GLType.SByte or GLType.UInt or GLType.Short or GLType.UShort or GLType.Int or GLType.Float or GLType.Double => 1, // Plz ignore big line :sobs:
            GLType.Int2 or GLType.Float2 or GLType.Double2 => 2,
            GLType.Int3 or GLType.Float3 or GLType.Double3 => 3,
            GLType.Int4 or GLType.Float4 => 4,
            _ => -1,
        };
    }
    public int glTypeSize()
    {
        return Type() switch
        {
            GLType.SByte or GLType.Byte or GLType.Bool => 1,
            GLType.Short or GLType.UShort => 2,
            GLType.UInt or GLType.Int or GLType.Int2 or GLType.Int3 or GLType.Int4 or
            GLType.Float or GLType.Float2 or GLType.Float3 or GLType.Float4 => 4,
            GLType.Double or GLType.Double2 or GLType.Double3 => 8,
            _ => -1,
        };
    }

    public int glStride()
    {
        return glItemSize() * glTypeSize();
    }

    public IEnumerable<byte> glConvertToData(int[] indicies = null)
    {
        List<byte> result = [];

        if (indicies == null)
        {
            for (int i = 0; i < values.Count; i++)
            {
                result.AddRange(glConvertItemToData(i));
            }
        }
        else
        {
            foreach (int i in indicies)
            {
                result.AddRange(glConvertItemToData(i));
            }
        }

        return result;
    }

    public IEnumerable<byte> glConvertItemToData(int index)
    {
        if (index < 0) return glGetDefaultData();
        else if (index >= values.Count) throw new IndexOutOfRangeException($"Could not get data at index '{index}'!");

        List<byte> results = [];

        object value = values[index];

        if(value == null) throw new TypeConversionException($"Could not convert '{Type()}' data to object!");

        // This feels so hacky, but it makes using boboengine much nicer :sob:
        // TODO: maybe add glConvertToData() for Int2 for example?

#pragma warning disable CS8605 // Unboxing a possibly null value.
        switch (Type())
        {
            case GLType.UInt:
            case GLType.Int:
                results.AddRange(BitConverter.GetBytes((int)value));
                break;
            case GLType.SByte:
            case GLType.Byte:
                results.Add((byte)value);
                break;
            case GLType.Float:
                results.AddRange(BitConverter.GetBytes((float)value));
                break;
            case GLType.Double:
                results.AddRange(BitConverter.GetBytes((double)value));
                break;
            case GLType.UShort:
            case GLType.Short:
                results.AddRange(BitConverter.GetBytes((short)value));
                break;
            case GLType.Bool:
                results.Add((bool)value ? (byte)255 : (byte)0);
                break;
            case GLType.Int2:
                results.AddRange(((Int2)value).glConvertToData());
                break;
            case GLType.Int3:
                results.AddRange(((Int3)value).glConvertToData());
                break;
            case GLType.Int4:
                results.AddRange(((Int4)value).glConvertToData());
                break;
            case GLType.Float2:
                results.AddRange(((Float2)value).glConvertToData());
                break;
            case GLType.Float3:
                results.AddRange(((Float3)value).glConvertToData());
                break;
            case GLType.Float4:
                results.AddRange(((Float4)value).glConvertToData());
                break;
            case GLType.Double2:
                results.AddRange(((Double2)value).glConvertToData());
                break;
            case GLType.Double3:
                results.AddRange(((Double3)value).glConvertToData());
                break;
            case GLType.UNKNOWN:
            default:
                break;
        }
#pragma warning restore CS8605 // Unboxing a possibly null value.

        if (results.Count == 0) throw new TypeConversionException($"Could not convert '{Type()}' data to float!");

        return results;
    }

    public IEnumerable<byte> glGetDefaultData()
    {
        List<byte> results = [];

        switch (Type())
        {
            case GLType.UInt:
            case GLType.Int:
                results.AddRange(BitConverter.GetBytes(0));
                break;
            case GLType.SByte:
            case GLType.Byte:
                results.Add(0);
                break;
            case GLType.Float:
                results.AddRange(BitConverter.GetBytes(0f));
                break;
            case GLType.Double:
                results.AddRange(BitConverter.GetBytes(0d));
                break;
            case GLType.UShort:
            case GLType.Short:
                results.AddRange(BitConverter.GetBytes((short)0));
                break;
            case GLType.Bool:
                results.Add(0);
                break;
            case GLType.Int2:
                results.AddRange(Int2.zero.glConvertToData());
                break;
            case GLType.Int3:
                results.AddRange(Int3.zero.glConvertToData());
                break;
            case GLType.Int4:
                results.AddRange(Int4.zero.glConvertToData());
                break;
            case GLType.Float2:
                results.AddRange(Float2.zero.glConvertToData());
                break;
            case GLType.Float3:
                results.AddRange(Float3.zero.glConvertToData());
                break;
            case GLType.Float4:
                results.AddRange(Float4.zero.glConvertToData());
                break;
            case GLType.Double2:
                results.AddRange(Double2.zero.glConvertToData());
                break;
            case GLType.Double3:
                results.AddRange(Double3.zero.glConvertToData());
                break;
            case GLType.UNKNOWN:
            default:
                break;
        }

        return results;
    }

    public unsafe void glBindPointers(int stride, int pointer = 0)
    {
        glVertexAttribPointer(shaderIndex, glItemSize(), glType(), false, stride, (void*)pointer);
        glEnableVertexAttribArray(shaderIndex);
    }

    public void Populate(IEnumerable<T> values)
    {
        this.values.Clear();
        this.values.AddRange(values);
    }

    public void Add(T value) => values.Add(value);


    public T[] GetValues() => values.ToArray();

    public static GLType Type()
    {
        return typeof(T).FullName switch
        {
            "System.Boolean" => GLType.Bool,
            "System.Byte" => GLType.Byte,
            "System.SByte" => GLType.SByte,
            "System.Int32" => GLType.Int,
            "BoboEngine.Int2" => GLType.Int2,
            "BoboEngine.Int3" => GLType.Int3,
            "BoboEngine.Int4" => GLType.Int4,
            "System.UInt32" => GLType.UInt,
            "System.Int16" => GLType.Short,
            "System.UInt16" => GLType.UShort,
            "System.Single" => GLType.Float,
            "BoboEngine.Float2" => GLType.Float2,
            "BoboEngine.Float3" => GLType.Float3,
            "BoboEngine.Float4" => GLType.Float4,
            "System.Double" => GLType.Double,
            "BoboEngine.Double2" => GLType.Double2,
            "BoboEngine.Double3" => GLType.Double3,
            _ => GLType.UNKNOWN,
        };
    }

    public class TypeConversionException(string message) : Exception(message);
}
