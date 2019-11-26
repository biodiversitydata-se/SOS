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
    public class AreaHelperWithNameMappingTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void CountyPartIdByNameShouldBeSetToHalland_When_CountyNameIsHallnad()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var provinceAreas = AreasTestRepository.LoadAreas(new[] { AreaType.County, AreaType.Province });
            var areaHelper = new AreaHelperWithNameMapping(
                CreateAreaVerbatimRepositoryMock(provinceAreas).Object,
                new AreaNameMapper());
            var observations = new List<DarwinCore<DynamicProperties>>();
            var observation = new DarwinCore<DynamicProperties>
            {
                Location = new DarwinCoreLocation
                {
                    DecimalLatitude = Coordinates.FalkenbergMunicipality.Latitude,
                    DecimalLongitude = Coordinates.FalkenbergMunicipality.Longitude,
                    County = "Hallnad"
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
            observation.DynamicProperties.CountyIdByName.Should().Be((int)CountyFeatureId.Halland);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ProvincePartIdByNameShouldBeSetToLappland_When_StateProvinceIsTorneLappmark()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var provinceAreas = AreasTestRepository.LoadAreas(new[] { AreaType.County, AreaType.Province });
            var areaHelper = new AreaHelperWithNameMapping(
                CreateAreaVerbatimRepositoryMock(provinceAreas).Object,
                new AreaNameMapper());
            var observations = new List<DarwinCore<DynamicProperties>>();
            var observation = new DarwinCore<DynamicProperties>
            {
                Location = new DarwinCoreLocation
                {
                    DecimalLatitude = Coordinates.KirunaMunicipality.Latitude,
                    DecimalLongitude = Coordinates.KirunaMunicipality.Longitude,
                    StateProvince = "Torne lappmark"
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
            observation.DynamicProperties.ProvincePartIdByName.Should().Be((int)ProvinceFeatureId.Lappland);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ProvincePartIdByNameShouldNotBeSetToLappland_When_StateProvinceIsNotTorneLappmark()
        {
            //todo - testa punkt på öland
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var provinceAreas = AreasTestRepository.LoadAreas(new[] { AreaType.County, AreaType.Province });
            var areaHelper = new AreaHelperWithNameMapping(
                CreateAreaVerbatimRepositoryMock(provinceAreas).Object,
                new AreaNameMapper());
            var observations = new List<DarwinCore<DynamicProperties>>();
            var observation = new DarwinCore<DynamicProperties>
            {
                Location = new DarwinCoreLocation
                {
                    DecimalLatitude = Coordinates.KirunaMunicipality.Latitude,
                    DecimalLongitude = Coordinates.KirunaMunicipality.Longitude,
                    StateProvince = "Öland",
                    County = "Kalmar"
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
            observation.DynamicProperties.ProvincePartIdByCoordinate.Should()
                .Be((int)ProvinceFeatureId.Lappland, "because the coordinate is in Kiruna");
            observation.DynamicProperties.ProvincePartIdByName.Should()
                .NotBe((int) ProvinceFeatureId.Lappland, "because we set the StateProvince name property to Öland")
                .And.Be((int) ProvinceFeatureId.Oland, "because we set the StateProvince name property to Öland");
            observation.DynamicProperties.CountyPartIdByName.Should().Be((int) CountyFeatureId.Oland);
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
