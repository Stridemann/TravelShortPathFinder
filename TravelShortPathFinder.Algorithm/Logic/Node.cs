namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Diagnostics;
    using System.Drawing;
    using System.Numerics;
    using Data;

    [DebuggerDisplay("Node Id:{Id}, IsVisited: {IsVisited}, Links: {Links.Count}")]
    public class Node
    {
        public readonly Stack<Point> Stack = new Stack<Point>();
        public readonly Point Pos;
        public readonly Stack<Point> PossibleSectors = new Stack<Point>();
        public readonly List<Node> PossibleLinks = new List<Node>();
        public readonly int Id;

        private readonly List<Node> _links = new List<Node>();
        public static int UniqIdCounter;
        public Point Min = new Point(int.MaxValue, int.MaxValue);
        public Point Max = new Point(int.MinValue, int.MinValue);
        public int Square;

        public Node(int id, Point pos)
        {
            Id = id;
            Pos = pos;
        }

        public IReadOnlyList<Node> Links => _links;

        public Point BoundingCenter { get; private set; }

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

        public bool Unwalkable { get; set; }
        public SeenNodesGroup? Group;
        public Vector2 GridPos; //TODO: Check who set

        public bool IsVisited { get; set; }

        public uint GraphExplorerIteration = uint.MaxValue;
        public bool GraphExplorerProcessed => GraphExplorerIteration == SeenNodesGroup.DfsIteration;

        public void SetGraphExplorerProcessed()
        {
            GraphExplorerIteration = SeenNodesGroup.DfsIteration;
        }

        #endregion
    }
}
