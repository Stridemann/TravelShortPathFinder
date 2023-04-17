namespace TravelShortPathFinder.Algorithm.Data
{
    using Logic;

    public class NavGrid
    {
        public readonly int Height;
        public readonly int Width;
        public readonly WalkableFlag[,] WalkArray;
        public readonly NavCell[,] NavArray;

        public NavGrid(
            int width,
            int height,
            WalkableFlag[,] walkArray,
            NavCell[,] navArray)
        {
            Width = width;
            Height = height;
            WalkArray = walkArray;
            NavArray = navArray;
        }

        public bool IsWalkable(int x, int y) => WalkArray[x, y].HasMyFlag(WalkableFlag.Walkable);
    }
}
