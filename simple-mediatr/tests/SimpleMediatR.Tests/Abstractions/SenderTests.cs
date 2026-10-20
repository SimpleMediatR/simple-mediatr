
namespace SimpleMediatR.Tests.Abstractions;

public sealed class SenderTests
{
    [Fact]
    public async Task SendWithResponseShouldReturnExpectedResultAsync()
    {
        // Arrange
        ISender sender = new TestSender();
        var request = new TestQuery();

        // Act
        var result = await sender.Send(request);

        // Assert
        Assert.Equal("Hello SimpleMediatR", result);
    }

    [Fact]
    public async Task SendWithoutResponseShouldCompleteAsync()
    {
        // Arrange
        var sender = new TestSender();
        var request = new TestCommand();

        // Act
        await sender.Send(request);

        // Assert
        Assert.True(sender.WasHandled);
    }

    [Fact]
    public async Task SendShouldForwardCancellationTokenAsync()
    {
        // Arrange
        var sender = new TestSender();
        using var cancellationTokenSource = new CancellationTokenSource();
        var expectedToken = cancellationTokenSource.Token;

        // Act
        await sender.Send(new TestCommand(), expectedToken);

        // Assert
        Assert.Equal(expectedToken, sender.ReceivedToken);
    }

    [Fact]
    public async Task SendObjectShouldReturnExpectedResultAsync()
    {
        // Arrange
        ISender sender = new TestSender();
        object request = new TestQuery();

        // Act
        var result = await sender.Send(request);

        // Assert
        Assert.Equal("Hello SimpleMediatR", result);
    }

    private sealed record TestQuery : IRequest<string>;

    private sealed record TestCommand : IRequest;

    private sealed class TestSender : ISender
    {
        public bool WasHandled { get; private set; }

        public CancellationToken ReceivedToken { get; private set; }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            => Task.FromResult((TResponse)(object)"Hello SimpleMediatR");

        public Task Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            WasHandled = true;
            ReceivedToken = cancellationToken;

            return Task.CompletedTask;
        }

        public Task<object?> Send(
            object request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<object?>("Hello SimpleMediatR");
    }
}
