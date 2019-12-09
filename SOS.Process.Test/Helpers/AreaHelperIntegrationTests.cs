using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Mappings;
using SOS.Process.Repositories.Source;
using SOS.Process.Test.TestRepositories;
using Xunit;

namespace SOS.Process.Test.Helpers
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
            List<DarwinCore<DynamicProperties>> observations = new List<DarwinCore<DynamicProperties>>();
            DarwinCore<DynamicProperties> observation = new DarwinCore<DynamicProperties>(DataProvider.Artdatabanken)
            {
                Location = new DarwinCoreLocation
                {
                    DecimalLatitude = Coordinates.TranasMunicipality.Latitude,
                    DecimalLongitude = Coordinates.TranasMunicipality.Longitude 
                }
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

            observation.DynamicProperties.CountyIdByCoordinate.Should().Be(6); // FeatureId
            observation.DynamicProperties.MunicipalityIdByCoordinate.Should().Be(687); // FeatureId
            observation.DynamicProperties.ParishIdByCoordinate.Should().Be(671); // FeatureId
            observation.DynamicProperties.ProvinceIdByCoordinate.Should().Be(3); // FeatureId
            observation.DynamicProperties.CountyPartIdByCoordinate.Should().Be(6); // FeatureId
            observation.DynamicProperties.ProvincePartIdByCoordinate.Should().Be(3); // FeatureId
            //observation.DynamicProperties.CountyIdByCoordinate.Should().Be(7); // Id
            //observation.DynamicProperties.MunicipalityIdByCoordinate.Should().Be(283); // Id
            //observation.DynamicProperties.ParishIdByCoordinate.Should().Be(42769); // Id
            //observation.DynamicProperties.ProvinceIdByCoordinate.Should().Be(8060); // Id
        }
    }
}