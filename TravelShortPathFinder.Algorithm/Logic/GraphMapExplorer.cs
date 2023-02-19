namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;
    using System.Numerics;
    using Data;
    using TravelShortPathFinder.Algorithm.Interfaces;
    using Utils;

    public class GraphMapExplorer
    {
        private const float UPDATE_DIST = 25;
        private readonly Settings _settings;
        private readonly Graph _graph;
        private int _passedNodesCount;
        private Vector2 _playerCachedPos;
        private readonly INextNodeSelector _nodeSelector;

        public GraphMapExplorer(Settings settings, Graph graph, INextNodeSelector nodeSelector)
        {
            _settings = settings;
            _graph = graph;
            _nodeSelector = nodeSelector;
            Segmentator = new NavGridSegmentator(graph.NavGrid, _settings);
        }

        public List<GraphPart> GraphParts { get; } = new List<GraphPart>();
        public NavGridSegmentator Segmentator { get; }

        public Node? NextRunNode { get; private set; }
        public bool HasLocation => NextRunNode != null;
        public float PercentComplete => (float)_passedNodesCount / _graph.Nodes.Count * 100;

        public void Update(Vector2 playerPos)
        {
            if (!(Vector2.Distance(_playerCachedPos, playerPos) > UPDATE_DIST)
                && NextRunNode is { IsVisited: false, Unwalkable: false }
                && !(Vector2.Distance(NextRunNode.GridPos, playerPos) < _settings.PlayerVisibilityRadius / 2))
            {
                return;
            }

            _playerCachedPos = playerPos;
            var seenNodes = new List<Node>();

            foreach (var node in _graph.Nodes)
            {
                if (node.IsVisited
                    || node.Unwalkable
                    || Vector2.Distance(playerPos, node.GridPos) > _settings.PlayerVisibilityRadius)
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
            _graph.Nodes.ForEach(x => x.Unwalkable = false);
        }

        public void ProcessSegmentation(Vector2 playerPos)
        {
            _passedNodesCount = 0;

            GraphParts.Clear();
            _playerCachedPos = playerPos;
            NextRunNode = null;

            StartSegmentation(new Point((int)playerPos.X, (int)playerPos.Y));
        }

        public void UpdateForTriggerableBlockage(Point gridCell)
        {
            StartSegmentation(gridCell);
        }

        private void StartSegmentation(Point segmentationStartPoint)
        {
            Segmentator.Process(segmentationStartPoint, _graph);
            NavGridOptimizer.OptimizeGraph(_graph, _settings.SegmentationMinSegmentSize);

            if (_graph.Nodes.Count == 0)
            {
                return;
            }

            Node? endNode = null;

            //Start node from segmentationStartPoint pos is always first.
            var startPoint = _graph.Nodes[0];

            //Find the farthest node from start position
            BreadthFirstSearch.Process(
                startPoint,
                procNode =>
                {
                    if (procNode is { Unwalkable: false })
                    {
                        endNode = procNode;

                        return true;
                    }

                    return false;
                });

            endNode!.PriorityFromEndDistance = 1;

            //Calculate and set a priority distance from end node to all another.
            //We will try to run most farthest nodes from end. That's the code idea of algo
            BreadthFirstSearch.Process(
                endNode,
                procNode =>
                {
                    foreach (var node in procNode.Links)
                    {
                        if (node.PriorityFromEndDistance == 0)
                        {
                            node.PriorityFromEndDistance = procNode.PriorityFromEndDistance + 1;
                            MapPriorityUpdated();
                        }
                    }

                    return true;
                });
        }

        private void NextRunNodeFromSeenNodes(IReadOnlyCollection<Node> seenNodes, Vector2 playerPos)
        {
            NextRunNode = null;
            _passedNodesCount += seenNodes.Count;

            GraphPart.DfsIteration++;

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
                        var newGroup = new GraphPart(GraphPart.DfsIteration);

                        //var avrgPos = Vector2.Zero;
                        var lastNode = node;

                        BreadthFirstSearch.Process(
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

                        //as alternative can be:
                        //newGroup.AveragePos = avrgPos / newGroup.NodesCount;
                        newGroup.AveragePos = lastNode.GridPos;
                        node.SetGraphExplorerProcessed();
                        node.Group = newGroup;
                        GraphParts.Add(newGroup);
                    }
                    else if (!node.Group.IsGroupProcessed)
                    {
                        var group = node.Group;

                        //var nodesBefore = group.Nodes.Count;
                        group.Nodes.Clear();

                        // var avrgPos = Vector2.Zero;
                        BreadthFirstSearch.Process(
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
                            GraphParts.Remove(group);
                        }
                        else
                        {
                            node.SetGraphExplorerProcessed();
                            group.SetGraphExplorerProcessed();
                            //Optionally AveragePos can be updated here:
                            //group.AveragePos = avrgPos / group.NodesCount;
                        }
                    }
                }

                for (var i = 0; i < GraphParts.Count; i++)
                {
                    var seenNodesGroup = GraphParts[i];
                    seenNodesGroup.Nodes.RemoveAll(x => x.IsVisited);

                    if (seenNodesGroup.NodesCount == 0)
                    {
                        GraphParts.RemoveAt(i);
                        i--;
                    }
                }
            }

            if (GraphParts.Count > 0)
            {
                if (seenNodes.Count > 0 || NextRunNode == null || NextRunNode.IsVisited || NextRunNode.Unwalkable)
                {
                    if (GraphParts.Count > 0)
                    {
                        NextRunNode = _nodeSelector.SelectNextNode(playerPos, GraphParts);
                    }
                }
            }
        }

        public event Action MapPriorityUpdated = delegate { };
    }
}
