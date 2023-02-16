using TravelShortPathFinder.Algorithm.Data;

namespace TravelShortPathFinder.Algorithm.Logic
{
    public class NavGridOptimizer
    {
        private readonly int _minSectorSqr;

        public NavGridOptimizer(int minSectorSqr)
        {
            _minSectorSqr = minSectorSqr;
        }

        public void OptimizeGraph(Graph graph, NavGrid grid)
        {
            for (var sectorIndex = 0; sectorIndex < graph.Nodes.Count; sectorIndex++)
            {
                var node = graph.Nodes[sectorIndex];

                if (node.Square < _minSectorSqr)
                {
                    grid.WalkArray[node.Pos.X, node.Pos.Y] = WalkableFlag.FailedCenter;

                    //Here from all around nodes we delete all links to node we gonna delete
                    for (var i = 0; i < node.Links.Count; i++)
                    {
                        var deletingNodeLinkedNode = node.Links[i];

                        if (deletingNodeLinkedNode.RemoveAllLinksToNode(node) == 0)
                        {
                            throw new InvalidOperationException($"Link to deleting sector {node.Id} was not present in {deletingNodeLinkedNode.Id}");
                        }

                        //Also we link together all around nodes, since our node was linking all around nodes together
                        for (var j = i + 1; j < node.Links.Count; j++)
                        {
                            var nextLinkedNode = node.Links[j];

                            if (deletingNodeLinkedNode == nextLinkedNode)
                                continue;

                            deletingNodeLinkedNode.LinkWith(nextLinkedNode);
                        }
                    }

                    graph.Nodes.RemoveAt(sectorIndex);
                    sectorIndex--;
                }
            }
        }
    }
}
