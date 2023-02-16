namespace TravelShortPathFinder.Algorithm.Data
{
    public class NavGrid
    {
        public readonly int Height;
        public readonly int Width;
        public readonly WalkableFlag[,] WalkArray;

        public NavGrid(int width, int height, WalkableFlag[,] walkArray)
        {
            Width = width;
            Height = height;
            WalkArray = walkArray;
        }
    }
}
