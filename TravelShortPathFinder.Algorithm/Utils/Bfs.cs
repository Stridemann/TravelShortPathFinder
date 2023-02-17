namespace TravelShortPathFinder.Algorithm.Utils
{
    using Data;

    public static class Bfs
    {
        public static void Process(Node startNode, Func<Node, bool> actionDelegate)
        {
            var queue = new Queue<Node>();
            Node.ProcessIteration++;
            startNode.SetProcessed();
            queue.Enqueue(startNode);
            actionDelegate(startNode);

            while (queue.TryDequeue(out var node))
            {
                foreach (var item in node.Links)
                {
                    if (item.IsProcessed)
                    {
                        continue;
                    }

                    item.SetProcessed();

                    if (actionDelegate(item))
                    {
                        queue.Enqueue(item);
                    }
                }
            }
        }
    }
}
