namespace Neba.Api.ErrorHandling;

#pragma warning disable S1144 // Unused private types or members should be removed
#pragma warning disable S2325 // Classes should be static

internal static class ErrorHandlingExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddErrorHandling()
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails(options
                        => options.CustomizeProblemDetails = context
                            => context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.TraceIdentifier));

            return services;
        }
    }
}
