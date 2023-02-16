namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;
    using Data;

    public class NavGridSegmentator
    {
        private readonly NavGrid _navGrid;
        private readonly int _halfSpreadRange;
        public event Action MapUpdated = delegate { };

        public NavGridSegmentator(NavGrid navGrid, int spreadRange)
        {
            _navGrid = navGrid;
            _halfSpreadRange = spreadRange / 2;
        }

        public List<Node> Process(Point startPoint, List<Node> sectors, Node?[,] mapSegmentMatrix)
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
                    _navGrid.WalkArray[currentNode.Pos.X, currentNode.Pos.Y] = WalkableFlag.PossibleSector;
                }
                catch (Exception e)
                {
                    throw new ArgumentOutOfRangeException(
                        "Error out of range: "
                        + $"Width: {_navGrid.Width}, "
                        + $"Height: {_navGrid.Height}. "
                        + $"IndexX: {currentNode.Pos.X}, "
                        + $"IndexY: {currentNode.Pos.Y}, "
                        + $"Sectors: {possibleSectors.Count}", e);
                }

                while (currentNode.Stack.Count > 0)
                {
                    DoSpread(currentNode, currentNode.Stack.Pop(), mapSegmentMatrix);
                }

                if (currentNode.Square > 1)
                {
                    _navGrid.WalkArray[currentNode.Pos.X, currentNode.Pos.Y] = WalkableFlag.SectorCenter;

                    foreach (var possibleLink in currentNode.PossibleLinks)
                    {
                        currentNode.LinkWith(possibleLink);
                    }

                    currentNode.PossibleLinks.Clear(); //just clear it to free some memory
                    currentNode.UpdateBoundingCenter();
                    sectors.Add(currentNode);
                }

                var orderedSectors = currentNode.PossibleSectors.OrderBy(x => PointDist(currentNode.Pos, x));
                possibleSectors.AddRange(orderedSectors);
            }

            return sectors;
        }

        private static double PointDist(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
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

            if (value.Contain(WalkableFlag.Nonwalkable))
            {
                _navGrid.WalkArray[point.X, point.Y] = value | WalkableFlag.Passed;
                MapUpdated();

                return;
            }

            if (value.Contain(WalkableFlag.Passed))
            {
                if (value.Contain(WalkableFlag.PossibleSectorMarkedPassed))
                {
                    var linkedSection = mapSegmentMatrix[point.X, point.Y];

                    if (linkedSection != null && linkedSection != node && linkedSection.Id != -1)
                    {
                        if (!node.PossibleLinks.Contains(linkedSection))
                        {
                            node.PossibleLinks.Add(linkedSection);
                        }

                        //if (!linkedSection.PossibleLinks.Contains(mapSegment))
                        //    linkedSection.PossibleLinks.Add(mapSegment);
                    }
                }

                return;
            }

            var absX = Math.Abs(node.Pos.X - point.X);
            var absY = Math.Abs(node.Pos.Y - point.Y);

            if (absX > _halfSpreadRange || absY > _halfSpreadRange)
            {
                return;
            }

            if (mapSegmentMatrix[point.X, point.Y] == null)
            {
                mapSegmentMatrix[point.X, point.Y] = node;
            }

            if (absX == _halfSpreadRange || absY == _halfSpreadRange)
            {
                _navGrid.WalkArray[point.X, point.Y] = value | WalkableFlag.PossibleSectorMarkedPassed;
                MapUpdated();
                node.PossibleSectors.Push(point);

                node.Stack.Push(new Point(point.X + 1, point.Y + 1));
                node.Stack.Push(new Point(point.X + 1, point.Y - 1));
                node.Stack.Push(new Point(point.X - 1, point.Y + 1));
                node.Stack.Push(new Point(point.X - 1, point.Y - 1));

                return;
            }

            node.Square++;

            if (node.Min.X > point.X)
            {
                node.Min.X = point.X;
            }

            if (node.Min.Y > point.Y)
            {
                node.Min.Y = point.Y;
            }

            if (node.Max.X < point.X)
            {
                node.Max.X = point.X;
            }

            if (node.Max.Y < point.Y)
            {
                node.Max.Y = point.Y;
            }

            _navGrid.WalkArray[point.X, point.Y] = value | WalkableFlag.Passed;
            MapUpdated();
            node.Stack.Push(point with { X = point.X + 1 });
            node.Stack.Push(point with { X = point.X - 1 });
            node.Stack.Push(point with { Y = point.Y + 1 });
            node.Stack.Push(point with { Y = point.Y - 1 });
            node.Stack.Push(new Point(point.X + 1, point.Y + 1));
            node.Stack.Push(new Point(point.X + 1, point.Y - 1));
            node.Stack.Push(new Point(point.X - 1, point.Y + 1));
            node.Stack.Push(new Point(point.X - 1, point.Y - 1));
        }
    }
}
