
using System.Reflection;
using BenchmarkDotNet.Attributes;
using SimpleMediatR.Internal;

namespace SimpleMediatR.Benchmarks;

/// <summary>
/// Compares reflection-based handler invocation with cached compiled delegates.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class DispatchInvocationBenchmarks
{
    private readonly BenchmarkQuery _request = new();
    private readonly BenchmarkHandler _handler = new();

    private readonly MethodInfo _handleMethod =
        typeof(IRequestHandler<BenchmarkQuery, int>)
            .GetMethod("Handle")!;

    private readonly Func<object, object, CancellationToken, object> _invoker =
        DispatcherCache.GetGenericInvoker(
            typeof(BenchmarkQuery),
            typeof(int));

    /// <summary>
    /// Invokes the request handler using reflection.
    /// </summary>
    /// <returns>The task containing the handler response.</returns>
    [Benchmark(Baseline = true)]
    public Task<int> Reflection()
        => (Task<int>)_handleMethod.Invoke(
            _handler,
            [_request, CancellationToken.None])!;

    /// <summary>
    /// Invokes the request handler using a cached compiled delegate.
    /// </summary>
    /// <returns>The task containing the handler response.</returns>
    [Benchmark]
    public Task<int> CachedDelegate()
        => (Task<int>)_invoker(
            _handler,
            _request,
            CancellationToken.None);

    private sealed record BenchmarkQuery : IRequest<int>;

    private sealed class BenchmarkHandler
        : IRequestHandler<BenchmarkQuery, int>
    {
        public Task<int> Handle(
            BenchmarkQuery request,
            CancellationToken cancellationToken)
            => Task.FromResult(42);
    }
}
