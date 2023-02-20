namespace TravelShortPathFinder.Algorithm.Data
{
    using System.Drawing;

    public class GraphPart
    {
        public static int DfsIteration;
        public int GraphExplorerIteration = -1;
        public List<Node> Nodes = new List<Node>();

        public GraphPart(int id)
        {
            Id = id;
        }

        public int Id { get; }
        public Point AveragePos { get; set; }
        public int NodesCount => Nodes.Count;
        public bool IsGroupProcessed => GraphExplorerIteration == DfsIteration;

        public void SetGraphExplorerProcessed()
        {
            GraphExplorerIteration = DfsIteration;
        }
    }
}
