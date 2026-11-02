using BoboEngine.UI;
using System.ComponentModel;
using Xunit;

namespace BoboEngine.Tests;

public class TextRendererTests
{

    public class ConnectByIndicesTests
    {
        [Fact]
        public void ConnectByIndices_HandleEmptyArraysInput()
        {
            int[][] input = [];

            Assert.Equal([], TextRenderer.ConnectByIndices(input));
        }

        [Fact]
        public void ConnectByIndices_BasicConnectsLists()
        {
            int[][] input = [[00, 01, 02], [10, 11, 12]];

            int[] expectedResult = [00, 01, 02, 10, 11, 12];

            Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(input, loopConnect:false));
        }

        [Fact]
        public void ConnectByIndices_LoopConnectsLists()
        {
            int[][] input = [[00, 01, 02], [10, 11, 12]];

            int[] expectedResult = [
                00, 01, 02,
                    10, 11, 12, 10,
                02];

            Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(input, [], []));
        }

        [Fact]
        public void ConnectByIndices_IntigrationTest()
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

        //[Fact]
        //public void ConnectByIndices_ExpectArrayInsertConnect()
        //{
        //    int[] expectedResult = [
        //        00, 01, 02,
        //            10,
        //                20, 21, 22, 23, 24,
        //                    30, 31,
        //            11, 12, 13, 14,
        //        03, 04];

        //    Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(arraysInput, [], [2, 0], loopConnect: false));
        //}
        //[Fact]
        //public void ConnectByIndices_ConnectMultipleArraysLoopingBackToLastItem()
        //{
        //    int[] expectedResult = [
        //        00, 01,
        //            02,
        //                10,
        //                    20,
        //                        21, 22, 23,
        //                        24,
        //                            30,
        //                                31,
        //                            30,
        //                        24,
        //                    20,
        //                10,
        //                    11, 12, 13, 14,
        //                10,
        //            02,
        //        03, 04];

        //    Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(arraysInput, [], [2, 0]));
        //}
        //[Fact]
        //public void ConnectByIndices_ShouldHandleNoConnectIndices()
        //{
        //    int[] expectedResult = [
        //        00, 01, 02, 03, 04,
        //        10, 11, 12, 13, 14,
        //        20, 21, 22, 23, 24,
        //        30, 31];

        //    Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(arraysInput, [], [], loopConnect: false));
        //}
        //[Fact]
        //public void ConnectByIndices_ShouldHandleNullConnectIndices()
        //{
        //    int[] expectedResult = [
        //        00, 01, 02, 03, 04,
        //        10, 11, 12, 13, 14,
        //        20, 21, 22, 23, 24,
        //        30, 31];

        //    Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(arraysInput, null, null, loopConnect: false));
        //}
        //[Fact]
        //public void ConnectByIndices_ShouldHandleNoConnectIndicesLoopConnected()
        //{
        //    int[] expectedResult = [
        //        00, 01, 02, 03,
        //            04,
        //                10, 11, 12, 13,
        //                    14,
        //                        20, 21, 22, 23,
        //                            24,
        //                                30,
        //                                    31,
        //                                30,
        //                            24,
        //                        20,
        //                    14,
        //                10,
        //            04];

        //    Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(arraysInput, [], []));
        //}
        //[Fact]
        //public void ConnectByIndices_ShouldHandleNullConnectIndicesLoopConnected()
        //{
        //    int[] expectedResult = [
        //        00, 01, 02, 03,
        //            04,
        //                10, 11, 12, 13,
        //                    14,
        //                        20, 21, 22, 23,
        //                            24,
        //                                30,
        //                                    31,
        //                                30,
        //                            24,
        //                        20,
        //                    14,
        //                10,
        //            04];

        //    Assert.Equal(expectedResult, TextRenderer.ConnectByIndices(arraysInput, null, null));
        //}
    }

    public class DoLinesIntersectTests
    {
        [Fact]
        [Category("DoLinesIntersect()")]
        public void DoLinesIntersect_ShouldBeTrue()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-4.3f, 1.79f), new(-2.96f, -2.3f)));
        }

        [Fact]
        [Category("DoLinesIntersect()")]
        public void DoLinesIntersect_ShouldBeFalse()
        {
            Assert.False(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-1.45f, -1.58f), new(-10.62f, -0.04f)));
        }

        [Fact]
        [Category("DoLinesIntersect()")]
        public void DoLinesIntersect_ShouldBeFalseALength0()
        {
            Assert.False(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-5.57f, -0.06f),
                new(-5.57f, -0.06f), new(-10.62f, -0.04f)));
        }

        [Fact]
        [Category("DoLinesIntersect()")]
        public void DoLinesIntersect_ShouldBeFalseBLength0()
        {
            Assert.False(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-5.57f, -0.06f), new(-5.57f, -0.06f)));
        }

        [Fact]
        [Category("DoLinesIntersect()")]
        public void DoLinesIntersect_ShouldBeTrueA1B1Meet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-5.57f, -0.06f), new(-10.62f, -0.04f)));
        }

        [Fact]
        [Category("DoLinesIntersect()")]
        public void DoLinesIntersect_ShouldBeTrueA1B2Meet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-1.45f, -1.58f), new(-5.57f, -0.06f)));
        }

        [Fact]
        [Category("DoLinesIntersect()")]
        public void DoLinesIntersect_ShouldBeTrueA2B1Meet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-2.54f, 2.23f), new(-10.62f, -0.04f)));
        }

        [Fact]
        [Category("DoLinesIntersect()")]
        public void DoLinesIntersect_ShouldBeTrueA2B2Meet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5.57f, -0.06f), new(-2.54f, 2.23f),
                new(-1.45f, -1.58f), new(-2.54f, 2.23f)));
        }

        [Fact]
        [Category("DoLinesIntersect()")]
        public void DoLinesIntersect_ShouldBeTrueA1BMeet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-3, -1), new(-1, 2.23f),
                new(-1.45f, -1), new(-6.04f, -1)));
        }

        [Fact]
        [Category("DoLinesIntersect()")]
        public void DoLinesIntersect_ShouldBeTrueB1AMeet()
        {
            Assert.True(TextRenderer.DoLinesIntersect(
                new(-5, 1), new(-1, 1),
                new(-2, -1), new(-3, 1)));
        }
    }
}
