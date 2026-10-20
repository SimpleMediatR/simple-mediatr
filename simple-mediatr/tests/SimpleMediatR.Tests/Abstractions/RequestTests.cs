
namespace SimpleMediatR.Tests.Abstractions;

public sealed class RequestTests
{
    [Fact]
    public void RequestShouldImplementIBaseRequest()
    {
        // Arrange
        var request = new TestRequest();

        // Act
        var isBaseRequest = request is IBaseRequest;

        // Assert
        Assert.True(isBaseRequest);
    }

    [Fact]
    public void GenericRequestShouldImplementIBaseRequest()
    {
        // Arrange
        var request = new TestGenericRequest();

        // Act
        var isBaseRequest = request is IBaseRequest;

        // Assert
        Assert.True(isBaseRequest);
    }

    [Fact]
    public void GenericRequestShouldSupportCovariance()
    {
        // Arrange
        IRequest<string> request = new TestGenericRequest();

        // Act
        IRequest<object> generalRequest = request;

        // Assert
        Assert.Same(request, generalRequest);
    }

    private sealed record TestRequest : IRequest;

    private sealed record TestGenericRequest : IRequest<string>;
}
