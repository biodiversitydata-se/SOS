using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using System.Linq;
using LinqStatistics;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using System.ComponentModel;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationProcessing.DarwinCore
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class DarwinCoreProcessorTests
    {
        private readonly IntegrationTestFixture _fixture;

        public DarwinCoreProcessorTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        [Description("Observation is positive (present) when OccurrenceStatus doesn't have the value 'absent'")]
        public void Observation_is_positive_when_OccurrenceStatus_is_not_null()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<DwcObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(20)
                    .With(v => v.OccurrenceStatus = "present")
                .TheNext(20)
                    .With(v => v.OccurrenceStatus = "")
                .TheNext(20)
                    .With(v => v.OccurrenceStatus = null)
                .TheNext(40)
                    .With(v => v.OccurrenceStatus = "absent")
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
                    new List<ItemCount<bool?>> {
                        new ItemCount<bool?>(true, 60),
                        new ItemCount<bool?>(false, 40) });
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        [Description("Sensitive species sightings should be protected")]
        public void Sensitive_species_sightings_should_be_protected()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<DwcObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .HaveSensitiveSpeciesTaxonId()
                    .With(m => m.AccessRights = null)
                .TheNext(20)
                    .HaveSensitiveSpeciesTaxonId()
                    .With(m => m.AccessRights = "Free usage") // Sensitive species with "Free usage" => Sensitive=false                
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var processedObservations = _fixture.ProcessObservations(verbatimObservations);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            processedObservations
                .Select(m => m.Sensitive)
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