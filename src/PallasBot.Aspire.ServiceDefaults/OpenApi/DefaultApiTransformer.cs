using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace PallasBot.Aspire.ServiceDefaults.OpenApi;

public class DefaultApiTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var resp = new OpenApiResponses
        {
            ["200"] = new OpenApiResponse
            {
                Description = "System healthy",
                Content =
                {
                    ["text/plain"] = new OpenApiMediaType
                    {
                        Example = new OpenApiString("Health")
                    }
                }
            },
            ["503"] = new OpenApiResponse
            {
                Description = "System unhealthy",
                Content =
                {
                    ["text/plain"] = new OpenApiMediaType
                    {
                        Example = new OpenApiString("Unhealthy")
                    }
                }
            }
        };

        var tag = new OpenApiTag
        {
            Name = "Status"
        };

        document.Paths.Add("/health", new OpenApiPathItem
        {
            Operations =
            {
                [OperationType.Get] = new OpenApiOperation
                {
                    Tags = [tag],
                    Description = "Get to know if the system can serve requests.",
                    Responses = resp
                }
            }
        });

        document.Paths.Add("/alive", new OpenApiPathItem
        {
            Operations =
            {
                [OperationType.Get] = new OpenApiOperation
                {
                    Tags = [tag],
                    Description = "Get to know if the system is running.",
                    Responses = resp
                }
            }
        });

        return Task.CompletedTask;
    }
}
