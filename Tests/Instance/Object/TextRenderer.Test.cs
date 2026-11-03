using BoboEngine.UI;
using Xunit;

namespace BoboEngine.Tests;

public class TextRendererTests
{

    public class ConnectByIndicesTests
    {
        [Fact]
        public void BasicConnectsLists()
        {
            int[][] input = [[00, 01, 02], [10, 11, 12]];

            int[] expectedResult = [00, 01, 02, 10, 11, 12];

            Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(input, loopConnect: false));
        }

        [Fact]
        public void HandleEmptyArraysInput()
        {
            int[][] input = [];

            Assert.Equal([], TextRenderer.ConnectByIndices(input));
        }

        [Fact]
        public void LoopConnectsLists()
        {
            int[][] input = [[00, 01, 02], [10, 11, 12]];

            int[] expectedResult = [
                00, 01, 02,
                    10, 11, 12, 10,
                02];

            Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(input, [], []));
        }

        [Fact]
        public void LoopConnectsListsAtIndex()
        {
            int[][] input = [[00, 01, 02], [10, 11, 12]];

            int[] expectedResult = [
                00, 01,
                    10, 11, 12, 10,
                01, 02];

            Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(input, [], [1]));
        }

        [Fact]
        public void LoopConnectsListsAtIndexWithStartingOffset()
        {
            int[][] input = [[00, 01, 02], [10, 11, 12]];

            int[] expectedResult = [
                02, 00, 01,
                    10, 11, 12, 10,
                01];

            Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(input, [2], [1]));
        }

        [Fact]
        public void LoopConnectsListsWithNullIndices()
        {
            int[][] input = [[00, 01, 02], [10, 11, 12]];

            int[] expectedResult = [
                00, 01, 02,
                    10, 11, 12, 10,
                02];

            Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(input, null, null));
        }

        [Fact]
        public void IntegrationTest()
        {
            int[][] input = [[00, 01, 02, 03, 04], [10, 11, 12, 13, 14], [20, 21, 22, 23, 24], [30, 31]];

            int[] expectedResult = [
                00, 01,
                    02,
                        11,12,13,14,
                            10,
                                23, 24, 20, 21,
                                    22,
                                        30,
                                            31,
                                        30,
                                    22,
                                23,
                            10,
                        11,
                    02,
                03, 04];

            Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(input, [0, 1, 3], [2, 0]));
        }
    }

    public class DoLinesIntersectTests
    {
        [Fact]
        public void ShouldBeFalse()
        {
            Assert.False(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-1.45f, -1.58f), new(-10.62f, -0.04f)));
        }

        [Fact]
        public void ShouldBeFalseALength0()
        {
            Assert.False(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-5.57f, -0.06f),
                new(-5.57f, -0.06f), new(-10.62f, -0.04f)));
        }

        [Fact]
        public void ShouldBeFalseBLength0()
        {
            Assert.False(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-5.57f, -0.06f), new(-5.57f, -0.06f)));
        }

        [Fact]
        public void ShouldBeTrue()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-4.3f, 1.79f), new(-2.96f, -2.3f)));
        }

        [Fact]
        public void ShouldBeTrueA1B1Meet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-5.57f, -0.06f), new(-10.62f, -0.04f)));
        }

        [Fact]
        public void ShouldBeTrueA1B2Meet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-1.45f, -1.58f), new(-5.57f, -0.06f)));
        }

        [Fact]
        public void ShouldBeTrueA1BMeet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-3, -1), new(-1, 2.23f),
                new(-1.45f, -1), new(-6.04f, -1)));
        }

        [Fact]
        public void ShouldBeTrueA2B1Meet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-2.54f, 2.23f), new(-10.62f, -0.04f)));
        }

        [Fact]
        public void ShouldBeTrueA2B2Meet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-1.45f, -1.58f), new(-2.54f, 2.23f)));
        }

        [Fact]
        public void ShouldBeTrueB1AMeet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5, 1), new(-1, 1),
                new(-2, -1), new(-3, 1)));
        }
    }
    
    public class IsPointWithinTriangleTests
    {
        static readonly Float2 v0 = new(0, 0);
        static readonly Float2 v1 = new(1, 0);
        static readonly Float2 v2 = new(0.5f, 1);

        [Fact]
        public void ShouldBeTrue()
        {
            Assert.True(TextRenderer.IsPointWithinTriangle(v0, v1, v2, new(0.5f, 0.5f)));
            Assert.True(TextRenderer.IsPointWithinTriangle(v0, v1, v2, new(0.25f, 0.25f)));
            Assert.True(TextRenderer.IsPointWithinTriangle(v0, v1, v2, new(0.95f, 0.01f)));
        }
        [Fact]
        public void ShouldBeFalse()
        {
            Assert.False(TextRenderer.IsPointWithinTriangle(v0, v1, v2, new(1, 1)));
            Assert.False(TextRenderer.IsPointWithinTriangle(v0, v1, v2, new(0, -1)));
            Assert.False(TextRenderer.IsPointWithinTriangle(v0, v1, v2, new(0.5f, 2)));
        }

        [Fact]
        public void ShouldBeFalseClockWise()
        {
            Assert.False(TextRenderer.IsPointWithinTriangle(v0, v2, v1, new(0.5f, 0.5f)));
            Assert.False(TextRenderer.IsPointWithinTriangle(v0, v2, v1, new(1f, 1f)));
            Assert.False(TextRenderer.IsPointWithinTriangle(v0, v2, v1, new(-1f, -1f)));
        }

        [Fact]
        public void ShouldBeTruePEqualsV0()
        {
            Assert.True(TextRenderer.IsPointWithinTriangle(v0, v1, v2, v0));
        }
        [Fact]
        public void ShouldBeTruePEqualsV1()
        {
            Assert.True(TextRenderer.IsPointWithinTriangle(v0, v1, v2, v1));
        }
        [Fact]
        public void ShouldBeTruePEqualsV2()
        {
            Assert.True(TextRenderer.IsPointWithinTriangle(v0, v1, v2, v2));
        }


        [Fact]
        public void ShouldBeTrueSquashedLine()
        {
            Float2 vCenter = new(0.5f, 0);

            Assert.True(TextRenderer.IsPointWithinTriangle(v0, vCenter, v1, v0));
            Assert.True(TextRenderer.IsPointWithinTriangle(v0, vCenter, v1, new(0.25f, 0)));
            Assert.True(TextRenderer.IsPointWithinTriangle(v0, vCenter, v1, vCenter));
            Assert.True(TextRenderer.IsPointWithinTriangle(v0, vCenter, v1, new(0.75f, 0)));
            Assert.True(TextRenderer.IsPointWithinTriangle(v0, vCenter, v1, v1));
        }
    }

    public class NormalizedUVChoordinateText
    {
        static readonly Float2 min = new(-0.5f, -0.5f);
        static readonly Float2 max = new(0.5f, 0.5f);

        [Fact]
        public void ExpectZeroWithPointAtMin()
        {
            Assert.Equal(Float2.zero, TextRenderer.NormalizedUVChoordinate(min, max, min));
        }
        [Fact]
        public void ExpectOneWithPointAtMax()
        {
            Assert.Equal(Float2.one, TextRenderer.NormalizedUVChoordinate(min, max, max));
        }
        [Fact]
        public void ExpectGradientInbetween()
        {
            var margin = 0.0001f;
            var expect1 = new Float2(0.4f, 0.1f);
            var expect2 = new Float2(0.5f, 0.5f);
            var expect3 = new Float2(0.7f, 0.9f);

            // Account for floating point precision
            Assert.InRange((expect1 - TextRenderer.NormalizedUVChoordinate(min, max, new(-0.1f, -0.4f))).Length, 0, margin);
            Assert.InRange((expect2 - TextRenderer.NormalizedUVChoordinate(min, max, Float2.zero)).Length, 0, margin);
            Assert.InRange((expect3 - TextRenderer.NormalizedUVChoordinate(min, max, new(0.2f, 0.4f))).Length, 0, margin);
        }
    }
}
