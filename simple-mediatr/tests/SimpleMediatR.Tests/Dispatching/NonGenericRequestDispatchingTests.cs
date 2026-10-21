
namespace SimpleMediatR.Tests.Dispatching;

public sealed class NonGenericRequestDispatchingTests
{
    [Fact]
    public async Task SendShouldInvokeCorrectHandlerAsync()
    {
        // Arrange
        var handler = new DeleteUserCommandHandler();
        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);
        var request = new DeleteUserCommand(Guid.NewGuid());

        // Act
        await mediator.Send(request);

        // Assert
        Assert.True(handler.WasHandled);
        Assert.Equal(request.UserId, handler.DeletedUserId);
    }

    [Fact]
    public async Task SendShouldForwardOriginalRequestAsync()
    {
        // Arrange
        var handler = new DeleteUserCommandHandler();
        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);
        var request = new DeleteUserCommand(Guid.NewGuid());

        // Act
        await mediator.Send(request);

        // Assert
        Assert.Same(request, handler.ReceivedRequest);
    }

    [Fact]
    public async Task SendShouldForwardCancellationTokenAsync()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        var handler = new DeleteUserCommandHandler();
        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);
        var request = new DeleteUserCommand(Guid.NewGuid());
        var expectedToken = cancellationTokenSource.Token;

        // Act
        await mediator.Send(request, expectedToken);

        // Assert
        Assert.Equal(expectedToken, handler.ReceivedCancellationToken);
    }

    [Fact]
    public void SendShouldPropagateSynchronousHandlerException()
    {
        // Arrange
        var handler = new DeleteUserCommandHandler
        {
            ThrowOnHandle = true
        };

        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);
        var request = new DeleteUserCommand(Guid.NewGuid());

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
        var handler = new DeleteUserCommandHandler
        {
            ThrowAsynchronously = true
        };

        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);
        var request = new DeleteUserCommand(Guid.NewGuid());

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
        var completionSource = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var handler = new DeleteUserCommandHandler
        {
            PendingCompletion = completionSource
        };

        var serviceProvider = new TestServiceProvider(handler);
        var mediator = new SimpleMediator(serviceProvider);
        var request = new DeleteUserCommand(Guid.NewGuid());

        // Act
        var task = mediator.Send(request);

        // Assert
        Assert.False(task.IsCompleted);

        completionSource.SetResult();

        await task;

        Assert.True(task.IsCompletedSuccessfully);
    }

    [Fact]
    public void SendShouldThrowWhenHandlerIsNotRegistered()
    {
        // Arrange
        var serviceProvider = new EmptyServiceProvider();
        var mediator = new SimpleMediator(serviceProvider);
        var request = new DeleteUserCommand(Guid.NewGuid());

        Action act = () =>
        {
            _ = mediator.Send(request);
        };

        // Act
        var exception = Assert.Throws<InvalidOperationException>(act);

        // Assert
        Assert.Contains(nameof(DeleteUserCommand), exception.Message);
    }

    [Fact]
    public void SendShouldThrowWhenRequestIsNull()
    {
        // Arrange
        var handler = new DeleteUserCommandHandler();
        var serviceProvider = new TestServiceProvider(handler);
        ISender sender = new SimpleMediator(serviceProvider);

        IRequest request = null!;

        Action act = () =>
        {
            _ = sender.Send(request);
        };

        // Act
        var exception = Assert.Throws<ArgumentNullException>(act);

        // Assert
        Assert.Equal("request", exception.ParamName);
    }

    private sealed record DeleteUserCommand(Guid UserId) : IRequest;

    private sealed class DeleteUserCommandHandler
        : IRequestHandler<DeleteUserCommand>
    {
        public bool WasHandled { get; private set; }

        public Guid? DeletedUserId { get; private set; }

        public DeleteUserCommand? ReceivedRequest { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public bool ThrowOnHandle { get; set; }

        public bool ThrowAsynchronously { get; set; }

        public TaskCompletionSource? PendingCompletion { get; set; }

        public Task Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            if (ThrowOnHandle)
                throw new InvalidOperationException("Handler failed.");

            if (ThrowAsynchronously)
                return Task.FromException(
                    new InvalidOperationException("Async handler failed."));

            WasHandled = true;
            DeletedUserId = request.UserId;
            ReceivedRequest = request;
            ReceivedCancellationToken = cancellationToken;

            if (PendingCompletion is not null)
                return PendingCompletion.Task;

            return Task.CompletedTask;
        }
    }

    private sealed class TestServiceProvider(
        DeleteUserCommandHandler handler) : IServiceProvider
    {
        public object? GetService(Type serviceType)
            => serviceType == typeof(IRequestHandler<DeleteUserCommand>)
                ? handler
                : null;
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
