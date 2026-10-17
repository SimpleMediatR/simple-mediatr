namespace SimpleMediatR.Tests.Abstractions;

public sealed class RequestHandlerTests
{
    [Fact]
    public async Task HandlerShouldHandleRequestWithoutResponseAsync()
    {
        // Arrange
        var handler = new TestCommandHandler();
        var request = new TestCommand();

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(handler.WasHandled);
    }

    private sealed record TestCommand : IRequest;

    private sealed class TestCommandHandler : IRequestHandler<TestCommand>
    {
        public bool WasHandled { get; private set; }

        public Task Handle(TestCommand request, CancellationToken cancellationToken)
        {
            WasHandled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task GenericHandlerShouldReturnExpectedResponse()
    {
        // Arrange
        var handler = new TestQueryHandler();
        var request = new TestQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal("Hello SimpleMediatR", result);
    }

    private sealed record TestQuery : IRequest<string>;

    private sealed class TestQueryHandler : IRequestHandler<TestQuery, string>
    {
        public Task<string> Handle(TestQuery request, CancellationToken cancellationToken) => Task.FromResult("Hello SimpleMediatR");
    }
}
