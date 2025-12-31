
namespace Neba.Website.Application.BowlingCenters;

/// <summary>
/// Represents a bowling center's public-facing contact and geolocation details for display and lookup.
/// </summary>
/// <remarks>
/// Latitude and longitude values use decimal degrees (WGS84) to keep map rendering consistent across clients.
/// </remarks>
public sealed record BowlingCenterDto
{
    /// <summary>
    /// Display name of the bowling center as provided by the operator.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Street address line for the center's physical location.
    /// </summary>
    public required string Street { get; init; }

    /// <summary>
    /// Optional unit, suite, or apartment designation for the center's address.
    /// </summary>
    public string? Unit { get; init; }

    /// <summary>
    /// City where the center is located.
    /// </summary>
    public required string City { get; init; }

    /// <summary>
    /// State or province abbreviation for the center location.
    /// </summary>
    public required string State { get; init; }

    /// <summary>
    /// Postal ZIP or ZIP+4 code for the center location.
    /// </summary>
    /// <example>
    /// <c>11747</c>
    /// </example>
    public required string ZipCode { get; init; }

    /// <summary>
    /// Primary contact phone number including area code.
    /// </summary>
    /// <example>
    /// <c>(631) 555-0123</c>
    /// </example>
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// Optional phone extension when required for front desk routing; leave blank if not applicable.
    /// </summary>
    /// <example>
    /// <c>204</c>
    /// </example>
    public string? Extension { get; init; }

    /// <summary>
    /// Latitude coordinate in decimal degrees for map placement.
    /// </summary>
    public required double Latitude { get; init; }

    /// <summary>
    /// Longitude coordinate in decimal degrees for map placement.
    /// </summary>
    public required double Longitude { get; init; }
}
