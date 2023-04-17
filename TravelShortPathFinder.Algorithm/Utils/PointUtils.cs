using System.Drawing;

namespace TravelShortPathFinder.Algorithm.Utils
{
    using System.Numerics;

    public static class PointUtils
    {
        public static float Distance(this Point point1, Point point2)
        {
            return MathF.Sqrt(DistanceSquared(point1, point2));
        }

        public static float DistanceSquared(this Point point1, Point point2)
        {
            var vector2 = point1.ToVector2() - point2.ToVector2();
            return Vector2.Dot(vector2, vector2);
        }

        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static Point Translate(this Point point, int addX, int addY)
        {
            return new Point(point.X + addX, point.Y + addY);
        }
    }
}
