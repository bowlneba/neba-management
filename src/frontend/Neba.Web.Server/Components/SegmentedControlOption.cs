namespace Neba.Web.Server.Components;

/// <summary>
/// Represents an option in a segmented control.
/// </summary>
public sealed record SegmentedControlOption
{
    /// <summary>
    /// Gets the display label for the option.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Gets the value used to identify this option.
    /// </summary>
    public required string Value { get; init; }
}
