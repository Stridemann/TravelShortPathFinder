namespace TravelShortPathFinder.Algorithm.Utils
{
    using System.Numerics;
    using Data;
    using Logic;

    public static class AlgorithmUtils
    {
        public static List<Node> GetShortestPath(NavGrid navGrid, Vector2 startPoint, Settings settings)
        {
            var graph = new Graph(navGrid);
            var explorer = new GraphMapExplorer(settings, graph);
            explorer.ProcessSegmentation(startPoint);
            var curPlayerNode = graph.Nodes.First();
            var result = new List<Node>();

            do
            {
                result.Add(curPlayerNode);
                explorer.Update(curPlayerNode.GridPos);
                curPlayerNode = explorer.NextRunNode;
            } while (explorer.HasLocation);

            return result;
        }
    }
}
