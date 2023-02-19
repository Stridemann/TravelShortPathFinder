namespace TravelShortPathFinder.Algorithm.Utils
{
    using System.Numerics;
    using Data;
    using Interfaces;
    using Logic;

    public static class AlgorithmUtils
    {
        public static List<Node> GetShortestPath(
            NavGrid navGrid,
            Vector2 startPoint,
            Settings settings,
            INextNodeSelector? nodeSelector = null)
        {
            var graph = new Graph(navGrid);
            nodeSelector ??= new DefaultNextNodeSelector(settings);
            var explorer = new GraphMapExplorer(settings, graph, nodeSelector);
            explorer.ProcessSegmentation(startPoint);
            var result = new List<Node>();

            if (graph.Nodes.Count == 0)
            {
                return result;
            }

            var curPlayerNode = graph.Nodes.First();

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
