using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.UnitTests.TestHelpers;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Gis;
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
            result.Location.GeodeticDatum.Should().Be(CoordinateSys.WGS84.EpsgCode());
        }

        [Fact]
        public void Wgs84_coordinates_with_GeodeticDatum_wgs84_is_parsed_to_double_data_type()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDecimalLatitude("58.01540")
                .WithDecimalLongitude("14.98998")
                .WithGeodeticDatum("WGS 84")
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


        [Fact]
        public void Assume_Wgs84_coordinate_system_when_GeodeticDatum_is_omitted()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDecimalLatitude("58.01540")
                .WithDecimalLongitude("14.98998")
                .WithGeodeticDatum(null)
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

        [Fact]
        public void SWEREF99TM_coordinates_are_converted_to_WGS84()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder 
                .WithDecimalLatitude(Coordinates.TranasMunicipality.Sweref99TmY)
                .WithDecimalLongitude(Coordinates.TranasMunicipality.Sweref99TmX)
                .WithGeodeticDatum(CoordinateSys.SWEREF99_TM.EpsgCode())
                .WithCoordinateUncertaintyInMeters(100)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Location.DecimalLatitude.Should().BeApproximately(Coordinates.TranasMunicipality.Latitude, 0.0001);
            result.Location.DecimalLongitude.Should().BeApproximately(Coordinates.TranasMunicipality.Longitude, 0.0001);
            result.Location.GeodeticDatum.Should().Be(CoordinateSys.WGS84.EpsgCode());
            result.Location.VerbatimSRS.Should().Be(CoordinateSys.SWEREF99_TM.EpsgCode());
            result.Location.VerbatimLongitude.Should().BeApproximately(Coordinates.TranasMunicipality.Sweref99TmX, 0.0001);
            result.Location.VerbatimLatitude.Should().BeApproximately(Coordinates.TranasMunicipality.Sweref99TmY, 0.0001);
        }

        [Fact]
        public void RT90_coordinates_are_converted_to_WGS84()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDecimalLatitude(Coordinates.TranasMunicipality.RT90Y)
                .WithDecimalLongitude(Coordinates.TranasMunicipality.RT90X)
                .WithGeodeticDatum(CoordinateSys.Rt90_25_gon_v.EpsgCode())
                .WithCoordinateUncertaintyInMeters(100)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Location.DecimalLatitude.Should().BeApproximately(Coordinates.TranasMunicipality.Latitude, 0.0001);
            result.Location.DecimalLongitude.Should().BeApproximately(Coordinates.TranasMunicipality.Longitude, 0.0001);
            result.Location.GeodeticDatum.Should().Be(CoordinateSys.WGS84.EpsgCode());
            result.Location.VerbatimSRS.Should().Be(CoordinateSys.Rt90_25_gon_v.EpsgCode());
            result.Location.VerbatimLongitude.Should().BeApproximately(Coordinates.TranasMunicipality.RT90X, 0.0001);
            result.Location.VerbatimLatitude.Should().BeApproximately(Coordinates.TranasMunicipality.RT90Y, 0.0001);
        }

        [Fact]
        public void WebMercator_coordinates_are_converted_to_WGS84()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDecimalLatitude(Coordinates.TranasMunicipality.WebMercatorY)
                .WithDecimalLongitude(Coordinates.TranasMunicipality.WebMercatorX)
                .WithGeodeticDatum(CoordinateSys.WebMercator.EpsgCode())
                .WithCoordinateUncertaintyInMeters(100)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Location.DecimalLatitude.Should().BeApproximately(Coordinates.TranasMunicipality.Latitude, 0.0001);
            result.Location.DecimalLongitude.Should().BeApproximately(Coordinates.TranasMunicipality.Longitude, 0.0001);
            result.Location.GeodeticDatum.Should().Be(CoordinateSys.WGS84.EpsgCode());
            result.Location.VerbatimSRS.Should().Be(CoordinateSys.WebMercator.EpsgCode());
            result.Location.VerbatimLongitude.Should().BeApproximately(Coordinates.TranasMunicipality.WebMercatorX, 0.0001);
            result.Location.VerbatimLatitude.Should().BeApproximately(Coordinates.TranasMunicipality.WebMercatorY, 0.0001);
        }
    }
}