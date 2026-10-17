

using SimpleMediatR.Abstractions;

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

    private sealed record TestRequest : IRequest;


    [Fact]
    public void GenericRequestSShouldImplementIBaseRequest()
    {
        // Arrange
        var request = new TestGenericRequest();

        // Act
        bool isBaseRequest = request is IBaseRequest;

        // Assert
        Assert.True(isBaseRequest);
    }

    private sealed record TestGenericRequest : IRequest<string>;

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
}
