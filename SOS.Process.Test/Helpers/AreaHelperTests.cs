using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using MongoDB.Driver.GeoJsonObjectModel;
using Moq;
using NetTopologySuite.Features;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Helpers;
using SOS.Process.Mappings;
using SOS.Process.Repositories.Source;
using SOS.Process.Repositories.Source.Interfaces;
using SOS.Process.Test.TestRepositories;
using Xunit;

namespace SOS.Process.Test.Helpers
{
    public class AreaHelperTests
    {
        [Fact]
        [Trait("Category","Unit")]
        public void ProvincePartIdByCoordinateShouldBeSetToLappland_When_ObservationIsInLappmark()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var provinceAreas = AreasTestRepository.LoadAreas(new[] { AreaType.County, AreaType.Province});
            var areaHelper = new AreaHelper(CreateAreaVerbatimRepositoryMock(provinceAreas).Object);
            var observations = new List<DarwinCore<DynamicProperties>>();
            var observation = new DarwinCore<DynamicProperties>(DataProviderId.SpeciesPortal)
            {
                Location = new DarwinCoreLocation
                {
                    DecimalLatitude = Coordinates.KirunaMunicipality.Latitude, 
                    DecimalLongitude = Coordinates.KirunaMunicipality.Longitude
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
            observation.DynamicProperties.ProvincePartIdByCoordinate.Should().Be((int) ProvinceFeatureId.Lappland);
            observation.DynamicProperties.ProvinceIdByCoordinate.Should().Be((int)ProvinceFeatureId.AseleLappmark);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CountyPartIdByCoordinateShouldBeSetToOland_When_ObservationIsOnOland()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var provinceAreas = AreasTestRepository.LoadAreas(new[] { AreaType.County, AreaType.Province });
            var areaHelper = new AreaHelper(CreateAreaVerbatimRepositoryMock(provinceAreas).Object);
            var observations = new List<DarwinCore<DynamicProperties>>();
            var observation = new DarwinCore<DynamicProperties>(DataProviderId.SpeciesPortal)
            {
                Location = new DarwinCoreLocation
                {
                    DecimalLatitude = Coordinates.BorgholmMunicipality.Latitude,
                    DecimalLongitude = Coordinates.BorgholmMunicipality.Longitude
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
            observation.DynamicProperties.CountyPartIdByCoordinate.Should().Be((int)CountyFeatureId.Oland);
            observation.DynamicProperties.CountyIdByCoordinate.Should().Be((int)CountyFeatureId.Kalmar);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CountyPartIdShouldBeSetToKalmarFastland_When_ObservationIsInKalmar()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var provinceAreas = AreasTestRepository.LoadAreas(new[] { AreaType.County, AreaType.Province });
            var areaHelper = new AreaHelper(CreateAreaVerbatimRepositoryMock(provinceAreas).Object);
            var observations = new List<DarwinCore<DynamicProperties>>();
            var observation = new DarwinCore<DynamicProperties>(DataProviderId.SpeciesPortal)
            {
                Location = new DarwinCoreLocation
                {
                    DecimalLatitude = Coordinates.KalmarMunicipality.Latitude,
                    DecimalLongitude = Coordinates.KalmarMunicipality.Longitude
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
            observation.DynamicProperties.CountyPartIdByCoordinate.Should().Be((int)CountyFeatureId.KalmarFastland);
            observation.DynamicProperties.CountyIdByCoordinate.Should().Be((int)CountyFeatureId.Kalmar);
        }

        private Mock<IAreaVerbatimRepository> CreateAreaVerbatimRepositoryMock(IEnumerable<Area> areas)
        {
            Mock<IAreaVerbatimRepository> areaVerbatimRepositoryMock = new Mock<IAreaVerbatimRepository>();
            areaVerbatimRepositoryMock
                .Setup(avm => avm.GetBatchAsync(0))
                .ReturnsAsync(areas);

            return areaVerbatimRepositoryMock;
        }
    }
}
