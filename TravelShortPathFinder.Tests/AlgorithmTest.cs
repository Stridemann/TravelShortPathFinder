#pragma warning disable CA1416
namespace TravelShortPathFinder.Tests
{
    using System.Numerics;
    using Algorithm.Data;
    using Algorithm.Logic;
    using Shouldly;
    using TestResources;
    using TestResources.Properties;

    public class AlgorithmTest
    {
        [Fact]
        public void GraphMapExplorerTest()
        {
            var navCase = InputNavCases.Case1;
            var navGrid = NavGridProvider.FromBitmap(navCase.Bitmap);
            var graph = new Graph(navGrid);

            var nodeSelector = new DefaultNextNodeSelector(navCase.Settings);
            var explorer = new GraphMapExplorer(navCase.Settings, graph, nodeSelector);
            var playerPos = new Vector2(navCase.StartPoint.X, navCase.StartPoint.Y);
            explorer.ProcessSegmentation(playerPos);
            var result = new List<Node>();

            graph.Nodes.Count.ShouldNotBe(0);

            #region The part that be implemented in game/bot side:
            //We just simply do explorer.Update(currentPlayerPos);
            //And get the next point to go from explorer.NextRunNode if explorer.HasLocation is true

            var curPlayerNode = graph.Nodes.First();

            do
            {
                result.Add(curPlayerNode);
                explorer.Update(curPlayerNode.GridPos);
                curPlayerNode = explorer.NextRunNode;
            } while (explorer.HasLocation);

            #endregion

            result.Count.ShouldNotBe(0);
        }

        [Fact]
        public void SegmentatorTest()
        {
            var navCase = InputNavCases.Case1;
            var navGrid = NavGridProvider.FromBitmap(navCase.Bitmap);
            var segmentator = new NavGridSegmentator(navGrid, navCase.Settings);
            var graph = new Graph(navGrid);
            segmentator.Process(navCase.StartPoint, graph);
            NavGridOptimizer.OptimizeGraph(graph, 300);
            //GraphDebugDrawUtils.DumpBitmap(graph, "DumpBitmap_Opt.png");
        }

        [Fact]
        public void NavGridProvider_Returns_Non_Null_2D_Array_With_Correct_Dimensions()
        {
            // Arrange
            var inputBitmap = Resources.InputNav_01;

            // Act
            var navGrid = NavGridProvider.FromBitmap(inputBitmap);

            // Assert
            navGrid.ShouldNotBeNull();
            navGrid.Height.ShouldBe(inputBitmap.Height);
            navGrid.Width.ShouldBe(inputBitmap.Width);
            navGrid.WalkArray.ShouldNotBeNull();
            navGrid.WalkArray.GetLength(0).ShouldBe(inputBitmap.Width);
            navGrid.WalkArray.GetLength(1).ShouldBe(inputBitmap.Height);
        }
    }
}
