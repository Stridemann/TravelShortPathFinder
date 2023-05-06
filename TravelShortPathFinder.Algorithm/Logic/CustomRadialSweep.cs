namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;
    using Data;

    public class CustomRadialSweep
    {
        private readonly NavGrid _navGrid;
        public Point CurProcessPoint;
        private Direction _nextCheckDir;
        private Direction _moveDir;
        private Point _initPoint;

        public CustomRadialSweep(NavGrid navGrid)
        {
            _navGrid = navGrid;
        }

        public void InitLoop(Point initPoint, Direction moveDir)
        {
            _initPoint = initPoint;
            _moveDir = moveDir;
            CurProcessPoint = initPoint;
        }

        public bool ProcessNext(int iterationId, List<Point> result)
        {
            _nextCheckDir = CustomRadialSweepUtils.InverseDir(_moveDir);

            for (int i = 0; i < 8; i++)
            {
                _nextCheckDir = CustomRadialSweepUtils.NextClockwiceDir(_nextCheckDir);
                var nextOffset = CustomRadialSweepUtils.OffsetFromDirection[_nextCheckDir];
                var checkPos = new Point(CurProcessPoint.X + nextOffset.X, CurProcessPoint.Y + nextOffset.Y);
                var arrayVal = _navGrid.NavArray[checkPos.X, checkPos.Y];

                var overlap = arrayVal.Flag == WalkableFlag.Processed && arrayVal.IterationId == iterationId;

                if (arrayVal.Flag == WalkableFlag.Walkable || overlap)
                {
                    var cell = new NavCell(
                        checkPos,
                        WalkableFlag.Processed,
                        iterationId);

                    if (overlap)
                        cell.ColorId = 1;

                    _navGrid.NavArray[checkPos.X, checkPos.Y] = cell;
                    _navGrid.WalkArray[checkPos.X, checkPos.Y] = WalkableFlag.Processed;
                    result.Add(checkPos);

                    if (checkPos == _initPoint)
                    {
                        return true;
                    }

                    CurProcessPoint = checkPos;
                    _moveDir = _nextCheckDir;

                    break;
                }
            }

            return false;
        }
    }
}
