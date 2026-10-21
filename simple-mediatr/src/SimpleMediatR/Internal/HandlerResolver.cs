
namespace SimpleMediatR.Internal;

/// <summary>
/// Resolves request handlers using the provided service provider.
/// </summary>
internal sealed class HandlerResolver(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider
        ?? throw new ArgumentNullException(nameof(serviceProvider));

    public object Resolve<TResponse>(IRequest<TResponse> request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, typeof(TResponse));

        return ResolveHandler(handlerType, requestType);
    }

    public object Resolve(IRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        var handlerType = typeof(IRequestHandler<>)
            .MakeGenericType(requestType);

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
