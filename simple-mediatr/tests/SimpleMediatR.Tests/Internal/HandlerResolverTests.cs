
using Microsoft.Extensions.DependencyInjection;

namespace SimpleMediatR.Tests.Internal;

public sealed class HandlerResolverTests
{
    [Fact]
    public async Task SendShouldResolveHandlerWithConstructorDependenciesAsync()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSingleton<TestDependency>();
        services.AddTransient<
            IRequestHandler<GetUserQuery, string>,
            GetUserQueryHandler>();

        using var serviceProvider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(serviceProvider);
        var request = new GetUserQuery(Guid.NewGuid());

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Hello from dependency", result);
    }

    [Fact]
    public async Task SendShouldResolveNewTransientHandlerForEachRequestAsync()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddTransient<TransientHandler>();
        services.AddTransient<
            IRequestHandler<GetHandlerIdQuery, Guid>>(
                provider => provider.GetRequiredService<TransientHandler>());

        using var serviceProvider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(serviceProvider);
        var request = new GetHandlerIdQuery();

        // Act
        var firstHandlerId = await mediator.Send(request);
        var secondHandlerId = await mediator.Send(request);

        // Assert
        Assert.NotEqual(firstHandlerId, secondHandlerId);
    }

    [Fact]
    public async Task SendShouldRespectScopedHandlerLifetimeAsync()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddScoped<
            IRequestHandler<GetHandlerIdQuery, Guid>,
            ScopedHandler>();

        using var serviceProvider = services.BuildServiceProvider(
            validateScopes: true);

        using var firstScope = serviceProvider.CreateScope();
        using var secondScope = serviceProvider.CreateScope();

        var firstMediator = new SimpleMediator(firstScope.ServiceProvider);
        var secondMediator = new SimpleMediator(secondScope.ServiceProvider);
        var request = new GetHandlerIdQuery();

        // Act
        var firstId = await firstMediator.Send(request);
        var secondId = await firstMediator.Send(request);
        var thirdId = await secondMediator.Send(request);

        // Assert
        Assert.Equal(firstId, secondId);
        Assert.NotEqual(firstId, thirdId);
    }

    [Fact]
    public async Task SendShouldRespectSingletonHandlerLifetimeAsync()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSingleton<
            IRequestHandler<GetHandlerIdQuery, Guid>,
            SingletonHandler>();

        using var serviceProvider = services.BuildServiceProvider(
            validateScopes: true);

        using var firstScope = serviceProvider.CreateScope();
        using var secondScope = serviceProvider.CreateScope();

        var firstMediator = new SimpleMediator(firstScope.ServiceProvider);
        var secondMediator = new SimpleMediator(secondScope.ServiceProvider);
        var request = new GetHandlerIdQuery();

        // Act
        var firstId = await firstMediator.Send(request);
        var secondId = await firstMediator.Send(request);
        var thirdId = await secondMediator.Send(request);

        // Assert
        Assert.Equal(firstId, secondId);
        Assert.Equal(firstId, thirdId);
    }

    [Fact]
    public void SendShouldThrowWhenHandlerIsNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        using var serviceProvider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(serviceProvider);
        var request = new GetUserQuery(Guid.NewGuid());

        Action act = () =>
        {
            _ = mediator.Send(request);
        };

        // Act
        var exception = Assert.Throws<InvalidOperationException>(act);

        // Assert
        Assert.Contains(nameof(GetUserQuery), exception.Message);
        Assert.Contains("IRequestHandler", exception.Message);
        Assert.Contains("Register an implementation", exception.Message);
    }

    [Fact]
    public void SendShouldPropagateInvalidHandlerRegistrationException()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddTransient<
            IRequestHandler<GetUserQuery, string>,
            GetUserQueryHandler>();

        using var serviceProvider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(serviceProvider);
        var request = new GetUserQuery(Guid.NewGuid());

        Action act = () =>
        {
            _ = mediator.Send(request);
        };

        // Act
        var exception = Assert.Throws<InvalidOperationException>(act);

        // Assert
        Assert.Contains(nameof(TestDependency), exception.Message);
        Assert.Contains(nameof(GetUserQueryHandler), exception.Message);
    }

    [Fact]
    public async Task SendShouldUseCurrentScopeForHandlerDependenciesAsync()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddScoped<TestScopedDependency>();
        services.AddTransient<
            IRequestHandler<GetScopedDependencyQuery, Guid>,
            ScopedDependencyHandler>();

        using var serviceProvider = services.BuildServiceProvider(
            validateScopes: true);

        using var scope = serviceProvider.CreateScope();

        var expectedDependency = scope.ServiceProvider
            .GetRequiredService<TestScopedDependency>();

        var mediator = new SimpleMediator(scope.ServiceProvider);
        var request = new GetScopedDependencyQuery();

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal(expectedDependency.Id, result);
    }

    private sealed record GetScopedDependencyQuery : IRequest<Guid>;

    private sealed record GetUserQuery(Guid UserId) : IRequest<string>;

    private sealed record GetHandlerIdQuery : IRequest<Guid>;

    private sealed class TestDependency
    {
        private readonly string _message = "Hello from dependency";

        public string Message => _message;
    }

    private sealed class GetUserQueryHandler(TestDependency dependency)
        : IRequestHandler<GetUserQuery, string>
    {
        public Task<string> Handle(
            GetUserQuery request,
            CancellationToken cancellationToken)
            => Task.FromResult(dependency.Message);
    }

    private sealed class TransientHandler
        : IRequestHandler<GetHandlerIdQuery, Guid>
    {
        private readonly Guid _instanceId = Guid.NewGuid();

        public Task<Guid> Handle(
            GetHandlerIdQuery request,
            CancellationToken cancellationToken)
            => Task.FromResult(_instanceId);
    }

    private sealed class ScopedHandler
        : IRequestHandler<GetHandlerIdQuery, Guid>
    {
        private readonly Guid _instanceId = Guid.NewGuid();

        public Task<Guid> Handle(
            GetHandlerIdQuery request,
            CancellationToken cancellationToken)
            => Task.FromResult(_instanceId);
    }

    private sealed class SingletonHandler
    : IRequestHandler<GetHandlerIdQuery, Guid>
    {
        private readonly Guid _instanceId = Guid.NewGuid();

        public Task<Guid> Handle(
            GetHandlerIdQuery request,
            CancellationToken cancellationToken)
            => Task.FromResult(_instanceId);
    }

    private sealed class TestScopedDependency
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    private sealed class ScopedDependencyHandler(
        TestScopedDependency dependency)
        : IRequestHandler<GetScopedDependencyQuery, Guid>
    {
        public Task<Guid> Handle(
            GetScopedDependencyQuery request,
            CancellationToken cancellationToken)
            => Task.FromResult(dependency.Id);
    }
}
