namespace TravelShortPathFinder.Gui.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using Algorithm.Data;
    using Prism.Mvvm;
    using TestResources.Properties;
    using TravelShortPathFinder.Algorithm.Logic;
    using Point = System.Drawing.Point;

    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";

        private BitmapImage _bitmapImage;
        private Node[,] _mapSegmentMatrix;
        private NavGrid _navGrid;
        private List<Node> _sectors;
        private Bitmap _curBitmap;

        public MainWindowViewModel()
        {
            Task.Run(Explore);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public BitmapImage BitmapImage
        {
            get => _bitmapImage;
            set => SetProperty(ref _bitmapImage, value);
        }

        private string _imageSessionFolder;
        private int _imageSessionNum;

        private void Explore()
        {
            _imageSessionFolder = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
            Directory.CreateDirectory(_imageSessionFolder);

            Application.Current.Dispatcher.Invoke(
                () => { BitmapImage = ConvertBitmapToBitmapImage(Resources.InputNav_01); });
            Thread.Sleep(10000);
            var segmentationStart = new Point(66, 20);
            _navGrid = NavGridProvider.FromBitmap(Resources.InputNav_01);
            var segmentator = new NavGridSegmentator(_navGrid, 60);
            _mapSegmentMatrix = new Node[_navGrid.Width, _navGrid.Height];

            segmentator.MapUpdated += SegmentatorOnMapUpdated;
            _sectors = new List<Node>();
            var segments = segmentator.Process(segmentationStart, _sectors, _mapSegmentMatrix);
        }

        private unsafe void SegmentatorOnMapUpdated()
        {
            var bitmap = _curBitmap;

            if (bitmap == null)
            {
                _curBitmap = bitmap = new Bitmap(_navGrid.Width, _navGrid.Height);
            }

            var data = bitmap.LockBits(new Rectangle(0, 0, _navGrid.Width, _navGrid.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int* pData = (int*)data.Scan0;
            var blackArgb = Color.Black.ToArgb();
            var redArgb = Color.Red.ToArgb();
            var greenArgb = Color.Green.ToArgb();
            var lightGrayArgb = Color.DimGray.ToArgb();
            var orangeRedArgb = Color.OrangeRed.ToArgb();

            Parallel.For(
                0,
                _navGrid.Width * _navGrid.Height,
                (i, state) =>
                {
                    unsafe
                    {
                        var y = i / _navGrid.Width;
                        var x = i % _navGrid.Width;

                        var gridVal = _navGrid.WalkArray[x, y];

                        if ((gridVal & WalkableFlag.Nonwalkable) != 0)
                        {
                            pData[y * _navGrid.Width + x] = blackArgb;
                            //bitmap.SetPixel(x, y, Color.Black);
                        }
                        else if (gridVal == WalkableFlag.PossibleSectorMarkedPassed)
                        {
                            pData[y * _navGrid.Width + x] = redArgb;
                            //bitmap.SetPixel(x, y, Color.Red);
                        }
                        else if (gridVal == WalkableFlag.SectorCenter)
                        {
                            pData[y * _navGrid.Width + x] = greenArgb;
                            //bitmap.SetPixel(x, y, Color.Green);
                        }
                        else if (gridVal == WalkableFlag.Walkable)
                        {
                            pData[y * _navGrid.Width + x] = lightGrayArgb;
                            //bitmap.SetPixel(x, y, Color.LightGray);
                        }
                        else
                        {
                            var node = _mapSegmentMatrix[x, y];

                            if (node != null)
                            {
                                var seed = node.Id;
                                var rand = new Random(seed);
                                var randomColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                                //bitmap.SetPixel(x, y, randomColor);
                                pData[y * _navGrid.Width + x] = randomColor.ToArgb();
                            }
                            else
                            {
                                pData[y * _navGrid.Width + x] = orangeRedArgb;
                                //bitmap.SetPixel(x, y, Color.OrangeRed);
                            }
                        }
                    }
                });

            bitmap.UnlockBits(data);

            using var g = Graphics.FromImage(bitmap);

            foreach (var node in _sectors)
            {
                foreach (var link in node.Links)
                {
                    g.DrawLine(
                        Pens.White,
                        node.BoundingCenter.X,
                        node.BoundingCenter.Y,
                        link.BoundingCenter.X,
                        link.BoundingCenter.Y);
                }
            }

            _imageSessionNum++;
            //bitmap.Save(Path.Combine(_imageSessionFolder, $"Image_{_imageSessionNum}.png"));

            var bImg = ConvertBitmapToBitmapImage(bitmap);

            Application.Current.Dispatcher.Invoke(
                () => { BitmapImage = bImg; });
        }

        public BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // optional if you want to use the BitmapImage in a different thread

                return bitmapImage;
            }
        }
    }
}
