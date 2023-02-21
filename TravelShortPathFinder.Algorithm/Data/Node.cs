namespace TravelShortPathFinder.Algorithm.Data
{
    using System.Diagnostics;
    using System.Drawing;

    [DebuggerDisplay("Node Id:{Id}, IsVisited: {IsVisited}, Links: {Links.Count}")]
    public class Node
    {
        internal readonly Stack<Point> Stack = new Stack<Point>();
        public readonly Point Pos;
        internal readonly Stack<Point> PossibleSegments = new Stack<Point>();
        internal readonly List<Node> PossibleLinks = new List<Node>();
        public readonly int Id;

        private readonly List<Node> _links = new List<Node>();
        internal static int UniqIdCounter;
        public int PriorityFromEndDistance;
        public int Square;
        internal Node? FindPathPreviousNode;

        public Node(int id, Point pos)
        {
            Id = id;
            Pos = pos;
        }

        public IReadOnlyList<Node> Links => _links;
        public bool IsRemovedByOptimizer { get; internal set; }

        public void LinkWith(Node node)
        {
            if (!Links.Contains(node))
            {
                _links.Add(node);
            }

            if (!node.Links.Contains(this))
            {
                node._links.Add(this);
            }
        }

        public int RemoveAllLinksToNode(Node node)
        {
            return _links.RemoveAll(x => x == node);
        }

        #region IsProcessed

        internal bool IsProcessed => _curProcessIter == ProcessIteration;
        internal static int ProcessIteration;
        private int _curProcessIter = -1;

        internal void SetProcessed()
        {
            _curProcessIter = ProcessIteration;
        }

        #endregion

        #region Node

        public GraphPart? GraphPart { get; internal set; }
        public bool Unwalkable { get; set; }
        public bool IsVisited { get; internal set; }
        internal int GraphExplorerIteration = -1;
        internal bool GraphExplorerProcessed => GraphExplorerIteration == GraphPart.GraphPartIteration;

        internal void SetGraphExplorerProcessed()
        {
            GraphExplorerIteration = GraphPart.GraphPartIteration;
        }

        #endregion

        internal bool IsLinkedTo(Node node)
        {
            if (Links.Contains(node))
                return true;

            foreach (var link in Links)
            {
                if (link == node)
                    return true;

                if (link.Links.Contains(node))
                    return true;
            }

            return false;
        }
    }
}
