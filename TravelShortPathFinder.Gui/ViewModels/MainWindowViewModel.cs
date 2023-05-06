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
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Algorithm.Data;
    using Algorithm.Logic;
    using Microsoft.Xaml.Behaviors.Core;
    using Prism.Mvvm;
    using TestResources;
    using TravelShortPathFinder.Algorithm.Utils;
    using Brushes = System.Drawing.Brushes;
    using Color = System.Drawing.Color;
    using FontStyle = System.Drawing.FontStyle;
    using Pen = System.Drawing.Pen;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;
    using Point = System.Drawing.Point;

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
        private CustomRadialSweep _radialSweep;
        private List<Point> _points;
        private int _iterCounter;

        public MainWindowViewModel()
        {
            IterateCommand = new ActionCommand(DoIteration);
            StepCommand = new ActionCommand(DoStep);
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

        public int IterationsCount { get; set; } = 5;
        public ICommand IterateCommand { get; }
        public ICommand StepCommand { get; }

        private void Explore()
        {
            var navCase = InputNavCases.Case1;
            _settings = navCase.Settings;

            _imageSessionFolder = Path.Combine("OutImg"); //, Path.GetFileNameWithoutExtension(Path.GetTempFileName())
            Directory.CreateDirectory(_imageSessionFolder);

            // Application.Current.Dispatcher.Invoke(
            //     () => { BitmapImage = ConvertBitmapToBitmapImage(navCase.Bitmap); });
            //Thread.Sleep(1000);
            _navGrid = NavGridProvider.FromBitmap(navCase.Bitmap);
            _explorer = new GraphMapExplorer(_navGrid, _settings);
            _graph = _explorer.Graph;

            _iterCounter = 1;
            _radialSweep = new CustomRadialSweep(_navGrid);
            _points = new List<Point>();

            _radialSweep.InitLoop(new Point(51, 28), Direction.Top);

            RepaintBitmap(null, null, true);
        }

        private void DoIteration()
        {
            while (!_radialSweep.ProcessNext(_iterCounter, _points))
            {
            }

            InitNewCircle();
            RepaintBitmap(null, null, true);
        }

        private void DoStep()
        {
            if (_radialSweep.ProcessNext(_iterCounter, _points))
            {
                InitNewCircle();
            }

            RepaintBitmap(null, null, true);
        }

        private void InitNewCircle()
        {
            var nextPoint = FindNextProcessPoint(_points);
            _points.Clear();

            if (nextPoint != null)
            {
                _iterCounter++;
                _radialSweep.InitLoop(nextPoint.Item1, nextPoint.Item2);
            }
        }

        private Tuple<Point, Direction> FindNextProcessPoint(List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                var contourPoint = points[i];

                foreach (var keyValuePair in CustomRadialSweepUtils.OffsetFromDirection)
                {
                    var nextOffset = keyValuePair.Value;
                    var checkPos = new Point(contourPoint.X + nextOffset.X, contourPoint.Y + nextOffset.Y);

                    if (_navGrid.WalkArray[checkPos.X, checkPos.Y] == WalkableFlag.Walkable)
                    {
                        return new Tuple<Point, Direction>(checkPos, keyValuePair.Key);
                    }
                }
            }

            return null;
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
            //pData[y * _navGrid.Width + x] = blackArgb;
            //is the same as:
            //bitmap.SetPixel(x, y, Color.Black);
            Parallel.For(
                0,
                _navGrid.Width * _navGrid.Height,
                i =>
                {
                    var y = i / _navGrid.Width;
                    var x = i % _navGrid.Width;

                    var navVal = _navGrid.NavArray[x, y];
                    var gridVal = navVal.Flag;

                    if (navVal.IterationId > 0)
                    {
                        if (navVal.ColorId == 1)
                        {
                            pData![y * _navGrid.Width + x] = blackArgb; 
                        }
                        else
                        {
                            var rand = new Random(navVal.IterationId);
                            //var alpha = navVal.IterationId * 10 + 50;
                            //var randomColor = Color.FromArgb(alpha, alpha, alpha);
                            var randomColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                            pData![y * _navGrid.Width + x] = randomColor.ToArgb(); 
                        }
                    }
                    else if ((gridVal & WalkableFlag.NonWalkable) != 0)
                    {
                        pData![y * _navGrid.Width + x] = blackArgb;
                    }
                    else if (gridVal == WalkableFlag.PossibleSegmentStart)
                    {
                        pData![y * _navGrid.Width + x] = redArgb;
                    }
                    else if (gridVal == WalkableFlag.PossibleSegmentProcessed)
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
                                    seed = node.GraphPart?.Id ?? node.Id;
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
            pData![_radialSweep.CurProcessPoint.Y * _navGrid.Width + _radialSweep.CurProcessPoint.X] = orangeRedArgb;
            

            bitmap.UnlockBits(data);

            using var g = Graphics.FromImage(bitmap);

            foreach (var node in _graph.Nodes)
            {
                const int CIRCLE_SIZE = 10;

                var randomColor = Color.Gray;

                if (node.GraphPart != null)
                {
                    var rand = new Random(node.GraphPart.Id);
                    randomColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                }

                g.FillEllipse(
                    node.IsVisited ? new SolidBrush(Color.FromArgb(100, Color.Black)) : Brushes.MediumBlue,
                    node.Pos.X - CIRCLE_SIZE / 2,
                    node.Pos.Y - CIRCLE_SIZE / 2,
                    CIRCLE_SIZE,
                    CIRCLE_SIZE);

                foreach (var link in node.Links)
                {
                    g.DrawLine(
                        new Pen(randomColor),
                        node.Pos.X,
                        node.Pos.Y,
                        link.Pos.X,
                        link.Pos.Y);
                }

                //if (node.PriorityFromEndDistance != 0)
                //    g.DrawString(node.PriorityFromEndDistance.ToString(), font, Brushes.White, new PointF(node.Pos.X, node.Pos.Y));
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
                            prev.Pos.X,
                            prev.Pos.Y,
                            node.Pos.X,
                            node.Pos.Y);
                    }

                    prev = node;
                }
            }

            if (playerNode != null)
            {
                g.DrawEllipse(
                    Pens.White,
                    playerNode.Pos.X - _settings.PlayerVisibilityRadius,
                    playerNode.Pos.Y - _settings.PlayerVisibilityRadius,
                    _settings.PlayerVisibilityRadius * 2,
                    _settings.PlayerVisibilityRadius * 2);

                g.DrawEllipse(
                    Pens.White,
                    playerNode.Pos.X - _settings.PlayerVisibilityRadius - 2,
                    playerNode.Pos.Y - _settings.PlayerVisibilityRadius - 2,
                    _settings.PlayerVisibilityRadius * 2 + 4,
                    _settings.PlayerVisibilityRadius * 2 + 4);
            }

            //Save to folder as animation
            bitmap.Save(Path.Combine(_imageSessionFolder, $"Image_{_imageSessionNum++}.png"));

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
