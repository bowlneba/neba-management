using Neba.Application.Abstractions.Messaging;
using Neba.Application.Tournaments.GetTournamentRules;

namespace Neba.Api.Endpoints.Website.Tournaments;

#pragma warning disable S1144 // Remove unused constructor of private type.
#pragma warning disable S2325 // Extension methods should be static

internal static class TournamentEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapTournamentEndpoints()
        {
            var tournamentGroup = app
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

                return TypedResults.Ok(htmlContent);
            })
            .WithName("GetTournamentRules")
            .WithSummary("Get the tournament rules document.")
            .WithDescription("Retrieves the tournament rules document as an HTML string.")
            .Produces<string>(StatusCodes.Status200OK, ContentTypes.TextHtml)
            .ProducesProblem(StatusCodes.Status500InternalServerError, ContentTypes.ApplicationProblemJson);

            return app;
        }
    }
}
