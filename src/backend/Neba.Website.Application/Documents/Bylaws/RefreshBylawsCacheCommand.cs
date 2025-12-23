using Neba.Application.Messaging;

namespace Neba.Website.Application.Documents.Bylaws;

/// <summary>
/// Command to request a refresh of the bylaws cache.
/// </summary>
/// <remarks>
/// Handled by an application service to rebuild or update any cached
/// representation of bylaws. The command produces a <see cref="string"/>
/// response which typically contains a result identifier, cache key or
/// a short status message indicating the outcome.
/// </remarks>
/// <seealso cref="ICommand{TResponse}"/>
public sealed record RefreshBylawsCacheCommand
    : ICommand<string>;
