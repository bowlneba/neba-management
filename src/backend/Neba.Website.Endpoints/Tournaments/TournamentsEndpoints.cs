using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Neba.Application.Documents;
using Neba.Application.Messaging;
using Neba.Website.Application.Tournaments.GetTournamentRules;

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
            app.MapGet("/rules", async (IQueryHandler<GetTournamentRulesQuery, DocumentDto> queryHandler) =>
            {
                GetTournamentRulesQuery query = new();

                DocumentDto documentDto = await queryHandler.HandleAsync(query, CancellationToken.None);

                return TypedResults.Ok(documentDto);
            })
            .WithName("GetTournamentRules")
            .WithSummary("Get the tournament rules document.")
            .WithDescription("Retrieves the tournament rules document")
            .Produces<DocumentDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)
            .WithTags("tournaments", "website", "documents");

            return app;
        }
    }
}
