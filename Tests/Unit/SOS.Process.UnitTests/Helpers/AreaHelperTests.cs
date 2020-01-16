using System.Collections.Generic;
using FluentAssertions;
using Moq;
using SOS.Lib.Enums;
using  SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Helpers;
using SOS.Process.Repositories.Source.Interfaces;
using SOS.Process.UnitTests.TestHelpers;
using SOS.TestHelpers.Gis;
using Xunit;

namespace SOS.Process.UnitTests.Helpers
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
            var observations = new List<ProcessedSighting> ();
            var observation = new ProcessedSighting(DataProvider.Artdatabanken)
            {
                Location = new ProcessedLocation
                {
                    DecimalLatitude = Coordinates.KirunaMunicipality.Latitude, 
                    DecimalLongitude = Coordinates.KirunaMunicipality.Longitude
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
            observation.Location.ProvincePartIdByCoordinate.Should().Be((int) ProvinceFeatureId.Lappland);
            observation.Location.Province.Id.Should().Be((int)ProvinceFeatureId.AseleLappmark);
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
            var observations = new List<ProcessedSighting>();
            var observation = new ProcessedSighting(DataProvider.Artdatabanken)
            {
                Location = new ProcessedLocation
                {
                    DecimalLatitude = Coordinates.BorgholmMunicipality.Latitude,
                    DecimalLongitude = Coordinates.BorgholmMunicipality.Longitude
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
            observation.Location.CountyPartIdByCoordinate.Should().Be((int)CountyFeatureId.Oland);
            observation.Location.County.Id.Should().Be((int)CountyFeatureId.Kalmar);
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
            var observations = new List<ProcessedSighting>();
            var observation = new ProcessedSighting(DataProvider.Artdatabanken)
            {
                Location = new ProcessedLocation
                {
                    DecimalLatitude = Coordinates.KalmarMunicipality.Latitude,
                    DecimalLongitude = Coordinates.KalmarMunicipality.Longitude
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
            observation.Location.CountyPartIdByCoordinate.Should().Be((int)CountyFeatureId.KalmarFastland);
            observation.Location.County.Id.Should().Be((int)CountyFeatureId.Kalmar);
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
