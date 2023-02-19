namespace TravelShortPathFinder.Gui.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Algorithm.Data;
    using Algorithm.Logic;
    using Prism.Mvvm;
    using TestResources;
    using Brushes = System.Drawing.Brushes;
    using Color = System.Drawing.Color;
    using FontStyle = System.Drawing.FontStyle;
    using Pen = System.Drawing.Pen;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;

    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";

        private BitmapImage _bitmapImage;
        private NavGrid _navGrid;
        private Bitmap _curBitmap;

        private string _imageSessionFolder;
        private int _imageSessionNum;
        private Graph _graph;
        private GraphMapExplorer _explorer;
        private Settings _settings;
        private Vector2 _playerPos;

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
            var navCase = InputNavCases.Case5;
            _settings = navCase.Settings;

            _imageSessionFolder = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
            Directory.CreateDirectory(_imageSessionFolder);

            Application.Current.Dispatcher.Invoke(
                () => { BitmapImage = ConvertBitmapToBitmapImage(navCase.Bitmap); });
            Thread.Sleep(1000);
            _navGrid = NavGridProvider.FromBitmap(navCase.Bitmap);
            _graph = new Graph(_navGrid);
            _explorer = new GraphMapExplorer(_settings, _graph, new DefaultNextNodeSelector(_settings));

            //_explorer.MapPriorityUpdated += () =>
            //{
            //    RepaintBitmap(null, null, true);
            //    Thread.Sleep(0);
            //};

            //_explorer.Segmentator.MapSegmentAdded += () =>
            //{
            //    RepaintBitmap(null, null, true);
            //    Thread.Sleep(20);
            //};

            _playerPos = new Vector2(navCase.StartPoint.X, navCase.StartPoint.Y);
            _explorer.ProcessSegmentation(_playerPos);
            
            var curPlayerNode = _graph.Nodes.First();

            _explorer.Update(_playerPos);
            RepaintBitmap(null, null, true);

            //Thread.Sleep(2000);

            if (!_explorer.HasLocation)
                return;

            const int DELAY = 250;

            do
            {
                _explorer.Update(curPlayerNode.GridPos);

                if (!_explorer.HasLocation)
                {
                    RepaintBitmap(null, curPlayerNode, false);

                    break;
                }

                var path = SimplePathFinder.FindPath(curPlayerNode, _explorer.NextRunNode);

                if (path == null)
                {
                    //For some reason cannot find path there
                    //Shouldn't happen
                    _explorer.NextRunNode.Unwalkable = true;
                }
                else if (path.Count == 0)
                {
                }
                else
                {
                    curPlayerNode = path[0];
                }

                Thread.Sleep(DELAY);
                RepaintBitmap(path, curPlayerNode, false);
            } while (_explorer.HasLocation);
        }

        private unsafe void RepaintBitmap(List<Node> navPath, Node playerNode, bool drawNodesSeparateColor)
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
            var darkGrayArgb = Color.DimGray.ToArgb();
            var font = new Font("Arial", 20, FontStyle.Regular);

            //Note:
            //pData![y * _navGrid.Width + x] = blackArgb;
            //is the same as:
            //bitmap.SetPixel(x, y, Color.Black);
            Parallel.For(
                0,
                _navGrid.Width * _navGrid.Height,
                i =>
                {
                    var y = i / _navGrid.Width;
                    var x = i % _navGrid.Width;

                    var gridVal = _navGrid.WalkArray[x, y];

                    if ((gridVal & WalkableFlag.NonWalkable) != 0)
                    {
                        pData![y * _navGrid.Width + x] = blackArgb;
                    }
                    else if (gridVal == WalkableFlag.PossibleSegmentStart)
                    {
                        pData![y * _navGrid.Width + x] = redArgb;
                    }
                    else if (gridVal == WalkableFlag.PossibleSegmentPassed)
                    {
                        pData![y * _navGrid.Width + x] = greenArgb;
                    }
                    else if (gridVal == WalkableFlag.Walkable)
                    {
                        pData![y * _navGrid.Width + x] = lightGrayArgb;
                    }
                    else
                    {
                        var node = _graph.MapSegmentMatrix[x, y];

                        if (node != null)
                        {
                            if (node.IsVisited)
                            {
                                pData![y * _navGrid.Width + x] = darkGrayArgb;
                            }
                            else if (node.IsRemovedByOptimizer)
                            {
                                pData![y * _navGrid.Width + x] = darkGrayArgb;
                            }
                            else
                            {
                                int seed;

                                if (drawNodesSeparateColor)
                                    seed = node.Id;
                                else
                                    seed = node.Group?.Id ?? node.Id;
                                var rand = new Random(seed);
                                var randomColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                                pData![y * _navGrid.Width + x] = randomColor.ToArgb();
                            }
                        }
                        else
                        {
                            pData![y * _navGrid.Width + x] = orangeRedArgb;
                        }
                    }
                });

            bitmap.UnlockBits(data);

            using var g = Graphics.FromImage(bitmap);
     
            foreach (var node in _graph.Nodes)
            {
                const int CIRCLE_SIZE = 10;

                var randomColor = Color.Gray;

                if (node.Group != null)
                {
                    var rand = new Random(node.Group.Id);
                    randomColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                }

                g.FillEllipse(
                    node.IsVisited ? new SolidBrush(Color.FromArgb(100, Color.Black)) : Brushes.MediumBlue,
                    node.GridPos.X - CIRCLE_SIZE / 2,
                    node.GridPos.Y - CIRCLE_SIZE / 2,
                    CIRCLE_SIZE,
                    CIRCLE_SIZE);

                foreach (var link in node.Links)
                {
                    g.DrawLine(
                        new Pen(randomColor),
                        node.GridPos.X,
                        node.GridPos.Y,
                        link.GridPos.X,
                        link.GridPos.Y);
                }

                //if (node.PriorityFromEndDistance != 0)
                //    g.DrawString(node.PriorityFromEndDistance.ToString(), font, Brushes.White, new PointF(node.GridPos.X, node.GridPos.Y));
            }

            if (navPath != null)
            {
                Node prev = null;

                foreach (var node in navPath)
                {
                    if (prev != null)
                    {
                        g.DrawLine(
                            Pens.White,
                            prev.GridPos.X,
                            prev.GridPos.Y,
                            node.GridPos.X,
                            node.GridPos.Y);
                    }

                    prev = node;
                }
            }

            if (playerNode != null)
            {
                g.DrawEllipse(
                    Pens.White,
                    playerNode.GridPos.X - _settings.PlayerVisibilityRadius,
                    playerNode.GridPos.Y - _settings.PlayerVisibilityRadius,
                    _settings.PlayerVisibilityRadius * 2,
                    _settings.PlayerVisibilityRadius * 2);

                g.DrawEllipse(
                    Pens.White,
                    playerNode.GridPos.X - _settings.PlayerVisibilityRadius - 2,
                    playerNode.GridPos.Y - _settings.PlayerVisibilityRadius - 2,
                    _settings.PlayerVisibilityRadius * 2 + 4,
                    _settings.PlayerVisibilityRadius * 2 + 4);
            }

            //Save to folder as animation
            //bitmap.Save(Path.Combine(_imageSessionFolder, $"Image_{_imageSessionNum++}.png"));

            var bImg = ConvertBitmapToBitmapImage(bitmap);

            Application.Current?.Dispatcher?.Invoke(
                () => { BitmapImage = bImg; });
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using var memory = new MemoryStream();

            bitmap.Save(memory, ImageFormat.Bmp);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
