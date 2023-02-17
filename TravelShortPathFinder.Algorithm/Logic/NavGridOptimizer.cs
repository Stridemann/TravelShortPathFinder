using TravelShortPathFinder.Algorithm.Data;

namespace TravelShortPathFinder.Algorithm.Logic
{
    public static class NavGridOptimizer
    {
        public static void OptimizeGraph(Graph graph, int minSectorSqr)
        {
            for (var sectorIndex = 0; sectorIndex < graph.Nodes.Count; sectorIndex++)
            {
                var deletingNode = graph.Nodes[sectorIndex];

                if (deletingNode.Square < minSectorSqr)
                {
                    deletingNode.IsRemovedByOptimizer = true;
                    graph.NavGrid.WalkArray[deletingNode.Pos.X, deletingNode.Pos.Y] = WalkableFlag.FailedCenter;

                    //Here from all around nodes we delete all links to node we gonna delete
                    for (var i = 0; i < deletingNode.Links.Count; i++)
                    {
                        var deletingNodeLinkedNode = deletingNode.Links[i];

                        if (deletingNodeLinkedNode.RemoveAllLinksToNode(deletingNode) == 0)
                        {
                            throw new InvalidOperationException(
                                $"Link to deleting sector {deletingNode.Id} was not present in {deletingNodeLinkedNode.Id}");
                        }

                        //Also we link together all around nodes, since our node was linking all around nodes together
                        for (var j = i + 1; j < deletingNode.Links.Count; j++)
                        {
                            var nextLinkedNode = deletingNode.Links[j];

                            if (deletingNodeLinkedNode == nextLinkedNode)
                                continue;

                            if (!deletingNodeLinkedNode.IsLinkedTo(nextLinkedNode))
                            {
                                deletingNodeLinkedNode.LinkWith(nextLinkedNode);
                            }
                        }
                    }

                    graph.Nodes.RemoveAt(sectorIndex);
                    sectorIndex--;
                }
            }
        }
    }
}
