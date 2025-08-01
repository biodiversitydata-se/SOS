﻿using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;
using SOS.TestHelpers.Gis;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SOS.Process.LiveIntegrationTests.Helpers
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
                    DecimalLatitude = TestCoordinates.BorgholmMunicipality.Latitude,
                    DecimalLongitude = TestCoordinates.BorgholmMunicipality.Longitude
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
            observation.Location.Attributes.CountyPartIdByCoordinate.Should().Be(SpecialCountyPartId.Öland);
            observation.Location.County.FeatureId.Should().Be(CountyId.Kalmar);
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
                    DecimalLatitude = TestCoordinates.KalmarMunicipality.Latitude,
                    DecimalLongitude = TestCoordinates.KalmarMunicipality.Longitude
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
            observation.Location.Attributes.CountyPartIdByCoordinate.Should().Be(SpecialCountyPartId.KalmarFastland);
            observation.Location.County.FeatureId.Should().Be(CountyId.Kalmar);
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
                    DecimalLatitude = TestCoordinates.KirunaMunicipality.Latitude,
                    DecimalLongitude = TestCoordinates.KirunaMunicipality.Longitude
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            _fixture.AreaHelper.AddAreaDataToProcessedLocation(observation.Location);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.Attributes.ProvincePartIdByCoordinate.Should().Be(SpecialProvincePartId.Lappland);
            observation.Location.Province.FeatureId.Should().Be(ProvinceIds.TorneLappmark);
        }
    }
}