using Xunit;
using static OpenGL.GL;

namespace BoboEngine.Tests;

public class VertexDataTests
{
    [Fact]
    public void ExpectSuccessfulAttributeCreating()
    {
        var data = new VertexData();

        data.AddAttribute<float>("test");

        Assert.NotNull(data.GetAttribute<float>("test"));
    }

    [Fact]
    public void ExpectAutomaticShaderIndexMapping()
    {
        var data = new VertexData();

        var attribute1 = data.AddAttribute<float>("test1");
        var attribute2 = data.AddAttribute<float>("test2");
        var attribute3 = data.AddAttribute<float>("test3");

        Assert.Equal((uint)0, attribute1.shaderIndex);
        Assert.Equal((uint)1, attribute2.shaderIndex);
        Assert.Equal((uint)2, attribute3.shaderIndex);
    }

    [Fact]
    public void ExpectReturningSameAttributeWhenAddingDuplicates()
    {
        var data = new VertexData();

        var attribute = data.AddAttribute<float>("test");

        Assert.Equal(attribute, data.AddAttribute<float>("test"));
    }

    [Fact]
    public void ExpectTypeMissMatchErrorWhenGettingAnExistingAttributeOfDifferentType()
    {
        var data = new VertexData();

        data.AddAttribute<float>("test");

        Assert.Throws<VertexData.AttributeTypeMissMatchException>(() => data.GetAttribute<bool>("test"));
    }

    [Fact]
    public void ExpectTypeMissMatchErrorWhenAddingDifferentDuplicates()
    {
        var data = new VertexData();

        data.AddAttribute<float>("test");

        Assert.Throws<VertexData.ExistingAttributeMissMatchException>(() => data.AddAttribute<bool>("test"));
    }

    [Fact]
    public void ExpectSuccessfullAttributePopulating()
    {
        float[] floatData = [0, 1, 2];
        double[] doubleData = [0.5, -1.1, 2.0];

        var data = new VertexData();

        var floatsAttribute = data.AddAttribute<float>("floats");
        data.AddAttribute<double>("doubles");

        floatsAttribute.Populate(floatData);
        data.PopulateAttribute("doubles", doubleData);

        Assert.Equal(floatData, floatsAttribute.GetValues());
        Assert.Equal(doubleData, data.GetAttribute<double>("doubles").GetValues());
    }

    [Fact]
    public void ExpectInvalidTypeErrorWhenCreatingAUnsupportedAttribute()
    {
        var data = new VertexData();

        Assert.Throws<VertexData.InvalidTypeException>(() => data.AddAttribute<string>("test"));
        Assert.Throws<VertexData.InvalidTypeException>(() => data.AddAttribute<object>("test"));
        Assert.Throws<VertexData.InvalidTypeException>(() => data.AddAttribute<GameObject>("test"));
    }

    [Fact]
    public void ExpectCorrectGLTypeMapping()
    {
        Assert.Equal(GL_UNSIGNED_BYTE, new VertexAttribute<bool>(0).glType());
        Assert.Equal(GL_UNSIGNED_BYTE, new VertexAttribute<byte>(0).glType());
        Assert.Equal(GL_BYTE, new VertexAttribute<sbyte>(0).glType());

        Assert.Equal(GL_INT, new VertexAttribute<int>(0).glType());
        Assert.Equal(GL_INT, new VertexAttribute<Int2>(0).glType());
        Assert.Equal(GL_INT, new VertexAttribute<Int3>(0).glType());
        Assert.Equal(GL_INT, new VertexAttribute<Int4>(0).glType());
        Assert.Equal(GL_UNSIGNED_INT, new VertexAttribute<uint>(0).glType());

        Assert.Equal(GL_SHORT, new VertexAttribute<short>(0).glType());
        Assert.Equal(GL_UNSIGNED_SHORT, new VertexAttribute<ushort>(0).glType());

        Assert.Equal(GL_FLOAT, new VertexAttribute<float>(0).glType());
        Assert.Equal(GL_FLOAT, new VertexAttribute<Float2>(0).glType());
        Assert.Equal(GL_FLOAT, new VertexAttribute<Float3>(0).glType());
        Assert.Equal(GL_FLOAT, new VertexAttribute<Float4>(0).glType());

        Assert.Equal(GL_DOUBLE, new VertexAttribute<double>(0).glType());
        Assert.Equal(GL_DOUBLE, new VertexAttribute<Double2>(0).glType());
        Assert.Equal(GL_DOUBLE, new VertexAttribute<Double3>(0).glType());
    }


    [Fact]
    public void ExpectCorrectGLTypeSizes()
    {
        Assert.Equal(sizeof(bool), new VertexAttribute<bool>(0).glTypeSize());
        Assert.Equal(sizeof(byte), new VertexAttribute<byte>(0).glTypeSize());
        Assert.Equal(sizeof(sbyte), new VertexAttribute<sbyte>(0).glTypeSize());

        Assert.Equal(sizeof(short), new VertexAttribute<short>(0).glTypeSize());
        Assert.Equal(sizeof(ushort), new VertexAttribute<ushort>(0).glTypeSize());

        Assert.Equal(sizeof(int), new VertexAttribute<int>(0).glTypeSize());
        Assert.Equal(sizeof(int), new VertexAttribute<Int2>(0).glTypeSize());
        Assert.Equal(sizeof(int), new VertexAttribute<Int3>(0).glTypeSize());
        Assert.Equal(sizeof(int), new VertexAttribute<Int4>(0).glTypeSize());
        Assert.Equal(sizeof(uint), new VertexAttribute<uint>(0).glTypeSize());
        Assert.Equal(sizeof(float), new VertexAttribute<float>(0).glTypeSize());
        Assert.Equal(sizeof(float), new VertexAttribute<Float2>(0).glTypeSize());
        Assert.Equal(sizeof(float), new VertexAttribute<Float3>(0).glTypeSize());
        Assert.Equal(sizeof(float), new VertexAttribute<Float4>(0).glTypeSize());

        Assert.Equal(sizeof(double), new VertexAttribute<double>(0).glTypeSize());
        Assert.Equal(sizeof(double), new VertexAttribute<Double2>(0).glTypeSize());
        Assert.Equal(sizeof(double), new VertexAttribute<Double3>(0).glTypeSize());
    }

    [Fact]
    public void ExpectCorrectGLItemSizes()
    {
        Assert.Equal(1, new VertexAttribute<bool>(0).glItemSize());
        Assert.Equal(1, new VertexAttribute<byte>(0).glItemSize());
        Assert.Equal(1, new VertexAttribute<sbyte>(0).glItemSize());

        Assert.Equal(1, new VertexAttribute<int>(0).glItemSize());
        Assert.Equal(2, new VertexAttribute<Int2>(0).glItemSize());
        Assert.Equal(3, new VertexAttribute<Int3>(0).glItemSize());
        Assert.Equal(4, new VertexAttribute<Int4>(0).glItemSize());
        Assert.Equal(1, new VertexAttribute<uint>(0).glItemSize());

        Assert.Equal(1, new VertexAttribute<short>(0).glItemSize());
        Assert.Equal(1, new VertexAttribute<ushort>(0).glItemSize());

        Assert.Equal(1, new VertexAttribute<float>(0).glItemSize());
        Assert.Equal(2, new VertexAttribute<Float2>(0).glItemSize());
        Assert.Equal(3, new VertexAttribute<Float3>(0).glItemSize());
        Assert.Equal(4, new VertexAttribute<Float4>(0).glItemSize());

        Assert.Equal(1, new VertexAttribute<double>(0).glItemSize());
        Assert.Equal(2, new VertexAttribute<Double2>(0).glItemSize());
        Assert.Equal(3, new VertexAttribute<Double3>(0).glItemSize());
    }

    [Fact]
    public void ExpectCorrectGLStrides()
    {
        Assert.Equal(sizeof(byte), new VertexAttribute<bool>(0).glStride());
        Assert.Equal(sizeof(byte), new VertexAttribute<byte>(0).glStride());
        Assert.Equal(sizeof(sbyte), new VertexAttribute<sbyte>(0).glStride());

        Assert.Equal(1 * sizeof(int), new VertexAttribute<int>(0).glStride());
        Assert.Equal(2 * sizeof(int), new VertexAttribute<Int2>(0).glStride());
        Assert.Equal(3 * sizeof(int), new VertexAttribute<Int3>(0).glStride());
        Assert.Equal(4 * sizeof(int), new VertexAttribute<Int4>(0).glStride());
        Assert.Equal(sizeof(uint), new VertexAttribute<uint>(0).glStride());

        Assert.Equal(sizeof(short), new VertexAttribute<short>(0).glStride());
        Assert.Equal(sizeof(ushort), new VertexAttribute<ushort>(0).glStride());

        Assert.Equal(1 * sizeof(float), new VertexAttribute<float>(0).glStride());
        Assert.Equal(2 * sizeof(float), new VertexAttribute<Float2>(0).glStride());
        Assert.Equal(3 * sizeof(float), new VertexAttribute<Float3>(0).glStride());
        Assert.Equal(4 * sizeof(float), new VertexAttribute<Float4>(0).glStride());

        Assert.Equal(1 * sizeof(double), new VertexAttribute<double>(0).glStride());
        Assert.Equal(2 * sizeof(double), new VertexAttribute<Double2>(0).glStride());
        Assert.Equal(3 * sizeof(double), new VertexAttribute<Double3>(0).glStride());
    }

    [Fact]
    public void ExpectIsEmptyToBeTrue()
    {
        var data = new VertexData();

        Assert.True(data.IsEmpty());
    }

    [Fact]
    public void ExpectIsEmptyToBeFalse()
    {
        var data = new VertexData();

        data.AddAttribute<Float3>("position").Populate([new()]);

        Assert.False(data.IsEmpty());
    }

    [Fact]
    public void ExpectCorrectlyConvertedData()
    {
        var data = new VertexData();

        var floats = data.AddAttribute<float>("floats");
        floats.Populate([0,1,2,3]);

        Assert.Equal(FloatToByteList([0, 1, 3]), floats.glConvertToData([0, 1, 3]));
        Assert.Equal(FloatToByteList([3, 1, 2]), floats.glConvertToData([3, 1, 2]));
        Assert.Equal(FloatToByteList([0, 1, 2, 3]), floats.glConvertToData());

        var float3s = data.AddAttribute<Float3>("float3s");
        float3s.Populate([new(0,0,0), new(0,1,0), new(1,0,0)]);

        Assert.Equal(FloatToByteList([0, 0, 0, 1, 0, 0]), float3s.glConvertToData([0, 2]));
        Assert.Equal(FloatToByteList([0,0,0,0,1,0,1,0,0]), float3s.glConvertToData());
    }

    public IEnumerable<byte> FloatToByteList(float[] values)
    {
        List<byte> result = [];

        foreach (var item in values)
        {
            result.AddRange(BitConverter.GetBytes(item));
        }

        return result;
    }
}
    