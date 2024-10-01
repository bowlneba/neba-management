using System.Diagnostics.CodeAnalysis;

namespace Neba.Web.Services.NebaApi;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via configuration.")]
internal sealed class NebaApiOptions
{
    public static string SectionName
        => "Neba.Api";

    public string BaseUrl { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;
}
