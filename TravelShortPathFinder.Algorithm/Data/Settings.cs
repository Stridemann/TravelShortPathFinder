namespace TravelShortPathFinder.Algorithm.Data
{
    public class Settings
    {
        public int SegmentationMinSegmentSize { get; set; } = 300;
        public float ExploreNodeProcessRadius { get; set; } = 50;
        public int SegmentationSquareSize { get; set; } = 80;
        public float PlayerMovementDistanceFromWall { get; set; } = 10;
        public bool FastSegmentationThroughOnePoint { get; set; }
    }
}
