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
}
