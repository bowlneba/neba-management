namespace Neba.UI.Services;

internal sealed class NebaApiOptions
{
    public static string SectionName { get; } = "NebaApi";

    public Uri? BaseUrl { get; set; }
}
