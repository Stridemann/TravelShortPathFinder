namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Numerics;
    using Data;
    using Interfaces;

    public class DefaultNextNodeSelector : INextNodeSelector
    {
        private readonly Settings _settings;

        public DefaultNextNodeSelector(Settings settings)
        {
            _settings = settings;
        }

        public Node? SelectNextNode(Vector2 playerPos, List<GraphPart> graphParts)
        {
            //We gonna chose the smallest group to run.
            //Usually this is the best strategy
            var bestGroup = graphParts.Where(x => x.Nodes.Any(y => !y.Unwalkable))
                                      .OrderBy(x => x.Nodes.Count(y => !y.Unwalkable))
                                      .ThenBy(x => Vector2.Distance(playerPos, x.AveragePos))
                                      .First();

            //We gonna run the farthest couple of nodes from end (with a range of LocalSelectNearNodeRange)
            //Then chose the closest point to player from that group
            return bestGroup
                   .Nodes.Where(x => !x.Unwalkable)
                   .OrderByDescending(x => x.PriorityFromEndDistance / _settings.LocalSelectNearNodeRange)
                   .ThenBy(x => Vector2.Distance(playerPos, x.GridPos))
                   .FirstOrDefault();
        }
    }
}
