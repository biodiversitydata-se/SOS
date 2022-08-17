
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using Xunit;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ExportsController.DownloadDwCEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class DownloadGeoJsonTests
    {
        private readonly IntegrationTestFixture _fixture;

        private FeatureCollection ReadGeoJsonFile(byte[] file)
        {
            var items = new List<Dictionary<string, string>>();
            using (var readMemoryStream = new MemoryStream(file))
            {
                var geoJsonString = Encoding.UTF8.GetString(file, 0, file.Length);
                var reader = new NetTopologySuite.IO.GeoJsonReader();

                // pass geoJson's FeatureCollection to read all the features
                var featureCollection = reader.Read<FeatureCollection>(geoJsonString);
              
                return featureCollection;
            }
        }

        public DownloadGeoJsonTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task DownloadGeoJsonFile_MinimumFieldSet()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto()
            {
                Output = new OutputFilterDto()
                {
                    //Fields = new List<string> { "Occurrence.OccurrenceId", "Event.StartDate", "Location.DecimalLatitude"}
                    FieldSet = OutputFieldSet.Minimum
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var geoJsonFileResult = await _fixture.ExportsController.DownloadGeoJson(
                null,
                null,
                searchFilter,
                OutputFieldSet.Minimum,
                gzip: false);
            var file = (FileContentResult)geoJsonFileResult;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            file.FileContents.Length.Should().BeGreaterThan(0);
            var fileEntries = ReadGeoJsonFile(file.FileContents);
            fileEntries.Count.Should().Be(100);
        }
    }
}
