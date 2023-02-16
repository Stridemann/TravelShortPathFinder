namespace TravelShortPathFinder.Algorithm.Data
{
    public class Settings
    {
        public int SegmentationRange { get; set; } = 80;
        public int SegmentationMinSegmentSize { get; set; } = 300;
        public float ExploreNodeProcessDist { get; set; } = 50;
        public float PlayerMovementDistanceFromWall { get; set; } = 10;
    }
}
