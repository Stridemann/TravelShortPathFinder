#pragma warning disable CA1416

namespace TravelShortPathFinder.Algorithm.Logic
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using Data;

    public static class NavGridProvider
    {
        public static NavGrid FromBitmap(Bitmap image)
        {
            var imageWidth = image.Width;
            var imageHeight = image.Height;
            var walkArray = new WalkableFlag[imageWidth, imageHeight];

            // Lock the bitmap bits to get a pointer to the pixel data
            var data = image.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                // Get a pointer to the pixel data
                int* pData = (int*)data.Scan0;

                // Process the pixel data on multiple threads
                Parallel.For(0, imageHeight, y =>
                {
                    for (int x = 0; x < imageWidth; x++)
                    {
                        // Get the pixel color from the pixel data pointer
                        var color = Color.FromArgb(pData[y * imageWidth + x]);

                        // Check the color and set the walkable flag accordingly
                        walkArray[x, y] = color.R > 128 ? WalkableFlag.Walkable : WalkableFlag.Nonwalkable;
                    }
                });
            }

            // Unlock the bitmap bits to release the pointer
            image.UnlockBits(data);

            return new NavGrid(imageWidth, imageHeight, walkArray);
        }
    }
}
