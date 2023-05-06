namespace TravelShortPathFinder.Algorithm.Data
{
    using System.Drawing;

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

        public bool IsWalkableNonProcessed(int x, int y) => WalkArray[x, y] == WalkableFlag.Walkable;
    }

    public struct NavCell
    {
        public Point Pos;
        public WalkableFlag Flag;
        public bool IsWalkableNonProcessed => Flag == WalkableFlag.Walkable;
        public bool IsProcessed => (Flag & WalkableFlag.Processed) != 0;
        public int IterationId;
        public int ColorId;

        public NavCell(
            Point pos,
            WalkableFlag flag,
            int iterationId)
        {
            Pos = pos;
            Flag = flag;
            IterationId = iterationId;
            ColorId = 0;
        }
    }
}
