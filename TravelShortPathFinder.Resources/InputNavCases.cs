namespace TravelShortPathFinder.TestResources
{
    using System.Drawing;
    using Algorithm.Data;
    using TravelShortPathFinder.TestResources.Properties;

    public class InputNavCase
    {
        public InputNavCase(Bitmap bitmap, Point startPoint, Settings settings)
        {
            Bitmap = bitmap;
            StartPoint = startPoint;
            Settings = settings;
        }

        public Bitmap Bitmap { get; }
        public Point StartPoint { get; }
        public Settings Settings { get; }
    }

    public static class InputNavCases
    {
        public static InputNavCase Case1 = new InputNavCase(Resources.InputNav_01, new Point(66, 20), new Settings(segmentationSquareSize:30,playerVisibilityRadius:30,segmentationMinSegmentSize:200));
        public static InputNavCase Case2 = new InputNavCase(Resources.InputNav_02, new Point(70, 25), new Settings(segmentationSquareSize:50,segmentationMinSegmentSize:200,playerVisibilityRadius:40));
        public static InputNavCase Case3 = new InputNavCase(Resources.InputNav_03, new Point(152, 30), new Settings(segmentationSquareSize:60,segmentationMinSegmentSize:200,playerVisibilityRadius:60));
        public static InputNavCase Case4 = new InputNavCase(Resources.InputNav_04, new Point(2315, 160), new Settings(segmentationSquareSize:80,playerVisibilityRadius:80));
    }
}
