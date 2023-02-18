namespace TravelShortPathFinder.Algorithm.Utils
{
    using Data;

    public static class BreadthFirstSearch
    {
        // The regular https://en.wikipedia.org/wiki/Breadth-first_search
        // actionDelegate called on each node and controls should we add process it (and it's child) further
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
