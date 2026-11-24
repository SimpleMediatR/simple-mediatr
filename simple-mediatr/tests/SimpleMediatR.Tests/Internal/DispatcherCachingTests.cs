
using Microsoft.Extensions.DependencyInjection;
using SimpleMediatR.Internal;

namespace SimpleMediatR.Tests.Internal;

public sealed class DispatcherCachingTests
{
    [Fact]
    public async Task SendShouldDispatchConcurrentGenericRequestsCorrectlyAsync()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddTransient<IRequestHandler<GetValueQuery, int>>(
            _ => new GetValueQueryHandler());

        using var serviceProvider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(serviceProvider);
        var expectedValues = Enumerable.Range(1, 100).ToArray();

        // Act
        var tasks = expectedValues
            .Select(value => Task.Run(
                () => mediator.Send(new GetValueQuery(value))))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(expectedValues, results);
    }

    [Fact]
    public void GenericInvokerShouldBeReusedForSameRequestType()
    {
        // Arrange
        var requestType = typeof(GetValueQuery);
        var responseType = typeof(int);

        // Act
        var firstInvoker = DispatcherCache.GetGenericInvoker(
            requestType,
            responseType);

        var secondInvoker = DispatcherCache.GetGenericInvoker(
            requestType,
            responseType);

        // Assert
        Assert.Same(firstInvoker, secondInvoker);
    }

    [Fact]
    public void NonGenericInvokerShouldBeReusedForSameRequestType()
    {
        // Arrange
        var requestType = typeof(DeleteValueCommand);

        // Act
        var firstInvoker = DispatcherCache.GetNonGenericInvoker(requestType);
        var secondInvoker = DispatcherCache.GetNonGenericInvoker(requestType);

        // Assert
        Assert.Same(firstInvoker, secondInvoker);
    }

    [Fact]
    public async Task GenericInvokerShouldBeThreadSafeAsync()
    {
        // Arrange
        var requestType = typeof(ConcurrentQuery);
        var responseType = typeof(int);

        // Act
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(
                () => DispatcherCache.GetGenericInvoker(
                    requestType,
                    responseType)))
            .ToArray();

        var invokers = await Task.WhenAll(tasks);

        // Assert
        var expectedInvoker = invokers[0];

        Assert.All(invokers, invoker =>
            Assert.Same(expectedInvoker, invoker));
    }

    [Fact]
    public async Task NonGenericInvokerShouldBeThreadSafeAsync()
    {
        // Arrange
        var requestType = typeof(ConcurrentCommand);

        // Act
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(
                () => DispatcherCache.GetNonGenericInvoker(requestType)))
            .ToArray();

        var invokers = await Task.WhenAll(tasks);

        // Assert
        var expectedInvoker = invokers[0];

        Assert.All(invokers, invoker =>
            Assert.Same(expectedInvoker, invoker));
    }

    [Fact]
    public async Task SendShouldNotCacheScopedHandlerInstancesAsync()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddScoped<IRequestHandler<GetScopedIdQuery, Guid>>(
            _ => new ScopedIdHandler());

        using var serviceProvider = services.BuildServiceProvider(
            validateScopes: true);

        using var firstScope = serviceProvider.CreateScope();
        using var secondScope = serviceProvider.CreateScope();

        var firstMediator = new SimpleMediator(firstScope.ServiceProvider);
        var secondMediator = new SimpleMediator(secondScope.ServiceProvider);
        var request = new GetScopedIdQuery();

        // Act
        var firstId = await firstMediator.Send(request);
        var secondId = await firstMediator.Send(request);
        var thirdId = await secondMediator.Send(request);

        var firstInvoker = DispatcherCache.GetGenericInvoker(
            typeof(GetScopedIdQuery),
            typeof(Guid));

        var secondInvoker = DispatcherCache.GetGenericInvoker(
            typeof(GetScopedIdQuery),
            typeof(Guid));

        // Assert
        Assert.Equal(firstId, secondId);
        Assert.NotEqual(firstId, thirdId);
        Assert.Same(firstInvoker, secondInvoker);
    }

    private sealed record GetValueQuery(int Value) : IRequest<int>;

    private sealed record DeleteValueCommand(int Value) : IRequest;

    private sealed record ConcurrentQuery : IRequest<int>;

    private sealed record ConcurrentCommand : IRequest;

    private sealed record GetScopedIdQuery : IRequest<Guid>;

    private sealed class GetValueQueryHandler
        : IRequestHandler<GetValueQuery, int>
    {
        public Task<int> Handle(
            GetValueQuery request,
            CancellationToken cancellationToken)
            => Task.FromResult(request.Value);
    }

    private sealed class DeleteValueCommandHandler
        : IRequestHandler<DeleteValueCommand>
    {
        public Task Handle(
            DeleteValueCommand request,
            CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class ScopedIdHandler
        : IRequestHandler<GetScopedIdQuery, Guid>
    {
        private readonly Guid _instanceId = Guid.NewGuid();

        public Task<Guid> Handle(
            GetScopedIdQuery request,
            CancellationToken cancellationToken)
            => Task.FromResult(_instanceId);
    }
}
