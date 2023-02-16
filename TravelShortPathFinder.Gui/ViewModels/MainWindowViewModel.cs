namespace TravelShortPathFinder.Gui.ViewModels
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using Algorithm.Data;
    using Algorithm.Logic;
    using Prism.Mvvm;
    using TestResources;

    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";

        private BitmapImage _bitmapImage;
        private NavGrid _navGrid;
        private Bitmap _curBitmap;

        private string _imageSessionFolder;
        private int _imageSessionNum;
        private Graph _graph;

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

        private void Explore()
        {
            var navCase = InputNavCases.Case1;
            var settings = new Settings();
            settings.SegmentationSquareSize = 40;

            _imageSessionFolder = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
            Directory.CreateDirectory(_imageSessionFolder);

            Application.Current.Dispatcher.Invoke(
                () => { BitmapImage = ConvertBitmapToBitmapImage(navCase.Bitmap); });

            Thread.Sleep(1000);

            _navGrid = NavGridProvider.FromBitmap(navCase.Bitmap);
            _graph = new Graph(_navGrid);

            var segmentator = new NavGridSegmentator(_navGrid, settings);

            segmentator.MapUpdated += SegmentatorOnMapUpdated;
            segmentator.Process(navCase.StartPoint, _graph);

            var optimizer = new NavGridOptimizer(settings.SegmentationMinSegmentSize);
            optimizer.OptimizeGraph(_graph, _navGrid);
            Thread.Sleep(2000);
            SegmentatorOnMapUpdated();
        }

        private unsafe void SegmentatorOnMapUpdated()
        {
            var bitmap = _curBitmap;

            if (bitmap == null)
            {
                _curBitmap = bitmap = new Bitmap(_navGrid.Width, _navGrid.Height);
            }

            var data = bitmap.LockBits(new Rectangle(0, 0, _navGrid.Width, _navGrid.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var pData = (int*)data.Scan0;
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
                    var y = i / _navGrid.Width;
                    var x = i % _navGrid.Width;

                    var gridVal = _navGrid.WalkArray[x, y];

                    if ((gridVal & WalkableFlag.Nonwalkable) != 0)
                    {
                        pData![y * _navGrid.Width + x] = blackArgb;
                        //bitmap.SetPixel(x, y, Color.Black);
                    }
                    else if (gridVal == WalkableFlag.PossibleSectorMarkedPassed)
                    {
                        pData![y * _navGrid.Width + x] = redArgb;
                        //bitmap.SetPixel(x, y, Color.Red);
                    }
                    else if (gridVal == WalkableFlag.SectorCenter)
                    {
                        pData![y * _navGrid.Width + x] = greenArgb;
                        //bitmap.SetPixel(x, y, Color.Green);
                    }
                    else if (gridVal == WalkableFlag.Walkable)
                    {
                        pData![y * _navGrid.Width + x] = lightGrayArgb;
                        //bitmap.SetPixel(x, y, Color.LightGray);
                    }
                    else
                    {
                        var node = _graph.MapSegmentMatrix[x, y];

                        if (node != null)
                        {
                            var seed = node.Id;
                            var rand = new Random(seed);
                            var randomColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                            //bitmap.SetPixel(x, y, randomColor);
                            pData![y * _navGrid.Width + x] = randomColor.ToArgb();
                        }
                        else
                        {
                            pData![y * _navGrid.Width + x] = orangeRedArgb;
                            //bitmap.SetPixel(x, y, Color.OrangeRed);
                        }
                    }
                });

            bitmap.UnlockBits(data);

            using var g = Graphics.FromImage(bitmap);

            foreach (var node in _graph.Nodes)
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
