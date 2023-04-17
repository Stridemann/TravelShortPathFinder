namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;
    using Data;
    using Utils;

    public struct NavCell
    {
        public Point Pos;
        public WalkableFlag Flag;
        public bool IsWalkableNonProcessed => Flag == WalkableFlag.Walkable;
        public bool IsProcessed => (Flag & WalkableFlag.Processed) != 0;
        public int Id;
        public int IterationId;

        public NavCell(
            Point pos,
            WalkableFlag flag,
            int id,
            int iterationId)
        {
            Pos = pos;
            Flag = flag;
            Id = id;
            IterationId = iterationId;
        }
    }

    public class NavGridSegmentatorV2
    {
        private readonly NavGrid _navGrid;
        private readonly Settings _settings;
        private float _visibilityRadius;
        private int _curIteration;

        public NavGridSegmentatorV2(NavGrid navGrid, Settings settings)
        {
            _settings = settings;
            _navGrid = navGrid;
            _visibilityRadius = settings.PlayerVisibilityRadius;
        }

        public void Process(Point startPoint, Graph graph)
        {
            var borderPos = new Point(51, 18);
            var contour = BoundaryTracing.Trace(_navGrid, borderPos, TheoDirection.North);
            var idCounter = 0;

            foreach (var point in contour)
            {
                _navGrid.NavArray[point.Item1.X, point.Item1.Y] = new NavCell(point.Item1, WalkableFlag.Processed, idCounter++, 0);
            }

            for (int i = 0; i < 10; i++)
            {
                contour = Dilate(contour, i);
            }

            foreach (var point in contour)
            {
                _navGrid.NavArray[point.Item1.X, point.Item1.Y].Flag = WalkableFlag.PossibleSegmentProcessed;
                _navGrid.NavArray[point.Item1.X, point.Item1.Y].Id = point.Item2;
            }
        }

        private HashSet<Tuple<Point, int>> Dilate(HashSet<Tuple<Point, int>> contour, int iterationId)
        {
            var rezult = new HashSet<Tuple<Point, int>>();

            foreach (var point in contour)
            {
                var curCell = _navGrid.NavArray[point.Item1.X, point.Item1.Y];

                TryAddPoint(
                    point.Item1.Translate(0, 1),
                    curCell,
                    rezult,
                    iterationId);

                TryAddPoint(
                    point.Item1.Translate(1, 0),
                    curCell,
                    rezult,
                    iterationId);

                TryAddPoint(
                    point.Item1.Translate(0, -1),
                    curCell,
                    rezult,
                    iterationId);

                TryAddPoint(
                    point.Item1.Translate(-1, 0),
                    curCell,
                    rezult,
                    iterationId);
            }

            return rezult;
        }

        private void TryAddPoint(
            Point point,
            NavCell fromCell,
            HashSet<Tuple<Point, int>> dilatedContour,
            int iterationId)
        {
            if (point.X < 0 || point.Y < 0 || point.X >= _navGrid.Width || point.Y >= _navGrid.Height)
                return;

            var curCell = _navGrid.NavArray[point.X, point.Y];

            if (curCell.IsWalkableNonProcessed)
            {
                var right = _navGrid.NavArray[point.X + 1, point.Y];
                var left = _navGrid.NavArray[point.X - 1, point.Y];
                var up = _navGrid.NavArray[point.X, point.Y + 1];
                var down = _navGrid.NavArray[point.X, point.Y - 1];

                var horisontalCheck = right.IsProcessed && left.IsProcessed;
                var verticalCheck = up.IsProcessed && down.IsProcessed;

                const int COMPARE_DIST = 0;//15;

                var pointId = dilatedContour.Count;
                if (horisontalCheck && Math.Abs(right.Id - left.Id) > COMPARE_DIST)
                {
                    _navGrid.NavArray[point.X, point.Y] = new NavCell(point, WalkableFlag.PossibleSegmentProcessed, pointId, iterationId + 1);
                }
                else if (verticalCheck && Math.Abs(up.Id - down.Id) > COMPARE_DIST)
                {
                    _navGrid.NavArray[point.X, point.Y] = new NavCell(point, WalkableFlag.PossibleSegmentProcessed, pointId, iterationId + 1);
                }
                else
                {
                    _navGrid.NavArray[point.X, point.Y] = new NavCell(point, WalkableFlag.Processed, pointId, iterationId + 1);
                    dilatedContour.Add(new Tuple<Point, int>(point, pointId));
                }
            }
            else if (curCell.IsProcessed
                     && curCell.IterationId == iterationId
                     && fromCell.IterationId == iterationId) //this and prev point is from current contour
            {
                var idDiff = fromCell.Id - curCell.Id;
                const int ID_DIFF_MERGE = 1;
                if (idDiff > ID_DIFF_MERGE)
                {
                    _navGrid.NavArray[fromCell.Pos.X, fromCell.Pos.Y].Flag = WalkableFlag.PossibleSegmentProcessed;
                }
                else if (idDiff < -ID_DIFF_MERGE)
                {
                    _navGrid.NavArray[point.X, point.Y].Flag = WalkableFlag.PossibleSegmentProcessed;
                }
            }
        }
    }
}
