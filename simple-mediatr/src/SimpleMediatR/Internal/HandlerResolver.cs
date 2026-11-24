
using System.Collections.Concurrent;

namespace SimpleMediatR.Internal;

/// <summary>
/// Resolves request handlers using the provided service provider.
/// </summary>
internal sealed class HandlerResolver(IServiceProvider serviceProvider)
{
    private static readonly ConcurrentDictionary<
        (Type RequestType, Type ResponseType),
        Type> s_genericHandlerTypes = new();

    private static readonly ConcurrentDictionary<
        Type,
        Type> s_nonGenericHandlerTypes = new();

    private readonly IServiceProvider _serviceProvider = serviceProvider
        ?? throw new ArgumentNullException(nameof(serviceProvider));

    public object Resolve<TResponse>(IRequest<TResponse> request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        var handlerType = s_genericHandlerTypes.GetOrAdd(
            (requestType, typeof(TResponse)),
            static key => typeof(IRequestHandler<,>)
                .MakeGenericType(key.RequestType, key.ResponseType));

        return ResolveHandler(handlerType, requestType);
    }

    public object Resolve(IRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        var handlerType = s_nonGenericHandlerTypes.GetOrAdd(
            requestType,
            static type => typeof(IRequestHandler<>)
                .MakeGenericType(type));

        return ResolveHandler(handlerType, requestType);
    }

    private object ResolveHandler(Type handlerType, Type requestType)
    {
        return _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException(
                $"No handler registered for request type '{requestType.FullName}'. " +
                $"Register an implementation of '{handlerType.FullName}' in dependency injection.");
    }
}
