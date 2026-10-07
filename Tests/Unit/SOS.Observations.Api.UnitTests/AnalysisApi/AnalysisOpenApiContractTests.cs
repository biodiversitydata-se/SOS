using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Moq;
using SOS.Analysis.Api.Controllers;
using SOS.Analysis.Api.Extensions;
using Swashbuckle.AspNetCore.Swagger;
using System.Text.Json.Serialization;
using Xunit;

namespace SOS.Observations.Api.UnitTests.AnalysisApi;

public class AnalysisOpenApiContractTests
{
    [Theory]
    [InlineData("PublicSosAnalysisv1.5")]
    [InlineData("InternalSosAnalysisv1.5")]
    public void ApiInformation_UsesTheSharedTypeName(string documentName)
    {
        using var provider = CreateSwaggerProvider();
        var document = provider.GetRequiredService<ISwaggerProvider>().GetSwagger(documentName);

        Assert.Contains("ApiInformation", document.Components.Schemas.Keys);
        Assert.DoesNotContain("ApiInformationDto", document.Components.Schemas.Keys);
        var response = document.Paths["/api/ApiInfo"].Operations[HttpMethod.Get].Responses["200"]
            .Content["application/json"].Schema;
        Assert.Equal("ApiInformation", Assert.IsType<OpenApiSchemaReference>(response).Reference.Id);
    }

    [Fact]
    public void AggregationSortOrder_PreservesInlineEnumValuesAndDefault()
    {
        using var provider = CreateSwaggerProvider();
        var document = provider.GetRequiredService<ISwaggerProvider>().GetSwagger("InternalSosAnalysisv1.5");
        var parameter = document.Paths["/internal/aggregation_simple"].Operations[HttpMethod.Post]
            .Parameters.Single(parameter => parameter.Name == "sortOrder");
        var schema = Assert.IsType<OpenApiSchema>(parameter.Schema);

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.Equal(new[]
        {
            "CountDescending", "CountAscending", "KeyAscending", "KeyDescending",
            "OrganismQuantityAscending", "OrganismQuantityDescending"
        }, schema.Enum.Select(value => value!.GetValue<string>()));
        Assert.Equal("CountDescending", schema.Default!.GetValue<string>());
        Assert.DoesNotContain("AggregationSortOrder", document.Components.Schemas.Keys);
        Assert.DoesNotContain("AggregationSortOrderDto", document.Components.Schemas.Keys);
    }

    [Fact]
    public void GeographicsFilterAndFeatureResponses_UseSharedGeometrySchema()
    {
        using var provider = CreateSwaggerProvider();
        var document = provider.GetRequiredService<ISwaggerProvider>().GetSwagger("InternalSosAnalysisv1.5");
        var schemas = document.Components.Schemas;

        Assert.Equal("Geometry", Assert.IsType<OpenApiSchemaReference>(
            schemas["GeographicsFilterDto"].Properties["geometries"].Items).Reference.Id);
        Assert.Equal("Geometry", Assert.IsType<OpenApiSchemaReference>(
            schemas["IFeature"].Properties["geometry"]).Reference.Id);
        Assert.DoesNotContain("IGeometryObject", schemas.Keys);
        Assert.DoesNotContain("GeoJSONObjectType", schemas.Keys);
        Assert.True(schemas["Geometry"].AdditionalPropertiesAllowed);
        Assert.Equal(7, schemas["Geometry"].OneOf.Count);
    }

    private static ServiceProvider CreateSwaggerProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.ApplicationName).Returns(typeof(AnalysisController).Assembly.GetName().Name!);
        environment.SetupGet(x => x.EnvironmentName).Returns("Development");
        environment.SetupGet(x => x.ContentRootPath).Returns(AppContext.BaseDirectory);
        environment.SetupGet(x => x.ContentRootFileProvider).Returns(new NullFileProvider());
        environment.SetupGet(x => x.WebRootPath).Returns(AppContext.BaseDirectory);
        environment.SetupGet(x => x.WebRootFileProvider).Returns(new NullFileProvider());
        services.AddSingleton(environment.Object);
        services.AddSingleton<IHostEnvironment>(environment.Object);
        services.AddControllers().AddApplicationPart(typeof(AnalysisController).Assembly)
            .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.SetupSwagger();

        return services.BuildServiceProvider();
    }
}
