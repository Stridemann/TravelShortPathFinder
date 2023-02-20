namespace TravelShortPathFinder.Algorithm.Data
{
    using System.Diagnostics;
    using System.Drawing;

    [DebuggerDisplay("Node Id:{Id}, IsVisited: {IsVisited}, Links: {Links.Count}")]
    public class Node
    {
        public readonly Stack<Point> Stack = new Stack<Point>();
        public readonly Point Pos;
        public readonly Stack<Point> PossibleSegments = new Stack<Point>();
        public readonly List<Node> PossibleLinks = new List<Node>();
        public readonly int Id;

        private readonly List<Node> _links = new List<Node>();
        public static int UniqIdCounter;
        public Point SegmentMin = new Point(int.MaxValue, int.MaxValue);
        public Point SegmentMax = new Point(int.MinValue, int.MinValue);
        public int PriorityFromEndDistance;
        public int Square;
        internal Node? FindPathPreviousNode;

        public Node(int id, Point pos)
        {
            Id = id;
            Pos = pos;
        }

        public IReadOnlyList<Node> Links => _links;
        public Point BoundingCenter { get; private set; }
        public bool IsRemovedByOptimizer { get; internal set; }

        public void UpdateBoundingCenter()
        {
            BoundingCenter = new Point((SegmentMax.X + SegmentMin.X) / 2, (SegmentMax.Y + SegmentMin.Y) / 2);
        }

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

        public GraphPart? Group { get; internal set; }
        public bool Unwalkable { get; set; }
        public bool IsVisited { get; internal set; }
        internal int GraphExplorerIteration = -1;
        internal bool GraphExplorerProcessed => GraphExplorerIteration == GraphPart.DfsIteration;

        internal void SetGraphExplorerProcessed()
        {
            GraphExplorerIteration = GraphPart.DfsIteration;
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
