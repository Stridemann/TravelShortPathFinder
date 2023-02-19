using System.Numerics;
using TravelShortPathFinder.Algorithm.Data;

namespace TravelShortPathFinder.Algorithm.Interfaces
{
    public interface INextNodeSelector
    {
        Node? SelectNextNode(Vector2 playerPos, List<GraphPart> graphParts);
    }
}
