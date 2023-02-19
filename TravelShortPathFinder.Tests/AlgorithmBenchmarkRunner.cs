namespace TravelShortPathFinder.Tests
{
    using System.Numerics;
    using Algorithm.Data;
    using Algorithm.Logic;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Code;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Loggers;
    using BenchmarkDotNet.Running;
    using TestResources;
    using Xunit.Abstractions;

    [MemoryDiagnoser]
    [InProcessAttribute]
    public class AlgorithmBenchmarkRunner
    {
        public AlgorithmBenchmarkRunner(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        [Fact]
        public void TestPerformance__HeavyMethod__ShouldAllocateNothing()
        {
            var logger = new AccumulationLogger();

            var config = ManualConfig.Create(DefaultConfig.Instance)
                                     .AddLogger(logger)
                                     .WithOptions(ConfigOptions.DisableOptimizationsValidator);

            BenchmarkRunner.Run<AlgorithmBenchmark>(config);

            // write benchmark summary
            _output.WriteLine(logger.GetLog());
        }
    }

    [InProcessAttribute]
    public class AlgorithmBenchmark
    {
        private GraphMapExplorer _explorer;
        private Node _curPlayerNode;

        public AlgorithmBenchmark()
        {
            //GlobalSetup();
        }

        [IterationSetup]
        public void GlobalSetup()
        {
            var _settings = new Settings
            {
                SegmentationSquareSize = 40,
                SegmentationMinSegmentSize = 200,
                PlayerVisibilityRadius = 50
            };
            //InputNavCase = new Tuple<InputNavCase, int>(InputNavCases.Case5, 643);
            var navCase = InputNavCase.Item1;
            var _navGrid = NavGridProvider.FromBitmap(navCase.Bitmap);
            var _graph = new Graph(_navGrid);

            _explorer = new GraphMapExplorer(_settings, _graph, new DefaultNextNodeSelector(_settings));
            //var _playerPos = new Vector2(navCase.StartPoint.X, navCase.StartPoint.Y);
            //_explorer.ProcessSegmentation(_playerPos);
            //_curPlayerNode = _graph.Nodes.First();

            //if (_curPlayerNode == null)
            //    throw new InvalidOperationException("_curPlayerNode == null");
        }

        [Benchmark]
        public void BenchmarkSegmentation()
        {
            var navCase = InputNavCase.Item1;
            var _playerPos = new Vector2(navCase.StartPoint.X, navCase.StartPoint.Y);
            _explorer.ProcessSegmentation(_playerPos);
        }

        //[Benchmark]
        //public void BenchmarkUpdate()
        //{
        //    //var counter = 0;

        //    //if (_curPlayerNode == null)
        //    //    throw new InvalidOperationException("_curPlayerNode == null");

        //    //do
        //    //{
        //    //    counter++;
        //    //    _explorer.Update(_curPlayerNode.GridPos);
        //    //    _curPlayerNode = _explorer.NextRunNode;
        //    //} while (_curPlayerNode != null);

        //    for (int i = 0; i < InputNavCase.Item2; i++)
        //    {
        //        _explorer.Update(_curPlayerNode.GridPos);
        //        _curPlayerNode = _explorer.NextRunNode;
        //    }
        //}

        [ParamsSource(nameof(Parameters))] public Tuple<InputNavCase, int> InputNavCase;

        public IEnumerable<Tuple<InputNavCase, int>> Parameters()
        {
            yield return new Tuple<InputNavCase, int>(InputNavCases.Case1, 6);
            yield return new Tuple<InputNavCase, int>(InputNavCases.Case2, 133);
            yield return new Tuple<InputNavCase, int>(InputNavCases.Case3, 108);
            yield return new Tuple<InputNavCase, int>(InputNavCases.Case4, 648);
            yield return new Tuple<InputNavCase, int>(InputNavCases.Case5, 643);
        }
    }
}
