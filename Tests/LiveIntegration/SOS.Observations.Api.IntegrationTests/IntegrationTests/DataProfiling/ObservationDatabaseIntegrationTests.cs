using FluentAssertions;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.DataProfiling
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class ObservationDatabaseIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public ObservationDatabaseIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Create_ObservationDatabase_File_With_All_CatalogNumbers()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var filter = new SearchFilter(0);
            filter.DataProviderIds = new List<int> { 17 };
            filter.ExtendedAuthorization = new ExtendedAuthorizationFilter
            {
                ProtectionFilter = Lib.Enums.ProtectionFilter.BothPublicAndSensitive,
                UserId = 1
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var catalogNumbers = await _fixture.ProcessedObservationRepository.GetAllAggregationItemsAsync(filter, "occurrence.catalogNumber");
            string strJson = System.Text.Json.JsonSerializer.Serialize(catalogNumbers);
            File.WriteAllText(Path.Join(@"C:\temp\2023-03-16", "sos-obsdb-prod.json"), strJson);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            catalogNumbers.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Compare_ObservationDatabase_SOS_vs_SSOS()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------                        
            var strSosJson = File.ReadAllText(@"C:\temp\2023-03-16\sos-obsdb-prod.json");
            var sosItems = System.Text.Json.JsonSerializer.Deserialize<List<AggregationItem>>(strSosJson);
            List<int> sosIds = sosItems
                .Select(m => int.Parse(m.AggregationKey))
                .Order()
                .ToList();

            var strSsosCsv = File.ReadAllText(@"C:\temp\2023-03-16\observationsdatabasen.csv");
            List<int> ssosIds = new List<int>();
            string[] lines = strSsosCsv.Split('\n');
            foreach (string line in lines)
            {
                if (int.TryParse(line, out int id))
                    ssosIds.Add(id);
            }
            ssosIds = ssosIds.Order().ToList();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var uniqueSosIds = sosIds.Except(ssosIds).ToList();
            var uniqueSsosIds = ssosIds.Except(sosIds).ToList();


            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create error information
            //-----------------------------------------------------------------------------------------------------------            
            var allInvalidObservations = await _fixture.InvalidObservationRepository.GetAllAsync();
            var obsDbInvalidObservationsByOccurrenceId = allInvalidObservations
                .Where(m => m.DatasetID == "17")
                .ToDictionary(m => m.OccurrenceID, m => m);

            //-----------------------------------------------------------------------------------------------------------
            // Act - Create error information
            //-----------------------------------------------------------------------------------------------------------
            List<string> uniqueSosIdsErrors = new List<string>();
            foreach (var catalogNumber in uniqueSosIds)
            {
                string occurrenceId = $"urn:lsid:observationsdatabasen.se:observation:{catalogNumber}";
                if (obsDbInvalidObservationsByOccurrenceId.TryGetValue(occurrenceId, out var invalidObservation))
                {
                    string description = string.Join(", ", invalidObservation.Defects.Select(m => m.Information));
                    uniqueSosIdsErrors.Add(description);
                }
                else
                {
                    uniqueSosIdsErrors.Add("");
                }
            }

            List<string> uniqueSsosIdsErrors = new List<string>();
            foreach (var catalogNumber in uniqueSsosIds)
            {
                string occurrenceId = $"urn:lsid:observationsdatabasen.se:observation:{catalogNumber}";
                if (obsDbInvalidObservationsByOccurrenceId.TryGetValue(occurrenceId, out var invalidObservation))
                {
                    string description = string.Join(", ", invalidObservation.Defects.Select(m => m.Information));
                    uniqueSsosIdsErrors.Add(description);
                }
                else
                {
                    uniqueSsosIdsErrors.Add("");
                }
            }

            // Summarize in DataTable in order to use Tabular visualizer in Visual Studio which supports Excel export.
            DataTable dt = new DataTable();
            dt.Columns.Add("OccurrenceID");
            dt.Columns.Add("CatalogNumber");
            dt.Columns.Add("Description");
            foreach (var error in obsDbInvalidObservationsByOccurrenceId.Values)
            {
                MatchCollection matches = Regex.Matches(error.OccurrenceID, @"\d+");
                int catalogNumber = int.Parse(matches.Single().Value);
                dt.Rows.Add(error.OccurrenceID, catalogNumber, string.Join(", ", error.Defects.Select(m => m.Information)));
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            sosIds.Should().NotBeEmpty();
        }
    }
}