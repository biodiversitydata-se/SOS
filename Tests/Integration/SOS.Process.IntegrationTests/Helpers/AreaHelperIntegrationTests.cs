using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;
using SOS.TestHelpers.Gis;
using Xunit;

namespace SOS.Process.IntegrationTests.Helpers
{
    public class AreaHelperIntegrationTests : IClassFixture<AreaHelperFixture>
    {
        public AreaHelperIntegrationTests(AreaHelperFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly AreaHelperFixture _fixture;

        [Fact]
        [Trait("Category", "Integration")]
        public void CountyPartIdByCoordinateShouldBeSetToOland_When_ObservationIsOnOland()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var observations = new List<Observation>();
            var observation = new Observation
            {
                Location = new Location(LocationType.Point)
                {
                    DecimalLatitude = Coordinates.BorgholmMunicipality.Latitude,
                    DecimalLongitude = Coordinates.BorgholmMunicipality.Longitude
                }
            };
            observations.Add(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            _fixture.AreaHelper.AddAreaDataToProcessedLocations(observations.Select(o => o.Location));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.Attributes.CountyPartIdByCoordinate.Should().Be((string) SpecialCountyPartId.Öland);
            observation.Location.County.FeatureId.Should().Be((string) CountyId.Kalmar);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CountyPartIdShouldBeSetToKalmarFastland_When_ObservationIsInKalmar()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var observations = new List<Observation>();
            var observation = new Observation
            {
                Location = new Location(LocationType.Point)
                {
                    DecimalLatitude = Coordinates.KalmarMunicipality.Latitude,
                    DecimalLongitude = Coordinates.KalmarMunicipality.Longitude
                }
            };
            observations.Add(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            _fixture.AreaHelper.AddAreaDataToProcessedLocations(observations.Select(o => o.Location));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.Attributes.CountyPartIdByCoordinate.Should().Be((string) SpecialCountyPartId.KalmarFastland);
            observation.Location.County.FeatureId.Should().Be((string) CountyId.Kalmar);
        }

       
        [Fact]
        [Trait("Category", "Integration")]
        public void ProvincePartIdByCoordinateShouldBeSetToLappland_When_ObservationIsInLappmark()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var observation = new Observation
            {
                Location = new Location(LocationType.Point)
                {
                    DecimalLatitude = Coordinates.KirunaMunicipality.Latitude,
                    DecimalLongitude = Coordinates.KirunaMunicipality.Longitude
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            _fixture.AreaHelper.AddAreaDataToProcessedLocation(observation.Location);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.Attributes.ProvincePartIdByCoordinate.Should().Be((string) SpecialProvincePartId.Lappland);
            observation.Location.Province.FeatureId.Should().Be((string) ProvinceIds.TorneLappmark);
        }
    }
}