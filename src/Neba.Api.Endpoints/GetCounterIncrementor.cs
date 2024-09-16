using System.Security.Cryptography;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Neba.Api.Endpoints;

#pragma warning disable CA1812
internal sealed class GetCounterIncrementor
    : EndpointWithoutRequest<Results<Ok<int>, ProblemDetails>>
{
    public override void Configure()
    {
        Get("counter");
        Version(1);
        AllowAnonymous();
    }

    public override async Task<Results<Ok<int>, ProblemDetails>> ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(5, ct);

        var incrementor = RandomNumberGenerator.GetInt32(2, 10);

        return TypedResults.Ok(incrementor);
    }
}