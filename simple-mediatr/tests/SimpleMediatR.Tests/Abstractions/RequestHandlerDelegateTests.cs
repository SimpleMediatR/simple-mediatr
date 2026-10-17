
using SimpleMediatR;

namespace SimpleMediatR.Tests.Abstractions;

public sealed class RequestHandlerDelegateTests
{

    [Fact]
    public async Task DelegateShouldReturnExpectedResponseAsync()
    {
        // Arrange
        RequestHandlerDelegate<string> next = HandleRequest;

        // Act
        var result = await next();

        // Assert
        Assert.Equal("Hello SimpleMediatR", result);

        static Task<string> HandleRequest(CancellationToken cancellationToken = default)
            => Task.FromResult("Hello SimpleMediatR");
    }


    [Fact]
    public async Task DelegateShouldReceiveCancellationTokenAsync()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken expectedToken = cancellationTokenSource.Token;
        CancellationToken receivedToken = default;

        RequestHandlerDelegate<string> next = HandleRequest;

        // Act
        await next(expectedToken);

        // Assert
        Assert.Equal(expectedToken, receivedToken);

        Task<string> HandleRequest(CancellationToken cancellationToken = default)
        {
            receivedToken = cancellationToken;
            return Task.FromResult("Hello SimpleMediatR");
        }
    }


    [Fact]
    public async Task DelegateShouldWorkInGenericPipelineAsync()
    {
        // Arrange
        RequestHandlerDelegate<string> next = HandleRequest;

        // Act
        var result = await ExecutePipelineAsync(next);

        // Assert
        Assert.Equal("Hello SimpleMediatR", result);

        static Task<string> HandleRequest(CancellationToken cancellationToken = default)
            => Task.FromResult("Hello SimpleMediatR");
    }

    private static async Task<TResponse> ExecutePipelineAsync<TResponse>(
        RequestHandlerDelegate<TResponse> next) => await next();

}
