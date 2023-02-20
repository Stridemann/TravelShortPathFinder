namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;
    using Data;
    using Interfaces;
    using Utils;

    public class DefaultNextNodeSelector : INextNodeSelector
    {
        private readonly Settings _settings;

        public DefaultNextNodeSelector(Settings settings)
        {
            _settings = settings;
        }

        public Node? SelectNextNode(Point playerPos, List<GraphPart> graphParts)
        {
            //We gonna chose the smallest group to run.
            //Usually this is the best strategy
            var bestGroup = graphParts.Where(x => x.Nodes.Any(y => !y.Unwalkable))
                                      .OrderBy(x => x.Nodes.Count(y => !y.Unwalkable))
                                      .ThenBy(x => playerPos.Distance(x.AveragePos))
                                      .First();

            //testing better way. This will reduce computational resources for pathfinding coz try to select the nearest node
            return bestGroup
                   .Nodes.Where(x => !x.Unwalkable)
                   .OrderBy(x => (int)(playerPos.Distance(x.Pos) / 30))
                   .ThenByDescending(x => x.PriorityFromEndDistance / _settings.LocalSelectNearNodeRange)
                   .FirstOrDefault();

            //We gonna run the farthest couple of nodes from end (with a range of LocalSelectNearNodeRange)
            //Then chose the closest point to player from that group
            return bestGroup
                   .Nodes.Where(x => !x.Unwalkable)
                   .OrderByDescending(x => x.PriorityFromEndDistance / _settings.LocalSelectNearNodeRange)
                   .ThenBy(x => playerPos.Distance(x.Pos))
                   .FirstOrDefault();
        }
    }
}
