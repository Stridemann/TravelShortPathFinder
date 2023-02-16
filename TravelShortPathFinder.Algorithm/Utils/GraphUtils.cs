namespace TravelShortPathFinder.Algorithm.Utils
{
    using Data;
    using Logic;

    public static class GraphUtils
    {
        public static void AppendNodeToGraph(Graph graph, Node newNode)
        {
            graph.Nodes.Add(newNode);

            foreach (var segmentLink in newNode.Links)
            {
                var targetId = segmentLink.Id;

                //we are not sure that nodes are ordered correctly, but they should
                var node = graph.Nodes.Count < targetId ? graph.Nodes[targetId] : null;

                if (node != null && node.Id != targetId)
                {
                    node = null;
                }

                if (node == null)
                {
                    //LogError("Failed to pick node by id. Going to do full search...", 20);
                    //TODO: Fix this (to Strideman)
                    node = graph.Nodes.FirstOrDefault(x => x.Id == targetId);

                    if (node == null)
                    {
                        //LogError("Failed to find node by id aftero full search...", 20);
                        return;
                    }
                }

                newNode.LinkWith(node);
            }
        }
    }
}
