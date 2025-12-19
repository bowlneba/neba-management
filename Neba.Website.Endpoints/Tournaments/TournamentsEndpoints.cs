using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Neba.Application.Messaging;
using Neba.Application.Tournaments.GetTournamentRules;

namespace Neba.Website.Endpoints.Tournaments;

#pragma warning disable S1144 // Remove unused constructor of private type.
#pragma warning disable S2325 // Extension methods should be static

internal static class TournamentEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapTournamentEndpoints()
        {
            RouteGroupBuilder tournamentGroup = app
                .MapGroup("/tournaments")
                .WithTags("tournaments", "website")
                .AllowAnonymous();

            tournamentGroup.MapGetTournamentRulesEndpoint();

            return app;
        }

        private IEndpointRouteBuilder MapGetTournamentRulesEndpoint()
        {
            app.MapGet("/rules", async (IQueryHandler<GetTournamentRulesQuery, string> queryHandler) =>
            {
                GetTournamentRulesQuery query = new();

                string htmlContent = await queryHandler.HandleAsync(query, CancellationToken.None);

                return TypedResults.Content(htmlContent, MediaTypeNames.Text.Html);
            })
            .WithName("GetTournamentRules")
            .WithSummary("Get the tournament rules document.")
            .WithDescription("Retrieves the tournament rules document as an HTML string.")
            .Produces<string>(StatusCodes.Status200OK, MediaTypeNames.Text.Html)
            .ProducesProblem(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)
            .WithTags("tournaments", "website", "documents");

            return app;
        }
    }
}
