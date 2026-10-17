
namespace SimpleMediatR;

/// <summary>
/// Defines a handler for a request that does not return a response.
/// </summary>
/// <typeparam name="TRequest">The type of request to handle.</typeparam>
public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Handles the specified request asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(TRequest request, CancellationToken cancellationToken);
}
