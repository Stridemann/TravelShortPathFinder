using TravelShortPathFinder.Algorithm.Data;

namespace TravelShortPathFinder.Algorithm.Logic
{
    public static class GraphPathFinder
    {
        /// <summary>
        /// Calculates the path between two nodes of one graph using BFS algorithm.
        /// "to" node is the last node in result list. "from" node is not added to list.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static List<Node>? FindPath(Node from, Node to)
        {
            var result = new List<Node>();

            if (from == to)
                return result;
            
            var queue = new Queue<Node>();

            Node.ProcessIteration++;
            from.SetProcessed();
            queue.Enqueue(from);

            while (queue.TryDequeue(out var node))
            {
                foreach (var childNode in node.Links)
                {
                    if (childNode.IsProcessed)
                        continue;
                    childNode.SetProcessed();

                    childNode.FindPathPreviousNode = node;

                    if (childNode == to)
                    {
                        var prevNode = to;

                        while (prevNode != from)
                        {
                            result.Insert(0, prevNode);
                            prevNode = prevNode.FindPathPreviousNode;
                        }
                        
                        return result;
                    }

                    queue.Enqueue(childNode);
                }
            }

            return null;
        }
    }
}
