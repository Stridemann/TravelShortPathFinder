namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;
    using Data;
    using Utils;

    public class NavGridSegmentator
    {
        private readonly NavGrid _navGrid;
        private readonly int _segmentSquareSize;
        private readonly Settings _settings;

        public NavGridSegmentator(NavGrid navGrid, Settings settings)
        {
            _settings = settings;
            _navGrid = navGrid;
            _segmentSquareSize = settings.SegmentationSquareSize / 2;
        }

        public void Process(Point startPoint, Graph graph)
        {
            if (startPoint.X < 0 || startPoint.Y < 0)
            {
                throw new ArgumentOutOfRangeException($"NavGridSegmentator. startPoint x or y is out of range (less than 0): {startPoint}");
            }

            if (startPoint.X >= _navGrid.Width || startPoint.Y >= _navGrid.Height)
            {
                throw new ArgumentOutOfRangeException(
                    "NavGridSegmentator. startPoint x or y is out of range (bigger than array size): "
                    + $"Point: {startPoint}, "
                    + $"ArrayWidth: {_navGrid.Width}, "
                    + $"ArrayHeight: {_navGrid.Height}");
            }

            var possibleSectors = new List<Point> { startPoint };
            Node currentNode;

            while (possibleSectors.Count > 0)
            {
                var possibleSectorPos = possibleSectors.Last();
                possibleSectors.RemoveAt(possibleSectors.Count - 1);

                currentNode = new Node(Node.UniqIdCounter++, possibleSectorPos);
                currentNode.Stack.Push(possibleSectorPos);

                try
                {
                    _navGrid.WalkArray[currentNode.Pos.X, currentNode.Pos.Y] = WalkableFlag.PossibleSegment;
                }
                catch (Exception e)
                {
                    throw new ArgumentOutOfRangeException(
                        "Error out of range: "
                        + $"Width: {_navGrid.Width}, "
                        + $"Height: {_navGrid.Height}. "
                        + $"IndexX: {currentNode.Pos.X}, "
                        + $"IndexY: {currentNode.Pos.Y}, "
                        + $"Sectors: {possibleSectors.Count}",
                        e);
                }

                while (currentNode.Stack.Count > 0)
                {
                    DoSpread(currentNode, currentNode.Stack.Pop(), graph.MapSegmentMatrix);
                }

                if (currentNode.Square > 1)
                {
                    _navGrid.WalkArray[currentNode.Pos.X, currentNode.Pos.Y] = WalkableFlag.PossibleSegmentPassed;

                    foreach (var possibleLink in currentNode.PossibleLinks)
                    {
                        currentNode.LinkWith(possibleLink);
                    }

                    currentNode.PossibleLinks.Clear(); //just clear it to free some memory
                    graph.Nodes.Add(currentNode);
                }

                var orderedSectors = currentNode.PossibleSegments.OrderBy(x => currentNode.Pos.Distance(x));
                possibleSectors.AddRange(orderedSectors);
            }
        }

        private void DoSpread(Node node, Point point, Node?[,] mapSegmentMatrix)
        {
            if (point.X < 0 || point.Y < 0)
            {
                return;
            }

            if (point.X >= _navGrid.Width || point.Y >= _navGrid.Height)
            {
                return;
            }

            var value = _navGrid.WalkArray[point.X, point.Y];

            if (value.Contain(WalkableFlag.NonWalkable))
            {
                _navGrid.WalkArray[point.X, point.Y] = value | WalkableFlag.Passed;
    
                return;
            }

            if (value.Contain(WalkableFlag.Passed))
            {
                if (value.Contain(WalkableFlag.PossibleSegmentStart))
                {
                    var linkedSection = mapSegmentMatrix[point.X, point.Y];

                    if (linkedSection != null && linkedSection != node && linkedSection.Id != -1)
                    {
                        if (!node.PossibleLinks.Contains(linkedSection))
                        {
                            node.PossibleLinks.Add(linkedSection);
                        }
                    }
                }

                return;
            }

            var absX = Math.Abs(node.Pos.X - point.X);
            var absY = Math.Abs(node.Pos.Y - point.Y);

            if (absX > _segmentSquareSize || absY > _segmentSquareSize)
            {
                return;
            }

            if (mapSegmentMatrix[point.X, point.Y] == null)
            {
                mapSegmentMatrix[point.X, point.Y] = node;
            }

            if (absX == _segmentSquareSize || absY == _segmentSquareSize)
            {
                _navGrid.WalkArray[point.X, point.Y] = value | WalkableFlag.PossibleSegmentStart;
                node.PossibleSegments.Push(point);

                node.Stack.Push(new Point(point.X + 1, point.Y + 1));
                node.Stack.Push(new Point(point.X + 1, point.Y - 1));
                node.Stack.Push(new Point(point.X - 1, point.Y + 1));
                node.Stack.Push(new Point(point.X - 1, point.Y - 1));

                return;
            }

            node.Square++;

            _navGrid.WalkArray[point.X, point.Y] = value | WalkableFlag.Passed;

            if (!_settings.FastSegmentationThroughOnePoint)
            {
                node.Stack.Push(point with { X = point.X + 1 });
                node.Stack.Push(point with { X = point.X - 1 });
                node.Stack.Push(point with { Y = point.Y + 1 });
                node.Stack.Push(point with { Y = point.Y - 1 });
            }

            node.Stack.Push(new Point(point.X + 1, point.Y + 1));
            node.Stack.Push(new Point(point.X + 1, point.Y - 1));
            node.Stack.Push(new Point(point.X - 1, point.Y + 1));
            node.Stack.Push(new Point(point.X - 1, point.Y - 1));
        }
    }
}
