
using System.Reflection;
using System.Runtime.ExceptionServices;
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

        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, typeof(TResponse));

        var handler = _handlerResolver.Resolve(request);

        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException(
                $"The handler '{handlerType.FullName}' does not contain a Handle method.");

        try
        {
            return (Task<TResponse>)handleMethod.Invoke(
                handler,
                [request, cancellationToken])!;
        }
        catch (TargetInvocationException exception)
            when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    /// <inheritdoc />
    public Task Send<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        var handlerType = typeof(IRequestHandler<>)
            .MakeGenericType(requestType);

        var handler = _handlerResolver.Resolve((IRequest)request);

        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException(
                $"The handler '{handlerType.FullName}' does not contain a Handle method.");

        try
        {
            return (Task)handleMethod.Invoke(
                handler,
                [request, cancellationToken])!;
        }
        catch (TargetInvocationException exception)
            when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    /// <inheritdoc />
    public Task<object?> Send(
        object request,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
