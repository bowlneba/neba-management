using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using Neba.Domain.Contact;

namespace Neba.Website.Contracts.BowlingCenters;

/// <summary>
/// Response DTO representing a bowling center returned by the website API.
/// </summary>
/// <remarks>
/// All coordinates are expressed in decimal degrees (WGS84). Phone numbers are stored unformatted
/// (country code + digits) to simplify international dialing and URI generation.
/// </remarks>
public sealed record BowlingCenterResponse
{
    /// <summary>
    /// The display name of the bowling center.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Primary street address for the center.
    /// </summary>
    public required string Street { get; init; }

    /// <summary>
    /// Optional apartment, suite, or unit identifier associated with the street address.
    /// </summary>
    public string? Unit { get; init; }

    /// <summary>
    /// City where the center is located.
    /// </summary>
    public required string City { get; init; }

    /// <summary>
    /// State for the center location.
    /// </summary>
    [JsonConverter(typeof(SmartEnumValueConverter<UsState, string>))]
    public required UsState State { get; init; }

    /// <summary>
    /// Postal ZIP or ZIP+4 code for the center location.
    /// </summary>
    public required string ZipCode { get; init; }

    /// <summary>
    /// Primary contact phone number stored as digits including country code (e.g. "12035551234").
    /// </summary>
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// Optional phone extension for internal routing (digits only).
    /// </summary>
    public string? PhoneExtension { get; init; }

    /// <summary>
    /// Latitude coordinate in decimal degrees for map display.
    /// </summary>
    public required double Latitude { get; init; }

    /// <summary>
    /// Longitude coordinate in decimal degrees for map display.
    /// </summary>
    public required double Longitude { get; init; }

    /// <summary>
    /// Indicates whether the center is currently closed (temporary or permanent).
    /// </summary>
    public required bool IsClosed { get; init; }
}
