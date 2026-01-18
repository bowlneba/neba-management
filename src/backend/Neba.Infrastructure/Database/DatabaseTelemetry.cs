using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry.Trace;

namespace Neba.Infrastructure.Database;

#pragma warning disable S1144 // Extensions should be static classes

internal static class DatabaseTelemetry
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDatabaseTelemetry()
        {
            services.AddOpenTelemetry()
                .WithMetrics(metrics => metrics
                    .AddNpgsqlInstrumentation())
                .WithTracing(tracing => tracing
                    .AddNpgsql()
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.EnrichWithIDbCommand = (activity, command) =>
                        {
                            // Filter out Hangfire internal queries to reduce telemetry volume/cost
                            if (command.CommandText?.Contains("hangfire.", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                activity.IsAllDataRequested = false;
                                return;
                            }

                            activity.SetTag("db.statement", command.CommandText);
                            activity.SetTag("db.command_type", command.CommandType.ToString());
                        };
                    }));

            return services;
        }
    }
}
