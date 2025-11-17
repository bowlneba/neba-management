using System.Diagnostics.CodeAnalysis;

namespace Neba.Web.Server;

[SuppressMessage("Design", "CA1812: Avoid uninstantiated internal classes", Justification = "Instantiated by configuration binding.")]
internal sealed record NebaApiConfiguration
{
    public required string BaseUrl { get; init; }
}
