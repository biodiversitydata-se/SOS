using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Moq;
using SOS.Observations.Api.Controllers;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Swagger;
using Swashbuckle.AspNetCore.Swagger;
using System.Text.Json.Serialization;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Swagger;

public class AzurePublicSearchFilterDocumentFilterTests
{
    [Fact]
    public void GeneratedDocuments_ExposeTypeFilterEverywhereExceptAzurePublic()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.ApplicationName)
            .Returns(typeof(ObservationsController).Assembly.GetName().Name!);
        environment.SetupGet(value => value.EnvironmentName).Returns("Development");
        environment.SetupGet(value => value.ContentRootPath).Returns(AppContext.BaseDirectory);
        environment.SetupGet(value => value.ContentRootFileProvider).Returns(new NullFileProvider());
        environment.SetupGet(value => value.WebRootPath).Returns(AppContext.BaseDirectory);
        environment.SetupGet(value => value.WebRootFileProvider).Returns(new NullFileProvider());
        services.AddSingleton(environment.Object);
        services.AddSingleton<IHostEnvironment>(environment.Object);
        services.AddControllers()
            .AddApplicationPart(typeof(ObservationsController).Assembly)
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.SetupSwagger();

        using var serviceProvider = services.BuildServiceProvider();
        var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();

        var publicDocument = swaggerProvider.GetSwagger("PublicSosObservationsv1.5");
        var azurePublicDocument = swaggerProvider.GetSwagger("AzurePublicSosObservationsv1.5");
        var azureInternalDocument = swaggerProvider.GetSwagger("AzureInternalSosObservationsv1.5");

        publicDocument.Components!.Schemas["SearchFilterDto"].Properties
            .Should().ContainKey("typeFilter");
        publicDocument.Components.Schemas["SightingTypeFilterDto"].Enum
            .Should().HaveCount(6);
        foreach (var schemaName in new[] { "SearchFilterBaseDto", "SearchFilterDto", "SearchFilterAggregationDto" })
        {
            azurePublicDocument.Components!.Schemas[schemaName].Properties
                .Should().NotContainKey("typeFilter");
        }
        azurePublicDocument.Components.Schemas["SearchFilterDto"].Properties
            .Should().NotContainKey("extendedFilter");
        azureInternalDocument.Components!.Schemas["ExtendedFilterDto"].Properties
            .Should().ContainKey("typeFilter");
    }

    [Theory]
    [InlineData("SearchFilterBaseDto")]
    [InlineData("SearchFilterDto")]
    [InlineData("SearchFilterAggregationDto")]
    public void RemoveTypeFilter_RemovesPropertyFromAzurePublicSearchSchemas(string schemaName)
    {
        var schemas = CreateSchemas(schemaName);

        AzurePublicSearchFilterDocumentFilter.RemoveTypeFilter(
            "AzurePublicSosObservationsv1.5",
            schemas);

        schemas[schemaName].Properties.Should().NotContainKey("typeFilter");
    }

    [Fact]
    public void RemoveTypeFilter_DoesNotChangeRegularPublicDocument()
    {
        var schemas = CreateSchemas("SearchFilterDto");

        AzurePublicSearchFilterDocumentFilter.RemoveTypeFilter(
            "PublicSosObservationsv1.5",
            schemas);

        schemas["SearchFilterDto"].Properties.Should().ContainKey("typeFilter");
    }

    [Fact]
    public void RemoveTypeFilter_DoesNotRemoveNestedInternalTypeFilter()
    {
        var schemas = CreateSchemas("ExtendedFilterDto");

        AzurePublicSearchFilterDocumentFilter.RemoveTypeFilter(
            "AzurePublicSosObservationsv1.5",
            schemas);

        schemas["ExtendedFilterDto"].Properties.Should().ContainKey("typeFilter");
    }

    private static Dictionary<string, IOpenApiSchema> CreateSchemas(string schemaName)
    {
        return new Dictionary<string, IOpenApiSchema>
        {
            [schemaName] = new OpenApiSchema
            {
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    ["typeFilter"] = new OpenApiSchema { Type = JsonSchemaType.String },
                    ["taxon"] = new OpenApiSchema { Type = JsonSchemaType.Object }
                }
            }
        };
    }
}
