using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SOS.Lib.Swagger;

/// <summary>
/// Represents the Swagger/Swashbuckle operation filter used to document the implicit API version parameter.
/// </summary>
/// <remarks>This <see cref="IOperationFilter"/> is only required due to bugs in the <see cref="SwaggerGenerator"/>.
/// Once they are fixed and published, this class can be removed.</remarks>
public class SwaggerDefaultValues : IOperationFilter
{
    /// <summary>
    /// Applies the filter to the specified operation using the given context.
    /// </summary>
    /// <param name="operation">The operation to apply the filter to.</param>
    /// <param name="context">The current operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
        {
            return;
        }

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
        foreach (var parameter in operation.Parameters)
        {
            if (parameter is OpenApiParameter openApiParameter)
            {
                var description = context.ApiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                if (openApiParameter.Description == null)
                {
                    openApiParameter.Description = description.ModelMetadata?.Description;
                }

                if (openApiParameter.Schema.Default == null && description.DefaultValue != null)
                {
                    Console.WriteLine($"Setting default value for parameter '{openApiParameter.Name}' to '{description.DefaultValue}'");
                    if (openApiParameter.Schema is OpenApiSchema openApiSchema)
                    {
                        var json = JsonSerializer.Serialize(description.DefaultValue);
                        openApiSchema.Default = JsonNode.Parse(json);
                    }                                              
                }
                
                openApiParameter.Required |= description.IsRequired;
            }               
        }
    }
}