namespace TravelShortPathFinder.Algorithm.Data
{
    using System.Drawing;

    public class GraphPart
    {
        public readonly List<Node> Nodes = new List<Node>();
        internal static int GraphPartIteration;
        private int _graphExplorerIteration = -1;

        public GraphPart(int id)
        {
            Id = id;
        }

        public int Id { get; }
        public Point AveragePos { get; set; }
        public int NodesCount => Nodes.Count;
        internal bool IsGroupProcessed => _graphExplorerIteration == GraphPartIteration;

        internal void SetGraphExplorerProcessed()
        {
            _graphExplorerIteration = GraphPartIteration;
        }
    }
}
