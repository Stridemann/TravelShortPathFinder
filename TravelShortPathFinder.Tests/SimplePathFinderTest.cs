using TravelShortPathFinder.Algorithm.Logic;

namespace TravelShortPathFinder.Tests
{
    using System.Drawing;
    using Shouldly;
    using TravelShortPathFinder.Algorithm.Data;

    public class SimplePathFinderTest
    {
        [Fact]
        public void PathFinderTest()
        {
            var testGraph = new List<Node>(5);

            for (int i = 0; i < 5; i++)
            {
                var node = new Node(i, new Point());
                testGraph.Add(node);

                if (i > 0)
                    node.LinkWith(testGraph[i - 1]);
            }

            var from = testGraph[0];
            var to = testGraph[^1];
            var path = SimplePathFinder.FindPath(from, to);
            path.Count.ShouldBe(5);
            path[0].ShouldBe(from);
            path[^1].ShouldBe(to);
        }
    }
}
