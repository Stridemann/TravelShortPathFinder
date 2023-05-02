namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;
    using Data;

    public class CustomRadialSweep
    {
        private static readonly Dictionary<Direction, Point> OffsetFromDirection;
        private readonly NavGrid _navGrid;

        static CustomRadialSweep()
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

        public CustomRadialSweep(NavGrid navGrid)
        {
            _navGrid = navGrid;
        }

        public List<Point>? ProcessNext(
            Point initPoint,
            Direction moveDir,
            int iterationId,
            bool moveInitPoint)
        {
            var point = initPoint;
            var result = new List<Point>();

            for (int j = 0; j < 10000; j++)
            {
                var invMoveDir = InverseDir(moveDir);
                var nextCheckDir = invMoveDir;

                for (int i = 0; i < 8; i++)
                {
                    nextCheckDir = NextClockwiceDir(nextCheckDir);
                    var nextOffset = OffsetFromDirection[nextCheckDir];
                    var checkPos = new Point(point.X + nextOffset.X, point.Y + nextOffset.Y);
                    var arrayVal = _navGrid.NavArray[checkPos.X, checkPos.Y];

                    if (arrayVal.Flag == WalkableFlag.Walkable || (arrayVal.Flag == WalkableFlag.Processed && arrayVal.IterationId == iterationId))
                    {
                        _navGrid.NavArray[checkPos.X, checkPos.Y] = new NavCell(
                            checkPos,
                            WalkableFlag.Processed,
                            iterationId);
                        _navGrid.WalkArray[checkPos.X, checkPos.Y] = WalkableFlag.Processed;
                        result.Add(checkPos);

                        if (checkPos == initPoint)
                        {
                            //done
                            return result;
                        }

                        if (moveInitPoint)
                        {
                            moveInitPoint = false;
                            initPoint = checkPos;
                        }

                        point = checkPos;
                        moveDir = nextCheckDir;

                        break;
                    }
                }
            }

            return null;
        }

        private static Direction NextClockwiceDir(Direction dir)
        {
            const int MAX = (int)Direction.Max;
            var next = ((int)dir + 1) % MAX;

            return (Direction)next;
        }

        private static Direction InverseDir(Direction dir)
        {
            const int MAX = (int)Direction.Max;
            var next = ((int)dir + 4) % MAX;

            return (Direction)next;
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
}
