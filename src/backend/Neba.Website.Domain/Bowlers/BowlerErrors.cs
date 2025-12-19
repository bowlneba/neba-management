using ErrorOr;
using Neba.Domain.Identifiers;

namespace Neba.Website.Domain.Bowlers;

/// <summary>
/// Provides domain errors related to bowlers.
/// </summary>
public static class BowlerErrors
{
    /// <summary>
    /// Returns a not found error for a bowler with the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier of the bowler.</param>
    /// <returns>An <see cref="Error"/> indicating the bowler was not found, including the bowler ID in metadata.</returns>
    public static Error BowlerNotFound(BowlerId id)
        => Error.NotFound(
            code: "Bowler.NotFound",
            description: "Bowler was not found.",
            metadata: new Dictionary<string, object>
            {
                { "bowlerId", id }
            }
        );
}
