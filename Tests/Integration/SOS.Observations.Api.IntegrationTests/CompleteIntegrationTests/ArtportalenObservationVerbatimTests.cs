using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Verbatim.Artportalen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.CompleteIntegrationTests
{
    public class ArtportalenObservationVerbatimTests
    {
        [Fact]
        public async Task CreateArtportalenTestData()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var verbatims = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveRandomValidValues()
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            verbatims.Count.Should().Be(100);
        }

        [Fact]
        public async Task CreateArtportalenTestDataFromPredefinedObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var predefinedObservations = ArtportalenObservationVerbatimBuilderExtensions.VerbatimFromJsonNewtonsoft;
            var verbatims = Builder<ArtportalenObservationVerbatimWrapper>.CreateListOfSize(100)
                .All()
                    .With(m => m.Observation = Pick<ArtportalenObservationVerbatim>.RandomItemFrom(predefinedObservations))
                .TheFirst(1)
                    .With(m => m.Observation.SightingId = 123456)                
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            verbatims.Count.Should().Be(100);
        }

        public class ArtportalenObservationVerbatimWrapper
        {
            public ArtportalenObservationVerbatim Observation { get; set; }
        }
    }
}
