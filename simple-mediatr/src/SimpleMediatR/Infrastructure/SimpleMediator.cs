
using SimpleMediatR.Internal;

namespace SimpleMediatR;

/// <summary>
/// Dispatches requests to their corresponding handlers.
/// </summary>
/// <param name="serviceProvider">
/// The service provider used to resolve request handlers.
/// </param>
public sealed class SimpleMediator(IServiceProvider serviceProvider) : ISender
{
    private readonly HandlerResolver _handlerResolver = new(serviceProvider);

    /// <inheritdoc />
    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        var handler = _handlerResolver.Resolve(request);

        var invoker = DispatcherCache.GetGenericInvoker(
            requestType,
            typeof(TResponse));

        return (Task<TResponse>)invoker(handler, request, cancellationToken);
    }

    /// <inheritdoc />
    public Task Send<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        var handler = _handlerResolver.Resolve((IRequest)request);

        var invoker = DispatcherCache.GetNonGenericInvoker(requestType);

        return (Task)invoker(handler, request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<object?> Send(
        object request,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
