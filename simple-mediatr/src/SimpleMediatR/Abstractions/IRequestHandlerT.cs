
namespace SimpleMediatR;

/// <summary>
/// Defines a handler for a request that returns a response.
/// </summary>
/// <typeparam name="TRequest">The type of request to handle.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task containing the response.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
