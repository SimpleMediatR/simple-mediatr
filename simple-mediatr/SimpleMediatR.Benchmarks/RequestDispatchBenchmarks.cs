
using Microsoft.Extensions.DependencyInjection;
using BenchmarkDotNet.Attributes;

namespace SimpleMediatR.Benchmarks;

/// <summary>
/// Compares reflection-based request dispatching with cached dispatching.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class RequestDispatchBenchmarks
{
    private readonly BenchmarkQuery _request = new();

    private ServiceProvider? _serviceProvider;
    private SimpleMediator? _mediator;

    /// <summary>
    /// Initializes the dependency injection container before benchmarking.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddTransient<IRequestHandler<BenchmarkQuery, int>>(
            _ => new BenchmarkHandler());

        _serviceProvider = services.BuildServiceProvider();
        _mediator = new SimpleMediator(_serviceProvider);
    }

    /// <summary>
    /// Dispatches a request using the original reflection-based implementation.
    /// </summary>
    /// <returns>The task containing the handler response.</returns>
    [Benchmark(Baseline = true)]
    public Task<int> ReflectionDispatch()
    {
        var requestType = _request.GetType();

        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, typeof(int));

        var handler = _serviceProvider!.GetService(handlerType)
            ?? throw new InvalidOperationException("Handler not registered.");

        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException("Handle method not found.");

        return (Task<int>)handleMethod.Invoke(
            handler,
            [_request, CancellationToken.None])!;
    }

    /// <summary>
    /// Dispatches a request using the cached SimpleMediator implementation.
    /// </summary>
    /// <returns>The task containing the handler response.</returns>
    [Benchmark]
    public Task<int> CachedDispatch()
        => _mediator!.Send(_request);

    /// <summary>
    /// Disposes the dependency injection container after benchmarking.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
        => _serviceProvider?.Dispose();

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
