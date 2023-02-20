namespace TravelShortPathFinder.Algorithm.Data
{
    public class Settings
    {
        public Settings(
            int segmentationMinSegmentSize = 300,
            float playerVisibilityRadius = 50,
            int segmentationSquareSize = 80,
            float localSelectNearNodeRange = 20,
            float optimizationMoveDist = 5,
            bool fastSegmentationThroughOnePoint = false)
        {
            SegmentationMinSegmentSize = segmentationMinSegmentSize;
            PlayerVisibilityRadius = playerVisibilityRadius;
            SegmentationSquareSize = segmentationSquareSize;
            LocalSelectNearNodeRange = localSelectNearNodeRange;
            FastSegmentationThroughOnePoint = fastSegmentationThroughOnePoint;
            OptimizationMoveDist = optimizationMoveDist;
        }

        /// <summary>
        /// The minimum square size of segment.
        /// All smaller square size segments will be removed.
        /// </summary>
        public int SegmentationMinSegmentSize { get; set; } = 300;

        /// <summary>
        /// The player look radius
        /// </summary>
        public float PlayerVisibilityRadius { get; set; } = 50;

        /// <summary>
        /// The maximum size of square segment during segmentation
        /// </summary>
        public int SegmentationSquareSize { get; set; } = 80;

        /// <summary>
        /// The range of selecting nearest node from nodes in this radius from player
        /// Note: radius mean amount of nodes from players, not distance
        /// Nodes in range of "LocalSelectNearNodeRange" will be chosed and from them the nearest node to player will be selected
        /// </summary>
        public float LocalSelectNearNodeRange { get; set; } = 20;

        /// <summary>
        /// The distance from last algorithm update pos in which the GraphMapExplorer.Update will not trigger algorithm
        /// </summary>
        public float OptimizationMoveDist { get;  } = 5;

        public bool FastSegmentationThroughOnePoint { get; set; }
    }
}
