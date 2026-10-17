
namespace SimpleMediatR;

/// <summary>
/// Represents an asynchronous continuation to the next step in the request pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the pipeline.</typeparam>
/// <param name="cancellationToken">An optional cancellation token.</param>
/// <returns>A task containing the response.</returns>
#pragma warning disable CA1711
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(
    CancellationToken cancellationToken = default);
#pragma warning restore CA1711
