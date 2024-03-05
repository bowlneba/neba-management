using System.ComponentModel.DataAnnotations;

namespace Neba.UI.Services;

internal sealed class NebaApiOptions
{
    public static string SectionName { get; } = "NebaApi";

    [Required] public Uri BaseUrl { get; init; } = null!;
}
