#pragma warning disable CA1416
namespace TravelShortPathFinder.Algorithm.Utils
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using Data;
    using Logic;

    public static class NavGridProvider
    {
        public static unsafe NavGrid FromBitmap(Bitmap image)
        {
            var imageWidth = image.Width;
            var imageHeight = image.Height;

            // Lock the bitmap bits to get a pointer to the pixel data
            var data = image.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            // Get a pointer to the pixel data
            int* pData = (int*)data.Scan0;

            var grid = CreateGrid(
                imageWidth,
                imageHeight,
                point =>
                {
                    var color = Color.FromArgb(pData[point.Y * imageWidth + point.X]);

                    return color.R > 128;
                });

            // Unlock the bitmap bits to release the pointer
            image.UnlockBits(data);

            return grid;
        }

        public static NavGrid CreateGrid(int imageWidth, int imageHeight, Func<Point, bool> isWalkableFunc)
        {
            var walkArray = new WalkableFlag[imageWidth, imageHeight];
            var navArray = new NavCell[imageWidth, imageHeight];

            // Process on multiple threads
            Parallel.For(
                0,
                imageHeight,
                y =>
                {
                    for (int x = 0; x < imageWidth; x++)
                    {
                        walkArray[x, y] = isWalkableFunc(new Point(x, y)) ? WalkableFlag.Walkable : WalkableFlag.NonWalkable;
                        navArray[x, y].Flag = walkArray[x, y];
                    }
                });

            return new NavGrid(imageWidth, imageHeight, walkArray, navArray);
        }
    }
}
