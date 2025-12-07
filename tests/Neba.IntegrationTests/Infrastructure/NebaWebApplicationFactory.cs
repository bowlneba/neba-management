using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Neba.Api;
using Neba.Tests;

namespace Neba.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration testing that configures the test database.
/// Each test class should create its own instance of WebsiteDatabase and pass it to this factory.
/// </summary>
public sealed class NebaWebApplicationFactory
    : WebApplicationFactory<IApiAssemblyMarker>
{
    private readonly WebsiteDatabase _database;

    public NebaWebApplicationFactory(WebsiteDatabase database)
    {
        _database = database;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:bowlneba", _database.ConnectionString);
    }
}
