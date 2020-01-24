using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using  SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Process.Database;
using SOS.Process.Helpers;
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
            var observation = new ProcessedSighting(DataProvider.Artdatabanken)
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
            areaHelper.AddAreaDataToProcessed(observations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.County.Should().Be("Jönköping");
            observation.Location.Municipality.Should().Be("Tranås");
            observation.Location.Province.Should().Be("Småland");
            observation.Location.Parish.Should().Be("Tranås");
        }

        private AreaHelper CreateAreaHelper()
        {
            var processConfiguration = GetProcessConfiguration();
            var verbatimClient = new VerbatimClient(
                processConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                processConfiguration.VerbatimDbConfiguration.DatabaseName,
                processConfiguration.VerbatimDbConfiguration.BatchSize);
            AreaVerbatimRepository areaVerbatimRepository = new AreaVerbatimRepository(
                verbatimClient,
                new Mock<ILogger<AreaVerbatimRepository>>().Object);
            var areaHelper = new AreaHelper(
                areaVerbatimRepository);

            return areaHelper;
        }
    }
}