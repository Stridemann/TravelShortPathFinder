namespace TravelShortPathFinder.Benchmarks
{
    using Algorithm.Data;
    using Algorithm.Logic;
    using BenchmarkDotNet.Attributes;
    using TestResources;
    using TravelShortPathFinder.Algorithm.Utils;

    [InProcess]
    public class SegmentationBenchmark
    {
        [ParamsSource(nameof(Parameters))] public InputNavCase InputNavCase;
        private GraphMapExplorer _explorer;

        [IterationSetup]
        public void GlobalSetup()
        {
            var _settings = new Settings
            {
                SegmentationSquareSize = 40,
                SegmentationMinSegmentSize = 200,
                PlayerVisibilityRadius = 50
            };
            var navGrid = NavGridProvider.FromBitmap(InputNavCase.Bitmap);

            _explorer = new GraphMapExplorer(navGrid, _settings, new DefaultNextNodeSelector(_settings));
        }

        [Benchmark]
        public void BenchmarkSegmentation()
        {
            _explorer.ProcessSegmentation(InputNavCase.StartPoint);
        }

        public IEnumerable<InputNavCase> Parameters()
        {
            yield return InputNavCases.Case1;
            yield return InputNavCases.Case2;
            yield return InputNavCases.Case3;
            yield return InputNavCases.Case4;
        }
    }
}
