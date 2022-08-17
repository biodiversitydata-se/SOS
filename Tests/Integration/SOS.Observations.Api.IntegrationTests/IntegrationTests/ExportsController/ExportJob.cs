using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Nest;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
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

            var searchFilter = new SearchFilterDto()
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
            var response = await _fixture.ExportsController.DownloadExcel(
                null,
                null, 
                searchFilter, 
                OutputFieldSet.Minimum, 
                PropertyLabelType.Swedish, 
                "sv-SE");
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

            var searchFilter = new SearchFilterDto()
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
            var response = await _fixture.ExportsController.DownloadGeoJson(
                null,
                null,
                searchFilter, 
                OutputFieldSet.Minimum, 
                PropertyLabelType.Swedish, 
                "sv-SE");
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
            var searchFilter = JsonSerializer.Deserialize<SearchFilterDto>(str, jsonSerializerOptions);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ExportsController.DownloadGeoJson(
                null,
                null,
                searchFilter, 
                OutputFieldSet.Minimum, 
                PropertyLabelType.Swedish, 
                "sv-SE");
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
        public async Task Convert_filter_geometries_to_GeoJson_FeatureCollection()
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
            FeatureCollection featureCollection = new FeatureCollection();
            var geoJsonReader = new GeoJsonReader();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            foreach (var geometry in searchFilter.Geographics.Geometries)
            {
                string strGeometry = JsonSerializer.Serialize(geometry, jsonSerializerOptions);
                var multiPolygon = geoJsonReader.Read<MultiPolygon>(strGeometry);
                var feature = new Feature { Geometry = multiPolygon };
                featureCollection.Add(feature);
            }

            GeoJsonWriter geoJsonWriter = new GeoJsonWriter();
            var strJson = geoJsonWriter.Write(featureCollection);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strJson.Should().NotBeNull();
        }

        private static Feature LoadFeature(string filePath)
        {
            var geoJsonReader = new GeoJsonReader();
            var str = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
            var feature = geoJsonReader.Read<Feature>(str);
            return feature;
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