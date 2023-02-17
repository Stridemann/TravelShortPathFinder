namespace TravelShortPathFinder.Algorithm.Data
{
    using System.Numerics;

    public class SeenNodesGroup
    {
        public static int DfsIteration;
        public int GraphExplorerIteration = -1;
        public List<Node> Nodes = new List<Node>();

        public SeenNodesGroup(int id)
        {
            Id = id;
        }

        public int Id { get; }
        public Vector2 AveragePos { get; set; }

        public int NodesCount => Nodes.Count;
        public bool IsGroupProcessed => GraphExplorerIteration == DfsIteration;

        public void SetGraphExplorerProcessed()
        {
            GraphExplorerIteration = DfsIteration;
        }
    }
}
