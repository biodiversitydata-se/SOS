using System.Collections.Generic;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using SOS.TestHelpers.Gis;
using SOS.TestHelpers.Helpers;
using Xunit;

namespace SOS.Process.IntegrationTests.Helpers
{
    public class AreaHelperIntegrationTests : IClassFixture<AreaHelperFixture>
    {
        private readonly AreaHelperFixture _fixture;

        public AreaHelperIntegrationTests(AreaHelperFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetRegionBelongingsForLocationInTranasMunicipality()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var observations = new List<ProcessedObservation>();
            var observation = new ProcessedObservation(DataProvider.Artportalen)
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
            _fixture.AreaHelper.AddAreaDataToProcessedObservations(observations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            _fixture.AreaHelper.AddValueDataToGeographicalFields(observation);
            observation.Location.CountyId.Value.Should().Be("Jönköping");
            observation.Location.MunicipalityId.Value.Should().Be("Tranås");
            observation.Location.ProvinceId.Value.Should().Be("Småland");
            observation.Location.ParishId.Value.Should().Be("Tranås");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void ProvincePartIdByCoordinateShouldBeSetToLappland_When_ObservationIsInLappmark()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var observation = new ProcessedObservation(DataProvider.Artportalen)
            {
                Location = new ProcessedLocation
                {
                    DecimalLatitude = Coordinates.KirunaMunicipality.Latitude,
                    DecimalLongitude = Coordinates.KirunaMunicipality.Longitude
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            _fixture.AreaHelper.AddAreaDataToProcessedObservation(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.ProvincePartIdByCoordinate.Should().Be((int)SpecialProvincePartId.Lappland);
            observation.Location.ProvinceId.Id.Should().Be((int)ProvinceId.TorneLappmark);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CountyPartIdByCoordinateShouldBeSetToOland_When_ObservationIsOnOland()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var observations = new List<ProcessedObservation>();
            var observation = new ProcessedObservation(DataProvider.Artportalen)
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
            _fixture.AreaHelper.AddAreaDataToProcessedObservations(observations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.CountyPartIdByCoordinate.Should().Be((int)SpecialCountyPartId.Oland);
            observation.Location.CountyId.Id.Should().Be((int)CountyId.Kalmar);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CountyPartIdShouldBeSetToKalmarFastland_When_ObservationIsInKalmar()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var observations = new List<ProcessedObservation>();
            var observation = new ProcessedObservation(DataProvider.Artportalen)
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
            _fixture.AreaHelper.AddAreaDataToProcessedObservations(observations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.CountyPartIdByCoordinate.Should().Be((int)SpecialCountyPartId.KalmarFastland);
            observation.Location.CountyId.Id.Should().Be((int)CountyId.Kalmar);
        }
    }
}