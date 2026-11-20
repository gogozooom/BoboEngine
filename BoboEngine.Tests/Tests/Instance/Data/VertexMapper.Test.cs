using System.ComponentModel;
using Xunit;
using Xunit.Internal;

namespace BoboEngine.Tests;

public class VertexMapperTest
{
    [Fact]
    public void ExpectConvertedDataToBeTheSame()
    {
        int[][][] input = [[[1, 2, 3], [4, 5, 6]]];

        Assert.Equal(input, new VertexMapper(input).Data());
    }

    [Fact]
    public void ExpectCorrectlyTriangluatedData()
    {
        int[][][] squareInput = [ // List
                [ // Face
                    [0,0], [1,2], [2,2], [3,4] // VertexI
                ]
            ];
        int[][][] hexagonInput = [[[0, 0], [1, 1], [2, 2], [3, 3], [4, 4], [5, 5]]];

        int[][][] expectedSquareOutput = [
                [[0, 0], [1, 2], [2, 2]],
                [[0, 0], [2, 2], [3, 4]]
            ];
        int[][][] expectedHexagonOutput = [
                [[0,0], [1,1], [2,2]],
                [[0,0], [2,2], [3,3]],
                [[0,0], [3,3], [4,4]],
                [[0,0], [4,4], [5,5]],
            ];

        Assert.Equal(expectedSquareOutput, new VertexMapper(squareInput).Triangulated().Data());
        Assert.Equal(expectedHexagonOutput, new VertexMapper(hexagonInput).Triangulated().Data());
    }

    [Fact]
    public void ExpectIsEmptyToBeTrue()
    {
        var mapper = new VertexMapper();

        Assert.True(mapper.IsEmpty());
    }

    [Fact]
    public void ExpectIsEmptyToBeFalse()
    {
        int[][][] sampleInput = [ // List
                [ // Face
                    [0,0], [1,2], [2,2], [3,4] // VertexI
                ]
            ];

        var mapper = new VertexMapper(sampleInput);

        Assert.False(mapper.IsEmpty());
    }

    [Fact]
    public void ExpectCorrectlyConvertedData()
    {
        var data = new VertexData();

        var verts = data.AddAttribute<Float3>("vertices");
        var uvs = data.AddAttribute<Float2>("uvCoords");

        verts.Populate([new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)]);
        uvs.Populate([new(0, 0), new(1, 0), new(1, 1), new(0, 1)]);

        int[][][] map = [
                [[0,0], [1,1], [2,2]],
                [[0,0], [2,2], [3,3]]
            ];

        var mapper = new VertexMapper(map);

        float[] expected = {
            0, 0, 0, // Vertex Pos
            0, 0, // UV Pos
            1, 0, 0,
            1, 0,
            1, 1, 0,
            1, 1,
            0, 0, 0,
            0, 0,
            1, 1, 0,
            1, 1,
            0, 1, 0,
            0, 1};

        List<byte> expectedBytes = [];

        foreach (var item in expected)
        {
            expectedBytes.AddRange(BitConverter.GetBytes(item));
        }

        byte[] result = mapper.glConvertToData(data);

        Assert.Equal(expectedBytes, result);
    }

    [Fact]

    public void ExpectNegativeIndexesToGiveDefaultValues()
    {
        var data = new VertexData();

        var verts = data.AddAttribute<Float3>("vertices");
        var uvs = data.AddAttribute<Float2>("uvCoords");

        verts.Populate([new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)]);
        uvs.Populate([new(0, 0), new(1, 0), new(1, 1), new(0, 1)]);

        int[][][] map = [
                [[-1,-2], [1,1], [2,2]],
                [[-5,-10], [2,2], [3,3]]
            ];

        var mapper = new VertexMapper(map);

        float[] expected = {
            0, 0, 0, // Vertex Pos
            0, 0, // UV Pos
            1, 0, 0,
            1, 0,
            1, 1, 0,
            1, 1,
            0, 0, 0,
            0, 0,
            1, 1, 0,
            1, 1,
            0, 1, 0,
            0, 1};

        List<byte> expectedBytes = [];

        foreach (var item in expected)
        {
            expectedBytes.AddRange(BitConverter.GetBytes(item));
        }

        byte[] result = mapper.glConvertToData(data);

        Assert.Equal(expectedBytes, result);
    }
    [Fact]

    public void ExpectEmptyIndexesToGiveDefaultValues()
    {
        var data = new VertexData();

        var verts = data.AddAttribute<Float3>("vertices");
        var uvs = data.AddAttribute<Float2>("uvCoords");

        verts.Populate([new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)]);
        uvs.Populate([new(0, 0), new(1, 0), new(1, 1), new(0, 1)]);

        int[][][] map = [
                [[-1], [1,1], [2,2]],
                [[], [2,2], [3,3]]
            ];

        var mapper = new VertexMapper(map);

        float[] expected = {
            0, 0, 0, // Vertex Pos
            0, 0, // UV Pos
            1, 0, 0,
            1, 0,
            1, 1, 0,
            1, 1,
            0, 0, 0,
            0, 0,
            1, 1, 0,
            1, 1,
            0, 1, 0,
            0, 1};

        List<byte> expectedBytes = [];

        foreach (var item in expected)
        {
            expectedBytes.AddRange(BitConverter.GetBytes(item));
        }

        byte[] result = mapper.glConvertToData(data);

        Assert.Equal(expectedBytes, result);
    }
}
