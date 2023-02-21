namespace TravelShortPathFinder.Algorithm.Data
{
    public class Graph
    {
        public Graph(NavGrid navGrid)
        {
            NavGrid = navGrid;
            MapSegmentMatrix = new Node[navGrid.Width, navGrid.Height];
        }

        /// <summary>
        /// The 2D grid array of segments. Filled after segmentation
        /// </summary>
        public Node?[,] MapSegmentMatrix { get; }
        public List<Node> Nodes { get; } = new List<Node>();
        public NavGrid NavGrid { get; }
    }
}
