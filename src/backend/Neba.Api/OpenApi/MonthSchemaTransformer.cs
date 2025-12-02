using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Neba.Api.OpenApi;

internal sealed class MonthSchemaTransformer
    : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type == typeof(Month))
        {
            schema.Properties?.Clear();

            schema.Type = JsonSchemaType.Integer;
            schema.Format = "int32";
            schema.Minimum = "1";
            schema.Maximum = "12";
            schema.Example = JsonValue.Create(7);
        }

        return Task.CompletedTask;
    }
}
