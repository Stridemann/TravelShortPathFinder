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
        public static InputNavCase Case2 = new InputNavCase(Resources.InputNav_02, new Point(70, 25));
        public static InputNavCase Case3 = new InputNavCase(Resources.InputNav_03, new Point(152, 30));
        public static InputNavCase Case4 = new InputNavCase(Resources.InputNav_04, new Point(1600, 1500));
        public static InputNavCase Case5 = new InputNavCase(Resources.InputNav_04, new Point(2315, 160));
    }
}
