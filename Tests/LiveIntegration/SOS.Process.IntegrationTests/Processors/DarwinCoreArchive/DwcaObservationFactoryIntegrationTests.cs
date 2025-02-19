﻿using FluentAssertions;
using SOS.Process.LiveIntegrationTests.TestHelpers;
using SOS.TestHelpers.Helpers.Builders;
using Xunit;

namespace SOS.Process.LiveIntegrationTests.Processors.DarwinCoreArchive
{
    public class DwcaObservationFactoryIntegrationTests : IClassFixture<DwcaObservationFactoryIntegrationFixture>
    {
        public DwcaObservationFactoryIntegrationTests(DwcaObservationFactoryIntegrationFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly DwcaObservationFactoryIntegrationFixture _fixture;

        [Fact]
        public void Wgs84_coordinates_is_parsed_to_double_data_type()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDecimalLatitude("58.01540")
                .WithDecimalLongitude("14.98998")
                .WithGeodeticDatum("EPSG:4326")
                .WithCoordinateUncertaintyInMeters(100)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Location.DecimalLatitude.Should().BeApproximately(58.01540, 0.0001);
            result.Location.DecimalLongitude.Should().BeApproximately(14.98998, 0.0001);
            result.Location.GeodeticDatum.Should().Be("EPSG:4326");
        }
    }
}