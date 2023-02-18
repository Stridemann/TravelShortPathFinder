namespace TravelShortPathFinder.Algorithm.Data
{
    public class Settings
    {
        /// <summary>
        /// The minimum square size of segment.
        /// All smaller square size segments will be removed.
        /// </summary>
        public int SegmentationMinSegmentSize { get; set; } = 300;

        /// <summary>
        /// The player look radius
        /// </summary>
        public float ExploreNodeProcessRadius { get; set; } = 50;

        /// <summary>
        /// The maximum size of square segment during segmentation
        /// </summary>
        public int SegmentationSquareSize { get; set; } = 80;

        /// <summary>
        /// The range of selecting nearest node from nodes in this radius from player
        /// Note: radius mean amount of nodes from players, not distance
        /// Nodes in range of "LocalSelectNearNodeRange" will be chosed and from them the nearest node to player will be selected
        /// </summary>
        public float LocalSelectNearNodeRange { get; set; } = 10;

        public bool FastSegmentationThroughOnePoint { get; set; }
    }
}
