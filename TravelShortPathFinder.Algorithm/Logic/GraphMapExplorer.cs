namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;
    using Data;
    using Interfaces;
    using Utils;

    public class GraphMapExplorer
    {
        private readonly Settings _settings;
        private readonly INextNodeSelector _nodeSelector;
        private readonly List<GraphPart> _graphParts = new List<GraphPart>();
        private readonly NavGridSegmentator _segmentator;
        private int _passedNodesCount;
        private Point _playerCachedPos;

        public GraphMapExplorer(NavGrid navGrid, Settings settings, INextNodeSelector? nodeSelector = null)
        {
            _settings = settings;
            Graph = new Graph(navGrid);
            _nodeSelector = nodeSelector ?? new DefaultNextNodeSelector(settings);
            _segmentator = new NavGridSegmentator(navGrid, _settings);
        }

        public Graph Graph { get; }
        private IReadOnlyList<GraphPart> GraphParts => _graphParts;
        public Node? NextRunNode { get; private set; }
        public bool HasLocation => NextRunNode != null;
        public float PercentComplete => (float)_passedNodesCount / Graph.Nodes.Count * 100;

        public void Update(Point playerPos)
        {
            if (!(_playerCachedPos.Distance(playerPos) > _settings.OptimizationMoveDist)
                && NextRunNode is { IsVisited: false, Unwalkable: false }
                && !(NextRunNode.Pos.Distance(playerPos) < _settings.PlayerVisibilityRadius / 2))
            {
                return;
            }

            _playerCachedPos = playerPos;
            var seenNodes = new List<Node>();

            foreach (var node in Graph.Nodes)
            {
                if (node.IsVisited
                    || node.Unwalkable
                    || playerPos.Distance(node.Pos) > _settings.PlayerVisibilityRadius)
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
            Graph.Nodes.ForEach(x => x.Unwalkable = false);
        }

        /// <summary>
        /// Start segmentation from specified Point.
        /// </summary>
        /// <param name="gridPos">This can be player start grid coord</param>
        public void ProcessSegmentation(Point gridPos)
        {
            _passedNodesCount = 0;

            _graphParts.Clear();
            _playerCachedPos = gridPos;
            NextRunNode = null;

            StartSegmentation(gridPos);
        }

        /// <summary>
        /// Can be called when the player teleported to a new non-segmentated area of map
        /// </summary>
        /// <param name="gridCell"></param>
        public void CheckSegmentation(Point gridCell)
        {
            StartSegmentation(gridCell);
        }

        private void StartSegmentation(Point segmentationStartPoint)
        {
            _segmentator.Process(segmentationStartPoint, Graph);
            NavGridOptimizer.OptimizeGraph(Graph, _settings.SegmentationMinSegmentSize);

            if (Graph.Nodes.Count == 0)
            {
                return;
            }

            Node? endNode = null;

            //Start node from segmentationStartPoint pos is always first.
            var startPoint = Graph.Nodes[0];

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
                        }
                    }

                    return true;
                });
        }

        private void NextRunNodeFromSeenNodes(IReadOnlyCollection<Node> seenNodes, Point playerPos)
        {
            NextRunNode = null;
            _passedNodesCount += seenNodes.Count;

            GraphPart.GraphPartIteration++;

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
                    if (node.GraphPart == null || (node.GraphPart.IsGroupProcessed && !node.GraphExplorerProcessed))
                    {
                        var newGroup = new GraphPart(GraphPart.GraphPartIteration);

                        BreadthFirstSearch.Process(
                            node,
                            procNode =>
                            {
                                if (procNode is { IsVisited: false, Unwalkable: false, GraphExplorerProcessed: false })
                                {
                                    //LogMessage($"New node: {procNode.Id} for new group: {newGroup.GroupId}");
                                    procNode.GraphPart = newGroup;
                                    procNode.SetGraphExplorerProcessed();
                                    newGroup.Nodes.Add(procNode);

                                    return true;
                                }

                                return false;
                            });

                        newGroup.AveragePos = node.Pos;

                        node.SetGraphExplorerProcessed();
                        node.GraphPart = newGroup;
                        _graphParts.Add(newGroup);
                    }
                    else if (!node.GraphPart.IsGroupProcessed)
                    {
                        var group = node.GraphPart;

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

                                    //avrgPos += procNode.Pos;
                                    return true;
                                }

                                return false;
                            });

                        if (group.NodesCount == 0)
                        {
                            //LogMessage($"Removing group {group.GroupId}.");
                            _graphParts.Remove(group);
                        }
                        else
                        {
                            node.SetGraphExplorerProcessed();
                            group.SetGraphExplorerProcessed();
                        }
                    }
                }

                for (var i = 0; i < _graphParts.Count; i++)
                {
                    var seenNodesGroup = _graphParts[i];
                    seenNodesGroup.Nodes.RemoveAll(x => x.IsVisited);

                    if (seenNodesGroup.NodesCount == 0)
                    {
                        _graphParts.RemoveAt(i);
                        i--;
                    }
                }
            }

            if (_graphParts.Count > 0)
            {
                if (seenNodes.Count > 0 || NextRunNode == null || NextRunNode.IsVisited || NextRunNode.Unwalkable)
                {
                    NextRunNode = _nodeSelector.SelectNextNode(playerPos, _graphParts);
                }
            }
        }
    }
}
