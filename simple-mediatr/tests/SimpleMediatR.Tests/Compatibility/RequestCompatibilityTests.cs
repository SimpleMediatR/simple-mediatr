
namespace SimpleMediatR.Tests.Compatibility;

public sealed class RequestCompatibilityTests
{
    [Fact]
    public void GenericRequestShouldImplementIBaseRequest()
    {
        // Arrange
        var request = new GetUserQuery(Guid.NewGuid());

        // Act
        IBaseRequest baseRequest = request;

        // Assert
        Assert.IsType<GetUserQuery>(baseRequest);
    }

    [Fact]
    public void NonGenericRequestShouldImplementIBaseRequest()
    {
        // Arrange
        var request = new DeleteUserCommand(Guid.NewGuid());

        // Act
        IBaseRequest baseRequest = request;

        // Assert
        Assert.IsType<DeleteUserCommand>(baseRequest);
    }

    [Fact]
    public async Task GenericHandlerShouldReturnExpectedResponseAsync()
    {
        // Arrange
        var handler = new GetUserQueryHandler();
        var request = new GetUserQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(request.UserId, result.Id);
    }

    [Fact]
    public async Task NonGenericHandlerShouldHandleCommandAsync()
    {
        // Arrange
        var handler = new DeleteUserCommandHandler();
        var request = new DeleteUserCommand(Guid.NewGuid());

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(request.UserId, handler.DeletedUserId);
    }

    [Fact]
    public void SenderShouldSupportGenericRequest()
    {
        // Arrange
        var request = new GetUserQuery(Guid.NewGuid());

        // Act
        Func<ISender, Task<UserDto>> send =
            sender => sender.Send(request);

        // Assert
        Assert.NotNull(send);
    }

    [Fact]
    public void SenderShouldSupportNonGenericRequest()
    {
        // Arrange
        var request = new DeleteUserCommand(Guid.NewGuid());

        // Act
        Func<ISender, Task> send =
            sender => sender.Send(request);

        // Assert
        Assert.NotNull(send);
    }

    [Fact]
    public async Task PipelineDelegateShouldSupportContinuationAsync()
    {
        // Arrange
        RequestHandlerDelegate<UserDto> next = HandleRequest;

        // Act
        var result = await ExecutePipelineAsync(next);

        // Assert
        Assert.Equal("Test User", result.Name);
    }

    private sealed record GetUserQuery(Guid UserId) : IRequest<UserDto>;

    private sealed record UserDto(Guid Id, string Name);

    private sealed record DeleteUserCommand(Guid UserId) : IRequest;

    private sealed class GetUserQueryHandler
        : IRequestHandler<GetUserQuery, UserDto>
    {
        public Task<UserDto> Handle(
            GetUserQuery request,
            CancellationToken cancellationToken)
            => Task.FromResult(
                new UserDto(request.UserId, "Test User"));
    }

    private sealed class DeleteUserCommandHandler
        : IRequestHandler<DeleteUserCommand>
    {
        public Guid? DeletedUserId { get; private set; }

        public Task Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            DeletedUserId = request.UserId;

            return Task.CompletedTask;
        }
    }

    private static Task<UserDto> HandleRequest(
        CancellationToken cancellationToken = default)
        => Task.FromResult(
            new UserDto(Guid.NewGuid(), "Test User"));

    private static async Task<TResponse> ExecutePipelineAsync<TResponse>(
        RequestHandlerDelegate<TResponse> next)
        => await next();
}
