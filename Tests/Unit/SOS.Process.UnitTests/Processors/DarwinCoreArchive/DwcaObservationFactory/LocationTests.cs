﻿using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Process.UnitTests.TestHelpers;
using SOS.TestHelpers.Gis;
using SOS.TestHelpers.Helpers.Builders;
using System.Globalization;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive.DwcaObservationFactory
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class LocationTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        public LocationTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly DwcaObservationFactoryFixture _fixture;


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

        [Fact]
        public void Etrs89_coordinates_are_converted_to_WGS84()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDecimalLatitude(Coordinates.TranasMunicipality.Etrs89Y)
                .WithDecimalLongitude(Coordinates.TranasMunicipality.Etrs89X)
                .WithGeodeticDatum(CoordinateSys.ETRS89_LAEA_Europe.EpsgCode())
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Location.DecimalLatitude.Should().BeApproximately(Coordinates.TranasMunicipality.Latitude, 0.0001);
            result.Location.DecimalLongitude.Should().BeApproximately(Coordinates.TranasMunicipality.Longitude, 0.0001);
            result.Location.GeodeticDatum.Should().Be(CoordinateSys.WGS84.EpsgCode());
            result.Location.VerbatimSRS.Should().Be(CoordinateSys.ETRS89_LAEA_Europe.EpsgCode());
            result.Location.VerbatimLongitude.Should().Be(Coordinates.TranasMunicipality.Etrs89X.ToString(CultureInfo.InvariantCulture));
            result.Location.VerbatimLatitude.Should().Be(Coordinates.TranasMunicipality.Etrs89Y.ToString(CultureInfo.InvariantCulture));
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
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Location.DecimalLatitude.Should().BeApproximately(Coordinates.TranasMunicipality.Latitude, 0.0001);
            result.Location.DecimalLongitude.Should().BeApproximately(Coordinates.TranasMunicipality.Longitude, 0.0001);
            result.Location.GeodeticDatum.Should().Be(CoordinateSys.WGS84.EpsgCode());
            result.Location.VerbatimSRS.Should().Be(CoordinateSys.Rt90_25_gon_v.EpsgCode());
            result.Location.VerbatimLongitude.Should().Be(Coordinates.TranasMunicipality.RT90X.ToString(CultureInfo.InvariantCulture));
            result.Location.VerbatimLatitude.Should().Be(Coordinates.TranasMunicipality.RT90Y.ToString(CultureInfo.InvariantCulture));
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
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Location.DecimalLatitude.Should().BeApproximately(Coordinates.TranasMunicipality.Latitude, 0.0001);
            result.Location.DecimalLongitude.Should().BeApproximately(Coordinates.TranasMunicipality.Longitude, 0.0001);
            result.Location.GeodeticDatum.Should().Be(CoordinateSys.WGS84.EpsgCode());
            result.Location.VerbatimSRS.Should().Be(CoordinateSys.SWEREF99_TM.EpsgCode());
            result.Location.VerbatimLongitude.Should().Be(Coordinates.TranasMunicipality.Sweref99TmX.ToString(CultureInfo.InvariantCulture));
            result.Location.VerbatimLatitude.Should().Be(Coordinates.TranasMunicipality.Sweref99TmY.ToString(CultureInfo.InvariantCulture));
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
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Location.DecimalLatitude.Should().BeApproximately(Coordinates.TranasMunicipality.Latitude, 0.0001);
            result.Location.DecimalLongitude.Should().BeApproximately(Coordinates.TranasMunicipality.Longitude, 0.0001);
            result.Location.GeodeticDatum.Should().Be(CoordinateSys.WGS84.EpsgCode());
            result.Location.VerbatimSRS.Should().Be(CoordinateSys.WebMercator.EpsgCode());
            result.Location.VerbatimLongitude.Should().Be(Coordinates.TranasMunicipality.WebMercatorX.ToString(CultureInfo.InvariantCulture));
            result.Location.VerbatimLatitude.Should().Be(Coordinates.TranasMunicipality.WebMercatorY.ToString(CultureInfo.InvariantCulture));
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

        [Theory]
        [InlineData("5", 5)]
        [InlineData(null, 5000)]
        [InlineData("4.699999809265137", 5)]
        [InlineData("11.28600025177002", 11)]
        public void Succeeds_to_parse_coordinateUncertaintyInMeters(
            string input,
            int? expectedValue)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithCoordinateUncertaintyInMeters(input)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observation = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Location.CoordinateUncertaintyInMeters.Should().Be(expectedValue);
        }

        [Fact]
        public void Parse_footprintWKT()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder                
                .WithGeodeticDatum("WGS 84")   
                .WithCoordinateUncertaintyInMeters(2000)
                .Build();
            //dwcaObservation.FootprintWKT = "POLYGON((17.5289 59.7905, 17.5289 59.9258, 17.7683 59.9258, 17.7683 59.7905, 17.5289 59.7905))";
            dwcaObservation.FootprintWKT = "LINESTRING(17.6444 59.8586, 17.6310 59.8580, 17.6285 59.8557)";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Location.DecimalLatitude.Should().BeApproximately(59.85648, 0.0001);
            result.Location.DecimalLongitude.Should().BeApproximately(17.63736, 0.0001);
            result.Location.GeodeticDatum.Should().Be("EPSG:4326");
            result.Location.FootprintWKT.Should().Be("LINESTRING(17.6444 59.8586, 17.6310 59.8580, 17.6285 59.8557)");
        }
    }
}