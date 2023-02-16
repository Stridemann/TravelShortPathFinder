namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;
    using System.Numerics;
    using Data;
    using Utils;

    public class GraphMapExplorer
    {
        private const float UPDATE_DIST = 25;

        private readonly Settings _settings;
        private readonly List<SeenNodesGroup> _groupsList = new List<SeenNodesGroup>();
        private Graph _graph;

        private NavGridSegmentator _segmentator;
        private int _passedNodesCount;
        private Vector2 _playerCachedPos;
        private readonly NavGrid _navGrid;
        private readonly Node[,] _mapSegmentMatrix;

        public GraphMapExplorer(Settings settings, NavGrid navGrid)
        {
            _settings = settings;
            _navGrid = navGrid;
            _mapSegmentMatrix = new Node[navGrid.Width, navGrid.Height];
        }

        public Node? CurrentRunNode { get; private set; }
        public bool IsAreaSegmentated { get; private set; }

        public Vector2? NextRunPoint => CurrentRunNode?.GridPos;
        public bool HasLocation => CurrentRunNode != null;
        public float PercentComplete => (float)_passedNodesCount / _graph.Nodes.Count * 100;

        public void Update(Vector2 playerPos)
        {
            if (!(Vector2.Distance(_playerCachedPos, playerPos) > UPDATE_DIST)
                && CurrentRunNode is { IsVisited: false, Unwalkable: false }
                && !(Vector2.Distance(CurrentRunNode.GridPos, playerPos) < _settings.ExploreNodeProcessDist / 2))
            {
                return;
            }

            _playerCachedPos = playerPos;
            var seenNodes = new List<Node>();

            foreach (var node in _graph.Nodes)
            {
                if (node.IsVisited
                    || node.Unwalkable
                    || //not 100% sure
                    Vector2.Distance(playerPos, node.GridPos) > _settings.ExploreNodeProcessDist)
                {
                    continue;
                }

                node.IsVisited = true;
                seenNodes.Add(node);
            }

            NextRunNodeFromSeenNodes(seenNodes, playerPos);
        }

        public void ResetUnwalkable()
        {
            _graph.Nodes.ForEach(
                x => { x.Unwalkable = false; });
        }

        public void ProcessSegmentation(Vector2 playerPos)
        {
            _passedNodesCount = 0;
            IsAreaSegmentated = false;

            _groupsList.Clear();
            _playerCachedPos = playerPos;
            CurrentRunNode = null;

            _segmentator = new NavGridSegmentator(_navGrid, _settings);
            _graph = new Graph();
            _segmentator.Process(new Point((int)playerPos.X, (int)playerPos.Y), _graph, _mapSegmentMatrix);
       
            var optimizer = new NavGridOptimizer(_settings.SegmentationMinSegmentSize);
            optimizer.OptimizeGraph(_graph, _navGrid);
            
            IsAreaSegmentated = true;
        }

        public void UpdateForTriggerableBlockage(Vector2 gridPos)
        {
            _segmentator.Process(new Point((int)gridPos.X, (int)gridPos.Y), _graph, _mapSegmentMatrix);
        }

        private void NextRunNodeFromSeenNodes(IReadOnlyCollection<Node> seenNodes, Vector2 playerPos)
        {
            CurrentRunNode = null;
            _passedNodesCount += seenNodes.Count;

            SeenNodesGroup.DfsIteration++;

            foreach (var seenNode in seenNodes)
            {
                foreach (var seenNodeEdge in seenNode.Links)
                {
                    var node = seenNodeEdge;

                    if (node.IsVisited
                        || node.Unwalkable //not sure
                        || node.GraphExplorerProcessed)
                    {
                        continue;
                    }

                    //lost connection with a group - making a new group for it
                    if (node.Group == null || (node.Group.IsGroupProcessed && !node.GraphExplorerProcessed))
                    {
                        var newGroup = new SeenNodesGroup();

                        //var avrgPos = Vector2.Zero;
                        var lastNode = node;

                        Dfs.Process(
                            node,
                            procNode =>
                            {
                                if (procNode is { IsVisited: false, Unwalkable: false, GraphExplorerProcessed: false })
                                {
                                    //LogMessage($"New node: {procNode.Id} for new group: {newGroup.GroupId}");
                                    procNode.Group = newGroup;
                                    procNode.SetGraphExplorerProcessed();
                                    newGroup.Nodes.Add(procNode);

                                    //avrgPos += procNode.GridPos;

                                    lastNode = procNode;

                                    return true;
                                }

                                //LogMessage($"-New node: {procNode.Id} for new group: {newGroup.GroupId}. Visited?: {procNode.IsVisited}, Processed: {procNode.GraphExplorerProcessed}");
                                return false;
                            });

                        newGroup.AveragePos = lastNode.GridPos; // avrgPos / newGroup.NodesCount;
                        node.SetGraphExplorerProcessed();
                        node.Group = newGroup;
                        _groupsList.Add(newGroup);
                    }
                    else if (!node.Group.IsGroupProcessed)
                    {
                        var group = node.Group;

                        //var nodesBefore = group.Nodes.Count;
                        group.Nodes.Clear();

                        // var avrgPos = Vector2.Zero;
                        Dfs.Process(
                            node,
                            procNode =>
                            {
                                if (procNode is { IsVisited: false, GraphExplorerProcessed: false })
                                {
                                    //LogMessage($"Update node: {procNode.Id} for old group: {group.GroupId}");
                                    procNode.SetGraphExplorerProcessed();
                                    group.Nodes.Add(procNode);

                                    //avrgPos += procNode.GridPos;
                                    return true;
                                }

                                return false;
                            });

                        if (group.NodesCount == 0)
                        {
                            //LogError($"Removing group {group.GroupId}. Nodes: {group.NodesCount}. Before: {nodesBefore}");
                            _groupsList.Remove(group);
                        }
                        else
                        {
                            node.SetGraphExplorerProcessed();
                            group.SetGraphExplorerProcessed();

                            //group.AveragePos = avrgPos / group.NodesCount;
                        }
                    }
                }
            }

            if (_groupsList.Count > 0)
            {
                if (seenNodes.Count > 0 || CurrentRunNode == null || CurrentRunNode.IsVisited || CurrentRunNode.Unwalkable) //stuck fix
                {
                    foreach (var seenNodesGroup in _groupsList.ToList())
                    {
                        seenNodesGroup.Nodes = seenNodesGroup.Nodes.Where(x => !x.IsVisited).ToList();

                        if (seenNodesGroup.NodesCount == 0)
                        {
                            _groupsList.Remove(seenNodesGroup);
                        }
                    }

                    if (_groupsList.Count > 0)
                    {
                        CurrentRunNode = _groupsList.Where(x => x.Nodes.Any(y => !y.Unwalkable))
                                                    .OrderBy(x => x.Nodes.Count(y => !y.Unwalkable))
                                                    .ThenBy(x => Vector2.Distance(playerPos, x.AveragePos))
                                                    .First()
                                                    .Nodes.Where(x => !x.Unwalkable)
                                                    .OrderBy(x => (int)Vector2.Distance(playerPos, x.GridPos) / 150)
                                                    .ThenByDescending(x => Vector2.Distance(x.Group.AveragePos, x.GridPos))
                                                    .FirstOrDefault();
                    }
                }
            }
        }
    }
}
