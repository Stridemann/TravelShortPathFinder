namespace TravelShortPathFinder.Benchmarks
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Loggers;
    using BenchmarkDotNet.Running;
    using Xunit;
    using Xunit.Abstractions;

    [MemoryDiagnoser]
    [InProcessAttribute]
    public class AlgorithmBenchmarkRunner
    {
        private readonly ITestOutputHelper _output;

        public AlgorithmBenchmarkRunner(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void NextPointBenchmark()
        {
            var logger = new AccumulationLogger();

            var config = ManualConfig.Create(DefaultConfig.Instance)
                                     .AddLogger(logger)
                                     .WithOptions(ConfigOptions.DisableOptimizationsValidator);

            BenchmarkRunner.Run<NextPointBenchmark>(config);

            // write benchmark summary
            _output.WriteLine(logger.GetLog());
        }

        [Fact]
        public void SegmentationBenchmark()
        {
            var logger = new AccumulationLogger();

            var config = ManualConfig.Create(DefaultConfig.Instance)
                                     .AddLogger(logger)
                                     .WithOptions(ConfigOptions.DisableOptimizationsValidator);

            BenchmarkRunner.Run<SegmentationBenchmark>(config);

            // write benchmark summary
            _output.WriteLine(logger.GetLog());
        }
    }
}
