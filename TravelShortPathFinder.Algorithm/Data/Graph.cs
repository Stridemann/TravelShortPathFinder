namespace TravelShortPathFinder.Algorithm.Data
{
    using Logic;

    public class Graph
    {
        public Graph(NavGrid navGrid)
        {
            NavGrid = navGrid;
            MapSegmentMatrix = new Node[navGrid.Width, navGrid.Height];
        }

        public Node?[,] MapSegmentMatrix { get; }
        public List<Node> Nodes { get; } = new List<Node>();
        public NavGrid NavGrid { get; }
    }
}
