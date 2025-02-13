using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OpenApi;
using System.Reflection;
using System.Xml.Linq;

namespace ML.Api.Filters;

public class OpenApiFilter : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Media Locator API",
            Version = "v1",
            Description = "This document describes the Media Locator API endpoints.",
            Contact = new OpenApiContact
            {
                Name = "Isaac Oselukwue",
                Email = "29353479@students.lincoln.ac.uk"
            }
        };

        string xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
        
        foreach (var path in document.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                operation.Value.Parameters ??= [];

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