using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Models.DarwinCore;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.Test.Helpers
{
    public class AreaHelperIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public void TestGetAreas()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processConfiguration = GetProcessConfiguration();
            var verbatimClient = new VerbatimClient(
                processConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                processConfiguration.VerbatimDbConfiguration.DatabaseName,
                processConfiguration.VerbatimDbConfiguration.BatchSize);
            AreaVerbatimRepository areaVerbatimRepository = new AreaVerbatimRepository(
                verbatimClient,
                new Mock<ILogger<AreaVerbatimRepository>>().Object);
            AreaHelper areaHelper = new AreaHelper(areaVerbatimRepository);
            List<DarwinCore<DynamicProperties>> observations = new List<DarwinCore<DynamicProperties>>();

            var tranaasMunicipalityCoord = (Longitude: 14.98996, Latitude: 58.01539);

            DarwinCore<DynamicProperties> observation = new DarwinCore<DynamicProperties>
            {
                Location = new DarwinCoreLocation { DecimalLongitude = tranaasMunicipalityCoord.Longitude, DecimalLatitude = tranaasMunicipalityCoord.Latitude }
            };
            observations.Add(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            areaHelper.AddAreaDataToDarwinCore(observations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.County.Should().Be("Jönköping");
            observation.Location.Municipality.Should().Be("Tranås");
            observation.Location.StateProvince.Should().Be("Småland");
            observation.DynamicProperties.Parish.Should().Be("Tranås");

            observation.DynamicProperties.CountyIdByCoordinate.Should().Be(7);
            observation.DynamicProperties.MunicipalityIdByCoordinate.Should().Be(283);
            observation.DynamicProperties.ParishIdByCoordinate.Should().Be(42769);
            observation.DynamicProperties.ProvinceIdByCoordinate.Should().Be(8060);
        }
    }
}
