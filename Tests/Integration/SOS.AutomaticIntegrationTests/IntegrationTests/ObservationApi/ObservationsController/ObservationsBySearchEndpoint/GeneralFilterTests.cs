using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.ObservationsBySearchEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class GeneralFilterTests
    {
        private readonly IntegrationTestFixture _fixture;

        public GeneralFilterTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestProjectIdFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.Projects = new[] { new Lib.Models.Verbatim.Artportalen.Project() { Id = 1 } })
                .TheNext(20)
                    .With(o => o.Projects = new[] { new Lib.Models.Verbatim.Artportalen.Project() { Id = 2 } })
                .TheRest()
                    .With(o => o.Projects = null)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                ProjectIds = new System.Collections.Generic.List<int> { 1 }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestOccurrenceStatusPresentFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.NotPresent = false)
                    .With(o => o.NotRecovered = false)
                .TheNext(20)
                    .With(o => o.NotPresent = true)
                    .With(o => o.NotRecovered = false)
                .TheNext(10)
                    .With(o => o.NotPresent = false)
                    .With(o => o.NotRecovered = true)
                .TheRest()
                    .With(o => o.NotPresent = true)
                    .With(o => o.NotRecovered = true)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestOccurrenceStatusAbsentFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(40)
                    .With(o => o.NotPresent = false)
                    .With(o => o.NotRecovered = false)
                .TheNext(20)
                    .With(o => o.NotPresent = true)
                    .With(o => o.NotRecovered = false)
                .TheNext(20)
                    .With(o => o.NotPresent = false)
                    .With(o => o.NotRecovered = true)
                 .TheNext(20)
                    .With(o => o.NotPresent = true)
                    .With(o => o.NotRecovered = true)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.Absent
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestValidationStatusVerifiedFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(10)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersDocumentation))
                .TheNext(10)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedSpecimenCheckedByValidator))
                .TheNext(10)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording))
                .TheNext(10)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersRarityForm))
                .TheNext(10)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnDeterminatorsVerification))
                 .TheNext(10)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersOldRarityForm))

                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DescriptionRequiredForTheNationalRaritiesCommittee))
                 .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DescriptionRequiredForTheRegionalRecordsCommittee))
                 .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DialogueAtReporterHiddenSighting))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.NotAbleToValidate))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Rejected))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ReportTreatedRegionalForNationalRaritiesCommittee))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Verified))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Unvalidated))
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                VerificationStatus = SearchFilterBaseDto.StatusVerificationDto.Verified
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestValidationStatusNotVerifiedFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(10)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersDocumentation))
                .TheNext(10)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedSpecimenCheckedByValidator))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersRarityForm))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnDeterminatorsVerification))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ApprovedBasedOnReportersOldRarityForm))

                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DescriptionRequiredForTheNationalRaritiesCommittee))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DescriptionRequiredForTheRegionalRecordsCommittee))
                 .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DialogueAtReporterHiddenSighting))
                 .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DialogueWithReporter))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DialogueWithValidator))
 
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.DocumentationRequested))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.NeedNotBeValidated))
                 .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.NotAbleToValidate))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Rejected))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.ReportTreatedRegionalForNationalRaritiesCommittee))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Verified))
                .TheNext(5)
                    .With(o => o.ValidationStatus = new Metadata<int>((int)ValidationStatusId.Unvalidated))
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                VerificationStatus = SearchFilterBaseDto.StatusVerificationDto.NotVerified
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestNotUnsureDeterminationFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.UnsureDetermination = false)
                .TheNext(40)
                    .With(o => o.UnsureDetermination = true)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                DeterminationFilter = SearchFilterBaseDto.SightingDeterminationFilterDto.NotUnsureDetermination
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestOnlyUnsureDeterminationFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.UnsureDetermination = true)
                .TheNext(40)
                    .With(o => o.UnsureDetermination = false)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                DeterminationFilter = SearchFilterBaseDto.SightingDeterminationFilterDto.OnlyUnsureDetermination
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestRecoveredFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.NotRecovered = false)
                .TheNext(40)
                     .With(o => o.NotRecovered = true)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                NotRecoveredFilter = SearchFilterBaseDto.SightingNotRecoveredFilterDto.DontIncludeNotRecovered
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestNotRecoveredFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.NotRecovered = true)
                .TheNext(40)
                    .With(o => o.NotRecovered = false)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                NotRecoveredFilter = SearchFilterBaseDto.SightingNotRecoveredFilterDto.OnlyNotRecovered
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestBirdNestActivityLimitFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .With(o => o.TaxonId = 100102)
                    .With(o => o.Activity = new MetadataWithCategory<int>(1000, 1))
                .TheNext(30)
                      .With(o => o.TaxonId = 102986)
                      .With(o => o.Activity = new MetadataWithCategory<int>(1000, 2))
                .TheNext(20)
                      .With(o => o.TaxonId = 100102)
                      .With(o => o.Activity = new MetadataWithCategory<int>(2000, 1))
                .TheNext(10)
                      .With(o => o.TaxonId = 102986)
                      .With(o => o.Activity = new MetadataWithCategory<int>(2000, 2))
                .TheNext(10)
                     .With(o => o.TaxonId = 100102)
                     .With(o => o.Activity = new MetadataWithCategory<int>(1100, 1))
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                BirdNestActivityLimit = 1000
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }
    }
}