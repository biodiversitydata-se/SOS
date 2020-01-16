using System.Collections.Generic;
using FluentAssertions;
using Moq;
using SOS.Lib.Enums;
using  SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Helpers;
using SOS.Process.Mappings;
using SOS.Process.Repositories.Source.Interfaces;
using SOS.Process.UnitTests.TestHelpers;
using Xunit;

namespace SOS.Process.UnitTests.Helpers
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
            var areaHelper = new AreaHelper(
                CreateAreaVerbatimRepositoryMock(provinceAreas).Object);
            var observations = new List<ProcessedSighting>();
            var observation = new ProcessedSighting(DataProvider.Artdatabanken)
            {
                Location = new ProcessedLocation
                {
                    DecimalLatitude = SOS.TestHelpers.Gis.Coordinates.FalkenbergMunicipality.Latitude,
                    DecimalLongitude = SOS.TestHelpers.Gis.Coordinates.FalkenbergMunicipality.Longitude
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
            observation.Location.County.Id.Should().Be((int)CountyFeatureId.Halland);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ProvincePartIdByNameShouldBeSetToLappland_When_StateProvinceIsTorneLappmark()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var provinceAreas = AreasTestRepository.LoadAreas(new[] { AreaType.County, AreaType.Province });
            var areaHelper = new AreaHelper(
                CreateAreaVerbatimRepositoryMock(provinceAreas).Object);
            var observations = new List<ProcessedSighting>();
            var observation = new ProcessedSighting(DataProvider.Artdatabanken)
            {
                Location = new ProcessedLocation
                {
                    DecimalLatitude = SOS.TestHelpers.Gis.Coordinates.KirunaMunicipality.Latitude,
                    DecimalLongitude = SOS.TestHelpers.Gis.Coordinates.KirunaMunicipality.Longitude
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
            observation.Location.Province.Id.Should().Be((int)ProvinceFeatureId.Lappland);
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
            var areaHelper = new AreaHelper(
                CreateAreaVerbatimRepositoryMock(provinceAreas).Object);
            var observations = new List<ProcessedSighting>();
            var observation = new ProcessedSighting(DataProvider.Artdatabanken)
            {
                Location = new ProcessedLocation
                {
                    DecimalLatitude = SOS.TestHelpers.Gis.Coordinates.KirunaMunicipality.Latitude,
                    DecimalLongitude = SOS.TestHelpers.Gis.Coordinates.KirunaMunicipality.Longitude
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
            observation.Location.ProvincePartIdByCoordinate.Should()
                .Be((int)ProvinceFeatureId.Lappland, "because the coordinate is in Kiruna");
            /*observation.ProvincePartIdByName.Should()
                .NotBe((int) ProvinceFeatureId.Lappland, "because we set the StateProvince name property to Öland")
                .And.Be((int) ProvinceFeatureId.Oland, "because we set the StateProvince name property to Öland");
            observation.DynamicProperties.CountyPartIdByName.Should().Be((int) CountyFeatureId.Oland);*/
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
