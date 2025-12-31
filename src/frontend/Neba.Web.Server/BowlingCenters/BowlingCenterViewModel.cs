namespace Neba.Web.Server.BowlingCenters;

/// <summary>
/// Represents a bowling center with location and contact information.
/// </summary>
/// <param name="Name">The name of the bowling center.</param>
/// <param name="Street">The street address.</param>
/// <param name="City">The city.</param>
/// <param name="State">The state abbreviation (e.g., MA, CT, RI, NH, ME, VT).</param>
/// <param name="ZipCode">The ZIP code.</param>
/// <param name="Phone">The phone number.</param>
/// <param name="Latitude">The latitude coordinate for map display.</param>
/// <param name="Longitude">The longitude coordinate for map display.</param>
public record BowlingCenterViewModel(
    string Name,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Phone,
    double Latitude,
    double Longitude);
