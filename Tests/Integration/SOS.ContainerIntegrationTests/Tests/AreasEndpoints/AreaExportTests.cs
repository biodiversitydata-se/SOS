using FluentAssertions.Execution;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData;
using SOS.Observations.Api.Dtos;
using System.IO.Compression;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsBySearchEndpoint;

/// <summary>
/// Integration tests for export area to zip file endpoint.
/// </summary>
[Collection(TestCollection.Name)]
public class AreaExportTests : TestBase
{
    public AreaExportTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }
    
    [Fact]    
    public async Task AreaExportEndpoint_UsesCamelCasePropertyNames_WhenExportingToGeoJson()
    {
        // Arrange                
        var apiClient = TestFixture.CreateApiClient();
        AreaExportFormatDto exportFormat = AreaExportFormatDto.GeoJson;
        string featureId = AreaFeatureIds.Enköping;

        // Act
        var response = await apiClient.GetAsync($"/areas/municipality/{featureId}/export?format={exportFormat}");
        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var archive = new ZipArchive(contentStream, ZipArchiveMode.Read);
        using var entryStream = archive.Entries.Single().Open();
        using var reader = new StreamReader(entryStream);
        string geoJsonContent = await reader.ReadToEndAsync();
        using JsonDocument document = JsonDocument.Parse(geoJsonContent);
        JsonElement root = document.RootElement;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);        
        using (new AssertionScope())
        {
            root.TryGetProperty("type", out var typeJsonElement).Should().BeTrue();
            root.TryGetProperty("geometry", out var geometryJsonElement).Should().BeTrue();
            root.TryGetProperty("properties", out var propertiesJsonElement).Should().BeTrue();
        }
    }

    /// <summary>
    /// Länsstyrelsen is expecting initial upper case letter in property names when exporting to JSON.
    /// </summary>    
    [Fact]
    public async Task AreaExportEndpoint_UsesPascalCasePropertyNames_WhenExportingToJson()
    {
        // Arrange                
        var apiClient = TestFixture.CreateApiClient();
        AreaExportFormatDto exportFormat = AreaExportFormatDto.Json;
        string featureId = AreaFeatureIds.Enköping;

        // Act
        var response = await apiClient.GetAsync($"/areas/municipality/{featureId}/export?format={exportFormat}");
        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var archive = new ZipArchive(contentStream, ZipArchiveMode.Read);
        using var entryStream = archive.Entries.Single().Open();
        using var reader = new StreamReader(entryStream);
        string jsonContent = await reader.ReadToEndAsync();
        using JsonDocument document = JsonDocument.Parse(jsonContent);
        JsonElement root = document.RootElement;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (new AssertionScope())
        {
            root.TryGetProperty("AreaType", out var areaElement).Should().BeTrue();
            root.TryGetProperty("FeatureId", out var featureIdElement).Should().BeTrue();
            root.TryGetProperty("Name", out var nameElement).Should().BeTrue();
            root.TryGetProperty("BoundingBox", out var boundingBoxElement).Should().BeTrue();
            root.TryGetProperty("Geometry", out var geometryElement).Should().BeTrue();
            geometryElement.TryGetProperty("Coordinates", out var coordinatesElement).Should().BeTrue();
            geometryElement.TryGetProperty("Type", out var typeElement).Should().BeTrue();            
        }
    }
}