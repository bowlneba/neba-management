namespace Neba.Web.Server.Components;

/// <summary>
/// Defines the type of skeleton loader to display.
/// </summary>
public enum SkeletonType
{
    /// <summary>
    /// Card layout with title and text lines.
    /// </summary>
    Card,

    /// <summary>
    /// Table rows layout.
    /// </summary>
    Table,

    /// <summary>
    /// Simple text lines.
    /// </summary>
    Text,

    /// <summary>
    /// Avatar with optional text lines.
    /// </summary>
    Avatar,

    /// <summary>
    /// Custom dimensions.
    /// </summary>
    Custom
}
