#pragma warning disable CA1416
namespace TravelShortPathFinder.Tests
{
    using System.Drawing;
    using Algorithm.Data;
    using Algorithm.Logic;
    using Shouldly;
    using TestResources;
    using TestResources.Properties;

    public class AlgorithmTest
    {
        private void DumpBitmap(Graph graph, string fileName)
        {
            var bitmap = new Bitmap(graph.NavGrid.Width, graph.NavGrid.Height);

            for (var x = 0; x < graph.NavGrid.Width; x++)
            {
                for (var y = 0; y < graph.NavGrid.Height; y++)
                {
                    var gridVal = graph.NavGrid.WalkArray[x, y];

                    if ((gridVal & WalkableFlag.Nonwalkable) != 0)
                    {
                        bitmap.SetPixel(x, y, Color.Black);
                    }
                    else if (gridVal == WalkableFlag.FailedCenter)
                    {
                        bitmap.SetPixel(x, y, Color.Red);
                    }
                    else if (gridVal == WalkableFlag.SectorCenter)
                    {
                        bitmap.SetPixel(x, y, Color.Green);
                    }
                    else if (gridVal == WalkableFlag.Walkable)
                    {
                        bitmap.SetPixel(x, y, Color.LightGray);
                    }
                    else
                    {
                        var node = graph.MapSegmentMatrix[x, y];
                        var seed = node.Id;
                        var rand = new Random(seed);
                        var randomColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                        bitmap.SetPixel(x, y, randomColor);
                    }
                }
            }

            using var g = Graphics.FromImage(bitmap);

            foreach (var node in graph.Nodes)
            {
                foreach (var link in node.Links)
                {
                    g.DrawLine(
                        Pens.White,
                        node.BoundingCenter.X,
                        node.BoundingCenter.Y,
                        link.BoundingCenter.X,
                        link.BoundingCenter.Y);
                }
            }

            bitmap.Save(fileName);
        }

        [Fact]
        public void MainTest()
        {
            var navCase = InputNavCases.Case1;
            var navGrid = NavGridProvider.FromBitmap(navCase.Bitmap);
            var segmentator = new NavGridSegmentator(navGrid, new Settings());
            var graph = new Graph(navGrid);
            segmentator.Process(navCase.StartPoint, graph);
            NavGridOptimizer.OptimizeGraph(graph, 300);
            DumpBitmap(graph, "DumpBitmap_Opt.png");
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
