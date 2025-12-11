namespace Neba.Contracts.Website.Awards;

/// <summary>
/// Response DTO representing a high block award for a bowler returned by website endpoints.
/// </summary>
/// <remarks>
/// This contract is a simple data-transfer object used by website APIs. Keep business logic out of contracts.
/// </remarks>
/// <example>
/// {
///   "id": "d2f1e8a5-3b9a-4c6b-8f2a-1a2b3c4d5e6f",
///   "bowlerId": "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
///   "bowlerName": "Jane Doe",
///   "season": "2024/2025"
/// }
/// </example>
public sealed record HighBlockAwardResponse
{
    /// <summary>
    /// Unique identifier of the award.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Identifier of the bowler who received the award.
    /// </summary>
    public required Guid BowlerId { get; init; }

    /// <summary>
    /// Display name of the bowler. This should be non-sensitive and suitable for public display.
    /// </summary>
    public required string BowlerName { get; set; }

    /// <summary>
    /// Season for which the award was given (for example "2024/2025").
    /// </summary>
    public required string Season { get; set; }
}
