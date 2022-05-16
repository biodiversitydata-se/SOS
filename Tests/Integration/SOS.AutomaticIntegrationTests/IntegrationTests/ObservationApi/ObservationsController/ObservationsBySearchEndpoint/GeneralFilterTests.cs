using System;
using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Enums.VocabularyValues;
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
                    .HaveProjectId(1)
                .TheNext(20)
                    .HaveProjectId(2)
                .TheRest()
                    .HaveProjectId(null)
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
                    .HaveStatus(false, false)
                .TheNext(20)
                     .HaveStatus(true, false)
                .TheNext(10)
                     .HaveStatus(false, true)
                .TheRest()
                    .HaveStatus(true, true)
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
                    .HaveStatus(false, false)
                .TheNext(20)
                     .HaveStatus(true, false)
                .TheNext(20)
                     .HaveStatus(false, true)
                 .TheNext(20)
                     .HaveStatus(true, true)
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
                    .HaveValidationStatus(ValidationStatusId.ApprovedBasedOnReportersDocumentation)
                .TheNext(10)
                      .HaveValidationStatus(ValidationStatusId.ApprovedSpecimenCheckedByValidator)
                .TheNext(10)
                     .HaveValidationStatus(ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording)
                .TheNext(10)
                     .HaveValidationStatus(ValidationStatusId.ApprovedBasedOnReportersRarityForm)
                .TheNext(10)
                     .HaveValidationStatus(ValidationStatusId.ApprovedBasedOnDeterminatorsVerification)
                 .TheNext(10)
                     .HaveValidationStatus(ValidationStatusId.ApprovedBasedOnReportersOldRarityForm)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.DescriptionRequiredForTheNationalRaritiesCommittee)
                 .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.DescriptionRequiredForTheRegionalRecordsCommittee)
                 .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.DialogueAtReporterHiddenSighting)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.NotAbleToValidate)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.Rejected)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.ReportTreatedRegionalForNationalRaritiesCommittee)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.Verified)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.Unvalidated)
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
                    .HaveValidationStatus(ValidationStatusId.ApprovedBasedOnReportersDocumentation)
                .TheNext(10)
                      .HaveValidationStatus(ValidationStatusId.ApprovedSpecimenCheckedByValidator)
                .TheNext(5)
                     .HaveValidationStatus(ValidationStatusId.ApprovedBasedOnImageSoundOrVideoRecording)
                .TheNext(5)
                     .HaveValidationStatus(ValidationStatusId.ApprovedBasedOnReportersRarityForm)
                .TheNext(5)
                     .HaveValidationStatus(ValidationStatusId.ApprovedBasedOnDeterminatorsVerification)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.ApprovedBasedOnReportersOldRarityForm)

                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.DescriptionRequiredForTheNationalRaritiesCommittee)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.DescriptionRequiredForTheRegionalRecordsCommittee)
                 .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.DialogueAtReporterHiddenSighting)
                 .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.DialogueWithReporter)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.DialogueWithValidator)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.DocumentationRequested)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.NeedNotBeValidated)
                 .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.NotAbleToValidate)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.Rejected)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.ReportTreatedRegionalForNationalRaritiesCommittee)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.Verified)
                .TheNext(5)
                    .HaveValidationStatus(ValidationStatusId.Unvalidated)
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
                    .HaveDeterminationStatus(false)
                .TheNext(40)
                      .HaveDeterminationStatus(true)
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
                    .HaveDeterminationStatus(true)
                .TheNext(40)
                      .HaveDeterminationStatus(false)
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
                    .HaveRecoveredStatus(true)
                .TheNext(40)
                      .HaveRecoveredStatus(false)
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
                    .HaveRecoveredStatus(false)
                .TheNext(40)
                      .HaveRecoveredStatus(true)
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
                    .HaveTaxonId(100102)
                    .HaveActivity(1000, 1)
                .TheNext(30)
                      .HaveTaxonId(102986)
                      .HaveActivity(1000, 2)
                .TheNext(20)
                      .HaveTaxonId(100102)
                      .HaveActivity(2000, 1)
                .TheNext(10)
                      .HaveTaxonId(102986)
                      .HaveActivity(2000, 1)
                .TheNext(10)
                     .HaveTaxonId(100102)
                      .HaveActivity(1100, 1)
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