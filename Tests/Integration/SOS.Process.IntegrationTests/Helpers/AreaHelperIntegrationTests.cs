using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.IntegrationTests.Helpers
{
    public class AreaHelperIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public void GetAreasFromMongoDb_And_GetRegionBelongingsForTranasMunicipality()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var areaHelper = CreateAreaHelper();
            var observations = new List<ProcessedSighting>();
            var observation = new ProcessedSighting(DataProvider.SpeciesPortal)
            {
                Location = new ProcessedLocation
                {
                    DecimalLatitude = TestHelpers.Gis.Coordinates.TranasMunicipality.Latitude,
                    DecimalLongitude = TestHelpers.Gis.Coordinates.TranasMunicipality.Longitude
                }
            };
            observations.Add(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            areaHelper.AddAreaDataToProcessedSightings(observations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            areaHelper.AddValueDataToGeographicalFields(observation);
            observation.Location.CountyId.Value.Should().Be("Jönköping");
            observation.Location.MunicipalityId.Value.Should().Be("Tranås");
            observation.Location.ProvinceId.Value.Should().Be("Småland");
            observation.Location.ParishId.Value.Should().Be("Tranås");
        }

        private AreaHelper CreateAreaHelper()
        {
            var processConfiguration = GetProcessConfiguration();
            var verbatimClient = new VerbatimClient(
                processConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                processConfiguration.VerbatimDbConfiguration.DatabaseName,
                processConfiguration.VerbatimDbConfiguration.BatchSize);
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
            var areaVerbatimRepository = new AreaVerbatimRepository(
                verbatimClient,
                new Mock<ILogger<AreaVerbatimRepository>>().Object);
            var processedFieldMappingRepository = new ProcessedFieldMappingRepository(
                processClient,
                new NullLogger<ProcessedFieldMappingRepository>());
            var areaHelper = new AreaHelper(
                areaVerbatimRepository,
                processedFieldMappingRepository);

            return areaHelper;
        }
    }
}