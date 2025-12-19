using Microsoft.AspNetCore.Routing;
using Neba.Website.Endpoints.Awards;
using Neba.Website.Endpoints.Bowlers;
using Neba.Website.Endpoints.Documents;
using Neba.Website.Endpoints.Titles;
using Neba.Website.Endpoints.Tournaments;

namespace Neba.Website.Endpoints;

/// <summary>
/// Provides extension point(s) to register the website's HTTP endpoint groups on an
/// <see cref="IEndpointRouteBuilder"/>. This centralizes wiring of feature endpoint
/// mappings (Bowlers, Titles, Awards, Tournaments and Documents) so they can be
/// registered with a single method call during application startup.
/// </summary>
public static class WebsiteEndpoints
{
#pragma warning disable S2325 // Extension methods should be static
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable S1144 // Remove unused constructor of private type.

    extension(IEndpointRouteBuilder app)
    {
        /// <summary>
        /// Registers all website endpoint groups on the provided endpoint route builder.
        /// This method composes the feature-level mapping methods in a single fluent call
        /// so callers can register all website endpoints from Program.cs (or equivalent) with
        /// one statement.
        /// </summary>
        /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance to allow fluent chaining.</returns>
        /// <remarks>
        /// The method relies on the extension context's <c>app</c> parameter; call it from
        /// application startup when configuring routing. Individual feature mapping methods
        /// are invoked in a deterministic order: Bowlers, Titles, Awards, Tournaments, Documents.
        /// </remarks>
        public IEndpointRouteBuilder MapWebsiteEndpoints()
        {
            app
                .MapBowlersEndpoints()
                .MapTitlesEndpoints()
                .MapAwardsEndpoints()
                .MapTournamentEndpoints()
                .MapDocumentEndpoints();

            return app;
        }
    }
}
