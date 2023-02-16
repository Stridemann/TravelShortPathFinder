namespace TravelShortPathFinder.TestResources
{
    using System.Drawing;
    using TravelShortPathFinder.TestResources.Properties;

    public class InputNavCase
    {
        public InputNavCase(Bitmap bitmap, Point startPoint)
        {
            Bitmap = bitmap;
            StartPoint = startPoint;
        }

        public Bitmap Bitmap { get; }
        public Point StartPoint { get; }
    }

    public static class InputNavCases
    {
        public static InputNavCase Case1 = new InputNavCase(Resources.InputNav_01, new Point(66, 20));
    }
}
