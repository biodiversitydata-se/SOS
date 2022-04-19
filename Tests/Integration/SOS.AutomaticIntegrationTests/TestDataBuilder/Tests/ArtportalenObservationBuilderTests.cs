using FizzWare.NBuilder;
using FluentAssertions;
using LinqStatistics;
using SOS.Lib.Models.Verbatim.Artportalen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SOS.AutomaticIntegrationTests.TestDataBuilder.Tests
{
    public class ArtportalenObservationBuilderTests
    {
        [Fact]
        [Trait("Category", "UnitTest")]
        public void CreateArtportalenTestDataWithAllRandomValues()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------           
            const int NrObservations = 100;
            var verbatims = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(NrObservations)
                .All()                    
                    .HaveRandomValues()
                .TheFirst(1)
                    .With(m => m.SightingId = 123456)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            verbatims.Count.Should().Be(NrObservations);
            verbatims.First().SightingId.Should().Be(123456);
            verbatims.Select(m => m.SightingId).Should().OnlyHaveUniqueItems("because all SightingIds should be unique");
        }

        [Fact]
        [Trait("Category", "UnitTest")]
        public void CreateArtportalenTestDataWithValuesFromPredefinedObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const int NrObservations = 10;
            var verbatims = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(NrObservations)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(1)
                    .With(m => m.SightingId = 123456)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            verbatims.Count.Should().Be(NrObservations);
            verbatims.First().SightingId.Should().Be(123456);
            verbatims.Select(m => m.SightingId).Should().OnlyHaveUniqueItems("because all SightingIds should be unique");
        }
    }
}
