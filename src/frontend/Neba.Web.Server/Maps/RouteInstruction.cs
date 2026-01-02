using UnitsNet;

namespace Neba.Web.Server.Maps;

/// <summary>
/// Represents a single turn-by-turn instruction.
/// </summary>
public sealed class RouteInstruction
{
    /// <summary>
    /// The instruction text (e.g., "Turn left onto Main St").
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Distance to this instruction in meters.
    /// </summary>
    public double DistanceMeters { get; set; }

    /// <summary>
    /// Gets the distance formatted for display.
    /// </summary>
    public string FormattedDistance
    {
        get
        {
            Length distance = Length.FromMeters(DistanceMeters);
            double miles = distance.Miles;
            if (miles < 0.1)
            {
                double feet = distance.Feet;
                return $"{feet:F0} ft";
            }
            return $"{miles:F1} mi";
        }
    }
}
