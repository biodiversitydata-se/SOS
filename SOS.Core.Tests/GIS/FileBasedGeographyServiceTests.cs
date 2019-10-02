using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SOS.Core.GIS;
using SOS.Core.Models.Observations;
using Xunit;

namespace SOS.Core.Tests.GIS
{
    public class FileBasedGeographyServiceTests
    {
        [Fact]
        public void TestPointInTranasMunicipality()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            IPoint pointInTranasMunicipality = new Point(1668674.71421154, 7970551.18856367);
            ProcessedDwcObservation observation = new ProcessedDwcObservation
            {
                CoordinateX_WebMercator = pointInTranasMunicipality.X,
                CoordinateY_WebMercator = pointInTranasMunicipality.Y
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var isObservationInSweden = FileBasedGeographyService.IsObservationInSweden(observation);
            FileBasedGeographyService.CalculateRegionBelongings(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            isObservationInSweden.Should().BeTrue();
            observation.DebugMunicipalityNameByCoordinate.Should().Be("Tranås");
            observation.DebugCountyNameByCoordinate.Should().Be("Jönköping");
            observation.DebugProvinceNameByCoordinate.Should().Be("Småland");
            observation.DebugCountryPartNameByCoordinate.Should().Be("Götaland");
        }
    }
}
