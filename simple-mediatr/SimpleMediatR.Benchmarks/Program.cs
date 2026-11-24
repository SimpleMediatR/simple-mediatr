
using BenchmarkDotNet.Running;

namespace SimpleMediatR.Benchmarks;

internal static class Program
{
    private static void Main()
        => BenchmarkRunner.Run<RequestDispatchBenchmarks>();
}
