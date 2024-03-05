using Microsoft.Extensions.Options;

namespace Neba.UI.Services;

internal static class ServicesConfiguration
{
    public static void AddServices(this IServiceCollection services)
        => services.AddNebaService();

    private static IServiceCollection AddNebaService(this IServiceCollection services)
    {
        services.AddOptions<NebaApiOptions>()
        .BindConfiguration(NebaApiOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

        services.AddHttpClient(NebaApiService._serviceName, (s, client) =>
        {
            var options = s.GetRequiredService<IOptions<NebaApiOptions>>().Value;
            client.BaseAddress = options.BaseUrl;
        });

        services.AddScoped<IWeatherService, WeatherService>();
        return services;
    }
}
