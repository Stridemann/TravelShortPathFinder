namespace TravelShortPathFinder.Benchmarks
{
    using Algorithm.Data;
    using Algorithm.Logic;
    using BenchmarkDotNet.Attributes;
    using TestResources;
    using TravelShortPathFinder.Algorithm.Utils;

    [InProcess]
    public class NextPointBenchmark
    {
        [ParamsSource(nameof(Parameters))] public InputNavCase InputNavCase;
        private GraphMapExplorer _explorer;
        private Node? _curPlayerNode;

        [IterationSetup]
        public void GlobalSetup()
        {
            var navGrid = NavGridProvider.FromBitmap(InputNavCase.Bitmap);
            _explorer = new GraphMapExplorer(navGrid, InputNavCase.Settings);
            _explorer.ProcessSegmentation(InputNavCase.StartPoint);
        }

        [Benchmark]
        public void BenchmarkUpdate()
        {
            _curPlayerNode = _explorer.Graph.Nodes.First();

            do
            {
                _explorer.Update(_curPlayerNode.Pos);
                _curPlayerNode = _explorer.NextRunNode;
            } while (_curPlayerNode != null);
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
