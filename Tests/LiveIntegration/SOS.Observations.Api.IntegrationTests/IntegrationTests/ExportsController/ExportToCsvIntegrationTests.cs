using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using NetTopologySuite.Geometries;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ExportsController
{
    [Collection(Fixtures.Collections.ApiIntegrationTestsCollection)]
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
                    Geometries = new List<Geometry>()
                    {
                        new Polygon(new LinearRing(
                                [
                                    new Coordinate(15.07063, 57.92573),
                                    new Coordinate(15.00510, 58.16108),
                                    new Coordinate(14.58003, 58.10148),
                                    new Coordinate(14.64143, 57.93294),
                                    new Coordinate(15.07063, 57.92573)
                                ]
                            )
                        )
                    },
                    ConsiderObservationAccuracy = true
                },
                VerificationStatus = StatusVerificationDto.BothVerifiedAndNotVerified,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ExportsController.DownloadCsvAsync(
                null,
                null,
                searchFilter,
                OutputFieldSet.Minimum,
                false,
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
        public async Task Export_to_CSV_filter_with_projects()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilterDto
            {
                ProjectIds = new List<int> { 2976 },
                Date = new DateFilterDto()
                {
                    StartDate = new DateTime(2016, 9, 1),
                    EndDate = new DateTime(2016, 9, 30),
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ExportsController.DownloadCsvAsync(
                null,
                null,
                searchFilter,
                OutputFieldSet.Minimum,
                false,
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
            var response = await _fixture.ExportsController.DownloadCsvAsync(
                null,
                null,
                searchFilter,
                OutputFieldSet.Minimum,
                false,
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