namespace TravelShortPathFinder.Tests
{
    using System.Drawing;
    using Algorithm.Data;
    using Algorithm.Logic;
    using Shouldly;

    public class SimplePathFinderTest
    {
        [Fact]
        public void PathFinderTest()
        {
            var testGraph = new List<Node>(5);

            for (var i = 0; i < 5; i++)
            {
                var node = new Node(i, new Point());
                testGraph.Add(node);

                if (i > 0)
                {
                    node.LinkWith(testGraph[i - 1]);
                }
            }

            var from = testGraph[0];
            var to = testGraph[^1];
            var path = GraphPathFinder.FindPath(from, to);
            path.ShouldNotBeNull();
            path.Count.ShouldBe(4);
            path[^1].ShouldBe(to);
        }
    }
}
