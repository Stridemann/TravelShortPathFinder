namespace TravelShortPathFinder.Algorithm.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Data;

    /// <summary>
    /// http://www.imageprocessingplace.com/downloads_V3/root_downloads/tutorials/contour_tracing_Abeer_George_Ghuneim/theo.html
    /// </summary>
    public enum TheoDirection
    {
        North,
        East,
        South,
        West
    }

    public static class BoundaryTracing
    {
        private enum TheoMovement
        {
            P1,
            P2,
            P3
        };

        private static readonly Dictionary<TheoDirection, TheoDirection> P1TheoDirection =
            new Dictionary<TheoDirection, TheoDirection>
            {
                { TheoDirection.North, TheoDirection.West },
                { TheoDirection.West, TheoDirection.South },
                { TheoDirection.South, TheoDirection.East },
                { TheoDirection.East, TheoDirection.North }
            };

        // <image src=".\images\theodem.GIF"/>
        private static readonly Func<Point, Point>[] TheoMove =
        {
            // Face North
            point => new Point(point.X - 1, point.Y - 1), // Up Left
            point => new Point(point.X, point.Y - 1), // Up
            point => new Point(point.X + 1, point.Y - 1), // Up Right
            // Face East
            point => new Point(point.X + 1, point.Y - 1), // Right Up
            point => new Point(point.X + 1, point.Y), // Right
            point => new Point(point.X + 1, point.Y + 1), // Right Down
            // Face South
            point => new Point(point.X + 1, point.Y + 1), // Down Right
            point => new Point(point.X, point.Y + 1), // Down
            point => new Point(point.X - 1, point.Y + 1), // Down Left
            // Face West
            point => new Point(point.X - 1, point.Y + 1), // Left Down
            point => new Point(point.X - 1, point.Y), // Left
            point => new Point(point.X - 1, point.Y - 1) // Left Up
        };

        public static HashSet<Tuple<Point, int>> Trace(NavGrid navGrid, Point start, TheoDirection initialDirection)
        {
            var contour = new HashSet<Tuple<Point, int>>();
            // Define p to be the current boundary pixel i.e. the pixel you are standing on.
            Point? p = start;

            // Define a "step" in a given direction as moving a distance of one pixel in that direction.
            int directionOffset = 0;
            int stationaryRotations = 0;
            TheoDirection lastDirection = initialDirection;

            do
            {
                // <image src=".\images\theopic2.GIF"/>
                Point p1 = TheoMove[(int)TheoMovement.P1 + directionOffset](p.Value);

                if (navGrid.IsWalkable(p1.X, p1.Y))
                {
                    p = p1;
                    contour.Add(new Tuple<Point, int>(p1, contour.Count));
                    navGrid.WalkArray[p1.X, p1.Y] = WalkableFlag.Processed;
                    stationaryRotations = 0;
                    lastDirection = P1TheoDirection[lastDirection];
                    directionOffset = (int)lastDirection * 3;
                }
                else
                {
                    Point p2 = TheoMove[(int)TheoMovement.P2 + directionOffset](p.Value);

                    if (navGrid.IsWalkable(p2.X, p2.Y))
                    {
                        contour.Add(new Tuple<Point, int>(p2, contour.Count));
                        navGrid.WalkArray[p2.X, p2.Y] = WalkableFlag.Processed;
                        // <image src=".\images\theopic3.GIF"/>
                        p = p2;
                        stationaryRotations = 0;
                    }
                    else
                    {
                        Point p3 = TheoMove[(int)TheoMovement.P3 + directionOffset](p.Value);

                        if (navGrid.IsWalkable(p3.X, p3.Y))
                        {
                            contour.Add(new Tuple<Point, int>(p3, contour.Count));
                            navGrid.WalkArray[p3.X, p3.Y] = WalkableFlag.Processed;
                            // <image src=".\images\theopic4.GIF"/>
                            p = p3;
                            stationaryRotations = 0;
                        }
                        // if you have already rotated through 90 degrees clockwise 3 times while on the same pixel p
                        else if (stationaryRotations >= 3)
                        {
                            // terminate the program and declare p as an isolated pixel;
                            break;
                        }
                        else
                        {
                            // rotate 90 degrees clockwise while standing on the current pixel p
                            lastDirection = (TheoDirection)(((int)lastDirection + 1) % 4);
                            stationaryRotations++;
                            directionOffset = (int)lastDirection * 3;
                        }
                    }
                }
            } while (p != start);

            return contour;
        }
    }
}
