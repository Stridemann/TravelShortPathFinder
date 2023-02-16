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
                var mapSegment = graph.Nodes[sectorIndex];

                if (mapSegment.Square < _minSectorSqr)
                {
                    grid.WalkArray[mapSegment.Pos.X, mapSegment.Pos.Y] = WalkableFlag.FailedCenter;

                    for (var i = 0; i < mapSegment.Links.Count; i++)
                    {
                        var linkedSector = mapSegment.Links[i];

                        if (linkedSector.RemoveAllLinksToNode(mapSegment) == 0)
                        {
                            throw new InvalidOperationException($"Link to deleting sector {mapSegment.Id} was not present in {linkedSector.Id}");
                        }

                        for (var j = i + 1; j < mapSegment.Links.Count; j++)
                        {
                            var sector2 = mapSegment.Links[j];

                            if (linkedSector == sector2)
                                continue;

                            linkedSector.LinkWith(sector2);
                        }
                    }

                    graph.Nodes.RemoveAt(sectorIndex);
                    sectorIndex--;
                }
            }
        }
    }
}
