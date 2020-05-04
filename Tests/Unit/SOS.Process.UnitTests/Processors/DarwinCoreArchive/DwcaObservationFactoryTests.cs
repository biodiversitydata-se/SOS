using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.UnitTests.TestHelpers;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Helpers.Builders;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive
{
    public class DwcaObservationFactoryTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        private readonly DwcaObservationFactoryFixture _fixture;

        public DwcaObservationFactoryTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }
    
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
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Location.DecimalLatitude.Should().BeApproximately(58.01540, 0.0001);
            result.Location.DecimalLongitude.Should().BeApproximately(14.98998, 0.0001);
            result.Location.GeodeticDatum.Should().Be("EPSG:4326");
        }

        
    }
}