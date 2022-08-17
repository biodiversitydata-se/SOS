using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.ExportsController
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class ExportToCsvIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public ExportToCsvIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Export_to_CSV_filter_with_polygon_geometry()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterDto
            {
                Taxon = new TaxonFilterDto { Ids = new List<int> { TestData.TaxonIds.Otter }, IncludeUnderlyingTaxa = true },
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new List<IGeoShape>()
                    {
                        new PolygonGeoShape(new List<List<GeoCoordinate>> { new List<GeoCoordinate>
                            {
                                new GeoCoordinate(57.92573, 15.07063),
                                new GeoCoordinate(58.16108, 15.00510),
                                new GeoCoordinate(58.10148, 14.58003),
                                new GeoCoordinate(57.93294, 14.64143),
                                new GeoCoordinate(57.92573, 15.07063)
                            }
                        })
                    },
                    ConsiderObservationAccuracy = true
                },
                VerificationStatus = SearchFilterBaseDto.StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ExportsController.DownloadCsv(
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

            var filename = FilenameHelper.CreateFilenameWithDate("csv_export","zip");
            var filePath = System.IO.Path.Combine(@"C:\temp\", filename);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Export_to_CSV_filter_with_projects()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterDto
            {
                ProjectIds = new List<int> {2976},
                Date = new DateFilterDto()
                {
                    StartDate = new DateTime(2016,9,1),
                    EndDate = new DateTime(2016, 9, 30),
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ExportsController.DownloadCsv(
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

            var filename = FilenameHelper.CreateFilenameWithDate("csv_export", "zip");
            var filePath = System.IO.Path.Combine(@"C:\temp\", filename);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Export_to_CSV_No_gzip()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterDto
            {
                DataProvider = new DataProviderFilterDto()
                {
                    Ids = new List<int> { 1 }
                },
                Date = new DateFilterDto()
                {
                    StartDate = new DateTime(2021, 4, 23),
                    EndDate = new DateTime(2021, 4, 23),
                }
            };


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ExportsController.DownloadCsv(
                null,
                null,
                searchFilter,
                OutputFieldSet.Minimum,
                PropertyLabelType.Swedish, 
                "sv-SE",
                false);
            var bytes = response.GetFileContentResult();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            bytes.Length.Should().BeGreaterThan(0);

            var filename = FilenameHelper.CreateFilenameWithDate("csv_export", "csv");
            var filePath = System.IO.Path.Combine(@"C:\temp\", filename);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);
        }

    }
}