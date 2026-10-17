namespace SimpleMediatR;

/// <summary>
/// Represents a request that returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the request handler.</typeparam>
public interface IRequest<out TResponse> : IBaseRequest
{
}
