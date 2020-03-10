﻿using System.Collections.Generic;
using FluentAssertions;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Helpers;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;
using SOS.Process.UnitTests.TestHelpers;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Gis;
using Xunit;

namespace SOS.Process.UnitTests.Helpers
{
    public class AreaHelperTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void ProvincePartIdByCoordinateShouldBeSetToLappland_When_ObservationIsInLappmark()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var areaTypes = new[] {AreaType.County, AreaType.Province};
            var areaHelper = new AreaHelper(
                AreaVerbatimRepositoryStubFactory.Create(areaTypes) .Object,
                ProcessedFieldMappingRepositoryStubFactory.Create().Object);
            var observations = new List<ProcessedSighting>();
            var observation = new ProcessedSighting(DataProvider.SpeciesPortal)
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
            areaHelper.AddAreaDataToProcessedSightings(observations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.ProvincePartIdByCoordinate.Should().Be((int)ProvinceFeatureId.Lappland);
            observation.Location.ProvinceId.Id.Should().Be((int)ProvinceFeatureId.AseleLappmark);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CountyPartIdByCoordinateShouldBeSetToOland_When_ObservationIsOnOland()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var areaTypes = new[] { AreaType.County, AreaType.Province };
            var areaHelper = new AreaHelper(
                AreaVerbatimRepositoryStubFactory.Create(areaTypes).Object,
                ProcessedFieldMappingRepositoryStubFactory.Create().Object);
            var observations = new List<ProcessedSighting>();
            var observation = new ProcessedSighting(DataProvider.SpeciesPortal)
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
            areaHelper.AddAreaDataToProcessedSightings(observations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.CountyPartIdByCoordinate.Should().Be((int)CountyFeatureId.Oland);
            observation.Location.CountyId.Id.Should().Be((int)CountyFeatureId.Kalmar);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CountyPartIdShouldBeSetToKalmarFastland_When_ObservationIsInKalmar()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var areaTypes = new[] { AreaType.County, AreaType.Province };
            var areaHelper = new AreaHelper(
                AreaVerbatimRepositoryStubFactory.Create(areaTypes).Object,
                ProcessedFieldMappingRepositoryStubFactory.Create().Object);
            var observations = new List<ProcessedSighting>();
            var observation = new ProcessedSighting(DataProvider.SpeciesPortal)
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
            areaHelper.AddAreaDataToProcessedSightings(observations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.CountyPartIdByCoordinate.Should().Be((int)CountyFeatureId.KalmarFastland);
            observation.Location.CountyId.Id.Should().Be((int)CountyFeatureId.Kalmar);
        }
    }
}
