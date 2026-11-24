using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace SimpleMediatR.Internal;

/// <summary>
/// Caches compiled delegates for invoking request handlers.
/// </summary>
internal static class DispatcherCache
{
    private static readonly ConcurrentDictionary<
        (Type RequestType, Type ResponseType),
        Func<object, object, CancellationToken, object>> s_genericInvokers = new();

    private static readonly ConcurrentDictionary<
        Type,
        Func<object, object, CancellationToken, object>> s_nonGenericInvokers = new();

    public static Func<object, object, CancellationToken, object> GetGenericInvoker(
        Type requestType,
        Type responseType)
        => s_genericInvokers.GetOrAdd(
            (requestType, responseType),
            static key => CreateGenericInvoker(key.RequestType, key.ResponseType));

    public static Func<object, object, CancellationToken, object> GetNonGenericInvoker(
        Type requestType)
        => s_nonGenericInvokers.GetOrAdd(
            requestType,
            static type => CreateNonGenericInvoker(type));

    private static Func<object, object, CancellationToken, object> CreateGenericInvoker(
        Type requestType,
        Type responseType)
    {
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, responseType);

        return CreateInvoker(handlerType, requestType);
    }

    private static Func<object, object, CancellationToken, object> CreateNonGenericInvoker(
        Type requestType)
    {
        var handlerType = typeof(IRequestHandler<>)
            .MakeGenericType(requestType);

        return CreateInvoker(handlerType, requestType);
    }

    private static Func<object, object, CancellationToken, object> CreateInvoker(
        Type handlerType,
        Type requestType)
    {
        var handlerParameter = Expression.Parameter(typeof(object), "handler");
        var requestParameter = Expression.Parameter(typeof(object), "request");
        var cancellationTokenParameter = Expression.Parameter(
            typeof(CancellationToken),
            "cancellationToken");

        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException(
                $"The handler '{handlerType.FullName}' does not contain a Handle method.");

        var call = Expression.Call(
            Expression.Convert(handlerParameter, handlerType),
            handleMethod,
            Expression.Convert(requestParameter, requestType),
            cancellationTokenParameter);

        var body = Expression.Convert(call, typeof(object));

        return Expression.Lambda<Func<object, object, CancellationToken, object>>(
            body,
            handlerParameter,
            requestParameter,
            cancellationTokenParameter).Compile();
    }
}
