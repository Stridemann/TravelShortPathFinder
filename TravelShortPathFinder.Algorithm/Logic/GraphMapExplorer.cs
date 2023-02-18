namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;
    using System.Numerics;
    using BenchmarkDotNet.Attributes;
    using Data;
    using Utils;

    public class GraphMapExplorer
    {
        private const float UPDATE_DIST = 25;

        private readonly Settings _settings;
        public List<SeenNodesGroup> GroupsList { get; } = new List<SeenNodesGroup>();
        private readonly Graph _graph;
        private readonly NavGrid _navGrid;
        private NavGridSegmentator _segmentator;
        private int _passedNodesCount;
        private Vector2 _playerCachedPos;

        public GraphMapExplorer(Settings settings, Graph graph)
        {
            _settings = settings;
            _navGrid = graph.NavGrid;
            _graph = graph;
        }

        public Node? CurrentRunNode { get; private set; }
        public Node NextRunNode => CurrentRunNode;
        public bool HasLocation => CurrentRunNode != null;
        public float PercentComplete => (float)_passedNodesCount / _graph.Nodes.Count * 100;
        
        public void Update(Vector2 playerPos)
        {
            if (!(Vector2.Distance(_playerCachedPos, playerPos) > UPDATE_DIST)
                && CurrentRunNode is { IsVisited: false, Unwalkable: false }
                && !(Vector2.Distance(CurrentRunNode.GridPos, playerPos) < _settings.ExploreNodeProcessRadius / 2))
            {
                return;
            }

            _playerCachedPos = playerPos;
            var seenNodes = new List<Node>();

            foreach (var node in _graph.Nodes)
            {
                if (node.IsVisited
                    || node.Unwalkable
                    || Vector2.Distance(playerPos, node.GridPos) > _settings.ExploreNodeProcessRadius)
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

            GroupsList.Clear();
            _playerCachedPos = playerPos;
            CurrentRunNode = null;

            _segmentator = new NavGridSegmentator(_navGrid, _settings);
            StartSegmentation(new Point((int)playerPos.X, (int)playerPos.Y));
        }

        public void UpdateForTriggerableBlockage(Point gridCell)
        {
            StartSegmentation(gridCell);
        }

        private void StartSegmentation(Point segmentationStartPoint)
        {
            _segmentator.Process(segmentationStartPoint, _graph);
            NavGridOptimizer.OptimizeGraph(_graph, _settings.SegmentationMinSegmentSize);

            if (_graph.Nodes.Count == 0)
            {
                return;
            }

            Node endNode = null;

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
                        }
                    }

                    return true;
                });
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
                        var newGroup = new SeenNodesGroup(SeenNodesGroup.DfsIteration);

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
                        GroupsList.Add(newGroup);
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
                            GroupsList.Remove(group);
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
            }

            if (GroupsList.Count > 0)
            {
                if (seenNodes.Count > 0 || CurrentRunNode == null || CurrentRunNode.IsVisited || CurrentRunNode.Unwalkable)
                {
                    for (var i = 0; i < GroupsList.Count; i++)
                    {
                        var seenNodesGroup = GroupsList[i];
                        seenNodesGroup.Nodes.RemoveAll(x => x.IsVisited);

                        if (seenNodesGroup.NodesCount == 0)
                        {
                            GroupsList.RemoveAt(i);
                            i--;
                        }
                    }

                    if (GroupsList.Count > 0)
                    {
                        var bestGroup = GroupsList.Where(x => x.Nodes.Any(y => !y.Unwalkable))
                                                  .OrderBy(x => x.Nodes.Count(y => !y.Unwalkable))
                                                  .ThenBy(x => Vector2.Distance(playerPos, x.AveragePos))
                                                  .First();

                        CurrentRunNode = bestGroup
                                         .Nodes.Where(x => !x.Unwalkable)
                                         .OrderByDescending(x => x.PriorityFromEndDistance / _settings.LocalSelectNearNodeRange)
                                         .ThenBy(x => Vector2.Distance(playerPos, x.GridPos))
                                         .FirstOrDefault();
                    }
                }
            }
        }
    }
}
