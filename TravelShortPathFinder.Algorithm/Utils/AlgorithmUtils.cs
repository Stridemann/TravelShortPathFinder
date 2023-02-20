namespace TravelShortPathFinder.Algorithm.Utils
{
    using System.Drawing;
    using Data;
    using Interfaces;
    using Logic;

    public static class AlgorithmUtils
    {
        public static List<Node> GetShortestPath(
            NavGrid navGrid,
            Point startPoint,
            Settings settings,
            INextNodeSelector? nodeSelector = null)
        {
            nodeSelector ??= new DefaultNextNodeSelector(settings);
            var explorer = new GraphMapExplorer(navGrid, settings, nodeSelector);
            explorer.ProcessSegmentation(startPoint);
            var result = new List<Node>();

            if (explorer.Graph.Nodes.Count == 0)
            {
                return result;
            }

            var curPlayerNode = explorer.Graph.Nodes.First();

            do
            {
                result.Add(curPlayerNode);
                explorer.Update(curPlayerNode.Pos);
                curPlayerNode = explorer.NextRunNode;
            } while (curPlayerNode != null);

            return result;
        }
    }
}
