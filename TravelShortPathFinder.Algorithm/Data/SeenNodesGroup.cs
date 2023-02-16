using TravelShortPathFinder.Algorithm.Logic;

namespace TravelShortPathFinder.Algorithm.Data
{
    using System.Numerics;

    public class SeenNodesGroup
    {
        public static uint DfsIteration;
        public Vector2 AveragePos { get; set; }
        public uint GraphExplorerIteration;
        public List<Node> Nodes = new List<Node>();
        public int NodesCount => Nodes.Count;
        public bool IsGroupProcessed => GraphExplorerIteration == DfsIteration;

        public void SetGraphExplorerProcessed()
        {
            GraphExplorerIteration = DfsIteration;
        }
    }
}
