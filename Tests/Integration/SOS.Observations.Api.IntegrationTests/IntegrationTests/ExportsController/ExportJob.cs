using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Nest;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.JsonConverters;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ExportsController
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class ExportJob
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public ExportJob(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Export_to_Excel_uttag()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string geojsonFilePath = @"C:\GIS\polygon_boundaries.geojson";
            var featureCollection = LoadFeatureCollection(geojsonFilePath);
            var feature = featureCollection[0];
            var geometry = feature.Geometry;
            var geoShape = geometry.ToGeoShape();

            ExportFilterDto searchFilter = new ExportFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new List<IGeoShape>()
                    {
                        geoShape
                    },
                    ConsiderObservationAccuracy = false
                },
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ExportsController.DownloadExcel(searchFilter, OutputFieldSet.AllWithKnownValues, PropertyLabelType.Swedish, "sv-SE");
            var bytes = response.GetFileContentResult();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            bytes.Length.Should().BeGreaterThan(0);

            var filename = FilenameHelper.CreateFilenameWithDate("excel_export","zip");
            var filePath = System.IO.Path.Combine(@"C:\temp\", filename);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Export_to_GeoJSON_uttag()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string geojsonFilePath = @"C:\GIS\polygon_boundaries.geojson";
            var featureCollection = LoadFeatureCollection(geojsonFilePath);
            var feature = featureCollection[0];
            var geometry = feature.Geometry;
            var geoShape = geometry.ToGeoShape();

            ExportFilterDto searchFilter = new ExportFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new List<IGeoShape>()
                    {
                        geoShape
                    },
                    ConsiderObservationAccuracy = false
                },
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };
            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ExportsController.DownloadGeoJson(searchFilter, OutputFieldSet.AllWithKnownValues, PropertyLabelType.Swedish, "sv-SE");
            var bytes = response.GetFileContentResult();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            bytes.Length.Should().BeGreaterThan(0);

            var filename = FilenameHelper.CreateFilenameWithDate("geojson_export", "zip");
            var filePath = System.IO.Path.Combine(@"C:\temp\", filename);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Export_to_GeoJSON_Uttag_Filter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string jsonFilePath = @"C:\GIS\Uttag filter\uttag_filter.json";
            var str = await System.IO.File.ReadAllTextAsync(jsonFilePath, Encoding.UTF8);
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new GeoShapeConverter(), new GeoLocationConverter(), new JsonStringEnumConverter() }
            };
            var searchFilter = JsonSerializer.Deserialize<ExportFilterDto>(str, jsonSerializerOptions);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ExportsController.DownloadGeoJson(searchFilter, OutputFieldSet.AllWithKnownValues, PropertyLabelType.Swedish, "sv-SE");
            var bytes = response.GetFileContentResult();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            bytes.Length.Should().BeGreaterThan(0);
            var filename = FilenameHelper.CreateFilenameWithDate("geojson_export", "zip");
            var filePath = System.IO.Path.Combine(@"C:\temp\", filename);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);
        }

        private static FeatureCollection LoadFeatureCollection(string filePath)
        {
            var geoJsonReader = new GeoJsonReader();
            var str = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
            var featureCollection = geoJsonReader.Read<FeatureCollection>(str);
            return featureCollection;
        }
    }
}