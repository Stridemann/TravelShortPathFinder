namespace TravelShortPathFinder.Algorithm.Interfaces
{
    using System.Drawing;
    using Data;

    public interface INextNodeSelector
    {
        Node? SelectNextNode(Point playerPos, List<GraphPart> graphParts);
    }
}
