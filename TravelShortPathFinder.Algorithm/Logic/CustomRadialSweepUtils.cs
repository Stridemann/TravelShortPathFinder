namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;

    public static class CustomRadialSweepUtils
    {
        public static readonly Dictionary<Direction, Point> OffsetFromDirection;

        static CustomRadialSweepUtils()
        {
            OffsetFromDirection = new Dictionary<Direction, Point>
            {
                { Direction.Right, new Point(1, 0) },
                { Direction.BottomRight, new Point(1, 1) },
                { Direction.Bottom, new Point(0, 1) },
                { Direction.BottomLeft, new Point(-1, 1) },
                { Direction.Left, new Point(-1, 0) },
                { Direction.TopLeft, new Point(-1, -1) },
                { Direction.Top, new Point(0, -1) },
                { Direction.TopRight, new Point(1, -1) }
            };
        }

        public static Direction NextClockwiceDir(Direction dir)
        {
            const int MAX = (int)Direction.Max;
            var next = ((int)dir + 1) % MAX;

            return (Direction)next;
        }

        public static Direction InverseDir(Direction dir)
        {
            const int MAX = (int)Direction.Max;
            var next = ((int)dir + 4) % MAX;

            return (Direction)next;
        }
    }

    public enum Direction
    {
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left,
        TopLeft,
        Top,
        TopRight,
        Max
    }
}
