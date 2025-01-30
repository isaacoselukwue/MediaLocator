using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;

namespace ML.Api.Filters;

public class OpenApiFilter : IOpenApiDocumentTransformer
{
    //public void Apply(OpenApiDocument document)
    //{
    //    foreach (var path in document.Paths)
    //    {
    //        foreach (var operation in path.Value.Operations)
    //        {
    //            operation.Value.Parameters ??= new List<OpenApiParameter>();

    //            operation.Value.Parameters.Add(new OpenApiParameter
    //            {
    //                Name = "X-Api-Key",
    //                In = ParameterLocation.Header,
    //                Required = true,
    //                Description = "API Key required for authentication",
    //                Example = new Microsoft.OpenApi.Any.OpenApiString("your-api-key-here")
    //            });
    //        }
    //    }
    //}

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var path in document.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                operation.Value.Parameters ??= new List<OpenApiParameter>();

                operation.Value.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Api-Key",
                    In = ParameterLocation.Header,
                    Required = false,
                    Description = "API Key required for authentication",
                    Example = new Microsoft.OpenApi.Any.OpenApiString("your-api-key-here")
                });
                operation.Value.Parameters.Add(new OpenApiParameter
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Required = false,
                    Description = "Bearer token required for authorization",
                    Example = new Microsoft.OpenApi.Any.OpenApiString("Bearer your-bearer-token-here")
                });
            }
        }
        return Task.CompletedTask;
    }
}