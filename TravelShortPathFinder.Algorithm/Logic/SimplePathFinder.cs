using TravelShortPathFinder.Algorithm.Data;

namespace TravelShortPathFinder.Algorithm.Logic
{
    public static class SimplePathFinder
    {
        public static List<Node>? FindPath(Node from, Node to)
        {
            var result = new List<Node>();

            if (from == to)
                return result;

            var nodesMap = new Dictionary<Node, Node>(); //save the back path map
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
                    nodesMap[childNode] = node;

                    if (childNode == to)
                    {
                        var prevNode = to;

                        while (prevNode != from)
                        {
                            result.Insert(0, prevNode);
                            prevNode = nodesMap[prevNode];
                        }

                        //result.Insert(0, from);

                        return result;
                    }

                    queue.Enqueue(childNode);
                }
            }

            return null;
        }
    }
}
