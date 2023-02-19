namespace TravelShortPathFinder.Algorithm.Data
{
    using System.Diagnostics;
    using System.Drawing;
    using System.Numerics;

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
        public Point Min = new Point(int.MaxValue, int.MaxValue);
        public Point Max = new Point(int.MinValue, int.MinValue);
        public int PriorityFromEndDistance;
        public int Square;

        public Node(int id, Point pos)
        {
            Id = id;
            Pos = pos;
            GridPos = new Vector2(pos.X, pos.Y);
        }

        public IReadOnlyList<Node> Links => _links;
        public Point BoundingCenter { get; private set; }
        public bool IsRemovedByOptimizer { get; set; }

        public void UpdateBoundingCenter()
        {
            BoundingCenter = new Point((Max.X + Min.X) / 2, (Max.Y + Min.Y) / 2);
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

        public bool IsProcessed => _curProcessIter == ProcessIteration;
        public static int ProcessIteration;
        private int _curProcessIter = -1;

        public void SetProcessed()
        {
            _curProcessIter = ProcessIteration;
        }

        #endregion

        #region Node

        public GraphPart? Group;
        public readonly Vector2 GridPos;
        public bool Unwalkable { get; set; }
        public bool IsVisited { get; set; }
        public int GraphExplorerIteration = -1;
        public bool GraphExplorerProcessed => GraphExplorerIteration == GraphPart.DfsIteration;

        public void SetGraphExplorerProcessed()
        {
            GraphExplorerIteration = GraphPart.DfsIteration;
        }

        #endregion

        public bool IsLinkedTo(Node node)
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
