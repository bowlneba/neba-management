using Microsoft.Extensions.Options;

namespace Neba.Web.Services.NebaApi;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class NebaApiAuthenticationDelegatingHandler(IOptions<NebaApiOptions> Options)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("x-api-key", Options.Value.Key);

        return base.SendAsync(request, cancellationToken);
    }
}
