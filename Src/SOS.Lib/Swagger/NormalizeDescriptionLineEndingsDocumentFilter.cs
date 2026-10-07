using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SOS.Lib.Swagger;

/// <summary>Uses the same line endings in descriptions on every platform.</summary>
public sealed class NormalizeDescriptionLineEndingsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context) =>
        new OpenApiWalker(new DescriptionVisitor()).Walk(document);

    private sealed class DescriptionVisitor : OpenApiVisitorBase
    {
        public override void Visit(OpenApiOperation operation) =>
            operation.Description = Normalize(operation.Description);

        public override void Visit(IOpenApiParameter parameter)
        {
            if (parameter is OpenApiParameter value)
                value.Description = Normalize(value.Description);
        }

        public override void Visit(IOpenApiSchema schema)
        {
            if (schema is OpenApiSchema value)
                value.Description = Normalize(value.Description);
        }

        private static string? Normalize(string? description) => description?.Replace("\r\n", "\n");
    }
}
