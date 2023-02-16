namespace TravelShortPathFinder.Algorithm.Data
{
    using Logic;

    public class Graph
    {
        public readonly List<Node> Nodes;

        public Graph(List<Node> nodes)
        {
            Nodes = nodes;
        }
    }
}
