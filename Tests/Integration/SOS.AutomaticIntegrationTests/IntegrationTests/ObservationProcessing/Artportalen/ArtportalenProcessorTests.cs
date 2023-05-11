using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using System.Linq;
using LinqStatistics;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using System.ComponentModel;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationProcessing.Artportalen
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class ArtportalenProcessorTests
    {
        private readonly IntegrationTestFixture _fixture;

        public ArtportalenProcessorTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        [Description("Observation is positive (present) when both NotPresent and NotRecovered is false")]
        public void Observation_is_positive_when_both_NotPresent_and_NotRecovered_is_false()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                    //.HaveRandomValues()
                .TheFirst(60)
                    .With(v => v.NotPresent = false)
                    .With(v => v.NotRecovered = false)
                .TheNext(20)
                    .With(v => v.NotPresent = true)
                .TheNext(20)
                    .With(v => v.NotRecovered = true)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var processedObservations = _fixture.ProcessObservations(verbatimObservations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            processedObservations
                .Select(m => m.Occurrence.IsPositiveObservation)
                .CountEach()
                .OrderByDescending(m => m.Count)
                .ToList()
                .Should().BeEquivalentTo(
                    new List<ItemCount<bool>> {
                        new ItemCount<bool>(true, 60),
                        new ItemCount<bool>(false, 40) });
        }
    }
}