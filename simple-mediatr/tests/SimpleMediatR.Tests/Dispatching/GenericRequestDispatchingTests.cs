
using SimpleMediatR;

namespace SimpleMediatR.Tests.Dispatching;

public sealed class GenericRequestDispatchingTests
{
    [Fact]
    public async Task SendShouldReturnHandlerResponseAsync()
    {
        // Arrange
        var handler = new GetUserQueryHandler();
        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);
        var request = new GetUserQuery(Guid.NewGuid());

        // Act
        UserDto result = await mediator.Send(request);

        // Assert
        Assert.Equal(request.UserId, result.Id);
        Assert.Equal("Test User", result.Name);
        Assert.Same(request, handler.ReceivedRequest);
    }

    private sealed record GetUserQuery(Guid UserId) : IRequest<UserDto>;

    private sealed record UserDto(Guid Id, string Name);

    private sealed class GetUserQueryHandler
        : IRequestHandler<GetUserQuery, UserDto>
    {
        public GetUserQuery? ReceivedRequest { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public bool ThrowOnHandle { get; set; }

        public bool ThrowAsynchronously { get; set; }

        public TaskCompletionSource<UserDto>? PendingResponse { get; set; }

        public Task<UserDto> Handle(
            GetUserQuery request,
            CancellationToken cancellationToken)
        {
            if (ThrowOnHandle)
                throw new InvalidOperationException("Handler failed.");

            if (ThrowAsynchronously)
                return Task.FromException<UserDto>(
                    new InvalidOperationException("Async handler failed."));

            ReceivedRequest = request;
            ReceivedCancellationToken = cancellationToken;

            if (PendingResponse is not null)
                return PendingResponse.Task;

            return Task.FromResult(
                new UserDto(request.UserId, "Test User"));
        }
    }

    private sealed class TestServiceProvider(
        GetUserQueryHandler handler) : IServiceProvider
    {
        public object? GetService(Type serviceType)
            => serviceType == typeof(IRequestHandler<GetUserQuery, UserDto>)
                ? handler
                : null;
    }

    [Fact]
    public async Task SendShouldForwardCancellationTokenAsync()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        var handler = new GetUserQueryHandler();
        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);
        var request = new GetUserQuery(Guid.NewGuid());

        // Act
        await mediator.Send(request, cancellationTokenSource.Token);

        // Assert
        Assert.Equal(
            cancellationTokenSource.Token,
            handler.ReceivedCancellationToken);
    }

    [Fact]
    public void SendShouldPropagateSynchronousHandlerException()
    {
        // Arrange
        var handler = new GetUserQueryHandler
        {
            ThrowOnHandle = true
        };

        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);
        var request = new GetUserQuery(Guid.NewGuid());

        Action act = () =>
        {
            _ = mediator.Send(request);
        };

        // Act
        var exception = Assert.Throws<InvalidOperationException>(act);

        // Assert
        Assert.Equal("Handler failed.", exception.Message);
    }

    [Fact]
    public async Task SendShouldPropagateAsynchronousHandlerExceptionAsync()
    {
        // Arrange
        var handler = new GetUserQueryHandler
        {
            ThrowAsynchronously = true
        };

        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);
        var request = new GetUserQuery(Guid.NewGuid());

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await mediator.Send(request));

        // Assert
        Assert.Equal("Async handler failed.", exception.Message);
    }

    [Fact]
    public async Task SendShouldWaitForHandlerCompletionAsync()
    {
        // Arrange
        var completionSource = new TaskCompletionSource<UserDto>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var handler = new GetUserQueryHandler
        {
            PendingResponse = completionSource
        };

        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);
        var request = new GetUserQuery(Guid.NewGuid());

        // Act
        Task<UserDto> responseTask = mediator.Send(request);

        // Assert
        Assert.False(responseTask.IsCompleted);

        completionSource.SetResult(
            new UserDto(request.UserId, "Completed User"));

        UserDto result = await responseTask;

        Assert.Equal(request.UserId, result.Id);
        Assert.Equal("Completed User", result.Name);
    }

    [Fact]
    public void SendShouldThrowWhenHandlerIsNotRegistered()
    {
        // Arrange
        var serviceProvider = new EmptyServiceProvider();
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
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [Fact]
    public void SendShouldThrowWhenRequestIsNull()
    {
        // Arrange
        var handler = new GetUserQueryHandler();
        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);

        Action act = () =>
        {
            _ = mediator.Send<UserDto>(null!);
        };

        // Act
        var exception = Assert.Throws<ArgumentNullException>(act);

        // Assert
        Assert.Equal("request", exception.ParamName);
    }
}
