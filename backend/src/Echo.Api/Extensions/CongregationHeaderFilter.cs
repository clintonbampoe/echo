using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Echo.Api.Extensions;

public class CongregationHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = "X-Congregation-Id",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" },
            }
        );
    }
}
