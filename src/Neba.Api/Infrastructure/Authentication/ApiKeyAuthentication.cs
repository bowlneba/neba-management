using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;

namespace Neba.Api.Infrastructure.Authentication;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class ApiKeyAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> Options, ILoggerFactory Logger, UrlEncoder Encoder, IConfiguration Config)
    : AuthenticationHandler<AuthenticationSchemeOptions>(Options, Logger, Encoder)
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "x-api-key";

    private readonly string _apiKey = Config["ApiKey"]
        ?? throw new InvalidOperationException("Api key not set in appsettings.json");

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Request.Headers.TryGetValue(HeaderName, out var apiKey);

        if (!(IsPublicEndPoint() || apiKey == _apiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Api Credentials"));
        }

        var identity = new ClaimsIdentity(
            claims: new[] { new Claim("ClientId", "Default") },
            authenticationType: Scheme.Name);
        var principal = new GenericPrincipal(identity, roles: null);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool IsPublicEndPoint()
        => Context.GetEndpoint()?.Metadata.OfType<AllowAnonymousAttribute>().Any() is null or true;
}
