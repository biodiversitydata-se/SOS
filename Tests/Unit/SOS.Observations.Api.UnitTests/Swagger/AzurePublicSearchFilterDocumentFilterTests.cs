using SOS.Api.Contracts.Aggregations.Requests;
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
    public void GeneratedDocuments_ExposeArtportalenFilterEverywhereExceptAzurePublic()
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

        publicDocument.Components!.Schemas.Keys.Should().NotContain(name => name.Contains("ServerModel"));
        azureInternalDocument.Components!.Schemas.Keys.Should().NotContain(name => name.Contains("ServerModel"));

        publicDocument.Components!.Schemas["SearchFilterDto"].Properties
            .Should().ContainKey("artportalen");
        publicDocument.Components.Schemas["ArtportalenFilterDto"].Properties
            .Should().ContainKey("typeFilter");
        publicDocument.Components.Schemas["SightingTypeFilterDto"].Enum
            .Should().HaveCount(6);
        foreach (var schemaName in new[] { "SearchFilterBaseDto", "SearchFilterDto", "SearchFilterAggregationDto" })
        {
            if (azurePublicDocument.Components!.Schemas.TryGetValue(schemaName, out var schema))
            {
                schema.Properties.Should().NotContainKey("artportalen");
            }
        }
        azurePublicDocument.Components!.Schemas.Should().NotContainKey("ArtportalenFilterDto");
        azurePublicDocument.Components.Schemas["SearchFilterDto"].Properties
            .Should().NotContainKey("extendedFilter");
        azureInternalDocument.Components!.Schemas["ExtendedFilterDto"].Properties
            .Should().ContainKey("typeFilter");
        publicDocument.Components.Schemas["Geometry"].OneOf.Should().HaveCount(7);
        publicDocument.Components.Schemas.Should().NotContainKey("IGeometryObject");
        publicDocument.Components.Schemas.Should().NotContainKey("GeoJSONObjectType");
        publicDocument.Components.Schemas["ExportJobInfoDto"].Properties["processingTime"]
            .ReadOnly.Should().BeTrue();
    }

    [Theory]
    [InlineData("SearchFilterBaseDto")]
    [InlineData("SearchFilterDto")]
    [InlineData("SearchFilterAggregationDto")]
    public void RemoveArtportalenFilter_RemovesPropertyFromAzurePublicSearchSchemas(string schemaName)
    {
        var schemas = CreateSchemas(schemaName);

        AzurePublicSearchFilterDocumentFilter.RemoveArtportalenFilter(
            "AzurePublicSosObservationsv1.5",
            schemas);

        schemas[schemaName].Properties.Should().NotContainKey("artportalen");
        schemas.Should().NotContainKey("ArtportalenFilterDto");
    }

    [Fact]
    public void RemoveArtportalenFilter_DoesNotChangeRegularPublicDocument()
    {
        var schemas = CreateSchemas("SearchFilterDto");

        AzurePublicSearchFilterDocumentFilter.RemoveArtportalenFilter(
            "PublicSosObservationsv1.5",
            schemas);

        schemas["SearchFilterDto"].Properties.Should().ContainKey("artportalen");
        schemas.Should().ContainKey("ArtportalenFilterDto");
    }

    [Fact]
    public void RemoveArtportalenFilter_DoesNotRemoveNestedInternalTypeFilter()
    {
        var schemas = CreateSchemas("ExtendedFilterDto");
        schemas["ExtendedFilterDto"].Properties["typeFilter"] =
            new OpenApiSchema { Type = JsonSchemaType.String };

        AzurePublicSearchFilterDocumentFilter.RemoveArtportalenFilter(
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
                    ["artportalen"] = new OpenApiSchema { Type = JsonSchemaType.Object },
                    ["taxon"] = new OpenApiSchema { Type = JsonSchemaType.Object }
                }
            },
            ["ArtportalenFilterDto"] = new OpenApiSchema
            {
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    ["typeFilter"] = new OpenApiSchema { Type = JsonSchemaType.String }
                }
            }
        };
    }
}
