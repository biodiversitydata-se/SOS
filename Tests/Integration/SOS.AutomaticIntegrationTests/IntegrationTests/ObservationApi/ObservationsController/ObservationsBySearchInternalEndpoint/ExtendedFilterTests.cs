using System;
using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.ObservationsBySearchInternalEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class ExtendedFilterTests
    {
        private readonly IntegrationTestFixture _fixture;
        private static Bogus.Faker _faker = new Bogus.Faker();

        public ExtendedFilterTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task GetObservationsWithOccurrenceRemarks()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()                    
                .TheFirst(60)
                    .With(o => o.Comment = _faker.Random.String(_faker.Random.Number(1, 1024)))
                .TheNext(40)
                    .With(o => o.Comment = null)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    OnlyWithNotes = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithActivityId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .With(o => o.Activity = new Lib.Models.Shared.MetadataWithCategory<int>(1, 2))
                .TheNext(30)
                    .With(o => o.Activity = new Lib.Models.Shared.MetadataWithCategory<int>(2, 1))
                .TheNext(20)
                    .With(o => o.Activity = new Lib.Models.Shared.MetadataWithCategory<int>(3, 1))
                .TheNext(20)
                    .With(o => o.Activity = null)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    ActivityIds = new[] {1,2}
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithBiotopeId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.Biotope = new Lib.Models.Shared.MetadataWithCategory<int>(1, 2))
                .TheNext(20)
                    .With(o => o.Biotope = new Lib.Models.Shared.MetadataWithCategory<int>(2, 1))
                .TheNext(20)
                    .With(o => o.Biotope = null)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    BiotopeId = 1
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithChecklistId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.ChecklistId = 1)
                .TheNext(20)
                    .With(o => o.ChecklistId = 2)
                .TheNext(20)
                    .With(o => o.ChecklistId = null)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    ChecklistId = 1
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithDatasourceId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.DatasourceId = 1)
                .TheNext(20)
                    .With(o => o.DatasourceId = 2)
                .TheNext(20)
                    .With(o => o.DatasourceId = null)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    DatasourceIds = new [] {1}
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithDiscoveryMethod()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.DiscoveryMethod = new Lib.Models.Shared.Metadata<int>(1))
                .TheNext(20)
                    .With(o => o.DiscoveryMethod = new Lib.Models.Shared.Metadata<int>(2))
                .TheNext(20)
                    .With(o => o.DiscoveryMethod = null)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    DiscoveryMethodIds = new[] { 1 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithExcludeValidationStatusIds()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(20)
                    .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(10))
                .TheNext(20)
                    .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(20))
                .TheNext(30)
                    .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(60))
                .TheNext(30)
                    .With(o => o.ValidationStatus = null)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    ExcludeVerificationStatusIds = new[] { 10, 20 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithExcludeVerificationStatusIds()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(20)
                   .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(10))
               .TheNext(20)
                   .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(20))
               .TheNext(30)
                   .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(60))
               .TheNext(30)
                   .With(o => o.ValidationStatus = null)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    ExcludeVerificationStatusIds = new[] { 10, 20 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithHasTriggerdValidationRule()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                   .With(o => o.HasTriggeredValidationRules = true)
               .TheNext(40)
                   .With(o => o.HasTriggeredValidationRules = false)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    HasTriggeredVerificationRule = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithHasTriggeredVerificationRule()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                   .With(o => o.HasTriggeredValidationRules = true)
               .TheNext(40)
                   .With(o => o.HasTriggeredValidationRules = false)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    HasTriggeredVerificationRule = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithHasTriggerdValidationRuleWithWarning()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                   .With(o => o.HasAnyTriggeredValidationRuleWithWarning = true)
               .TheNext(40)
                   .With(o => o.HasAnyTriggeredValidationRuleWithWarning = false)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    HasTriggeredVerificationRuleWithWarning = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithHasTriggeredVerificationRuleWithWarning()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                   .With(o => o.HasAnyTriggeredValidationRuleWithWarning = true)
               .TheNext(40)
                   .With(o => o.HasAnyTriggeredValidationRuleWithWarning = false)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    HasTriggeredVerificationRuleWithWarning = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithInstitutionId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                   .With(o => o.OwnerOrganization = new Lib.Models.Shared.Metadata<int>(1))
               .TheNext(20)
                   .With(o => o.OwnerOrganization = new Lib.Models.Shared.Metadata<int>(2))
                .TheLast(20)
                   .With(o => o.OwnerOrganization = null)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    InstitutionId = "urn:lsid:artdata.slu.se:organization:1"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithLengthEq()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                   .With(o => o.Length = 10)
               .TheNext(20)
                   .With(o => o.Length = 20)
                .TheLast(20)
                  .With(o => o.Length = null)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Length = 10,
                    LengthOperator = "eq"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithLengthGte()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                   .With(o => o.Length = 50)
                .TheNext(30)
                   .With(o => o.Length = 100)
               .TheNext(20)
                   .With(o => o.Length = 49)
                .TheLast(20)
                  .With(o => o.Length = null)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Length = 50,
                    LengthOperator = "gte"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithLengthLte()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                   .With(o => o.Length = 50)
                .TheNext(30)
                   .With(o => o.Length = 20)
               .TheNext(20)
                   .With(o => o.Length = 51)
                .TheLast(20)
                  .With(o => o.Length = null)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Length = 50,
                    LengthOperator = "lte"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithLifeStageIds()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                   .With(o => o.Stage = new Lib.Models.Shared.Metadata<int>(5))
                .TheLast(40)
                    .With(o => o.Stage = new Lib.Models.Shared.Metadata<int>(0))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    LifeStageIds = new[] {5}
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithLocationNameFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                   .With(o => o.Site.Name = "Test")
                .TheNext(30)
                    .With(o => o.Site.Name = "test location")
                .TheLast(40)
                    .With(o => o.Site.Name = "location test")
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                Geographics = new GeographicsFilterDto
                {
                    LocationNameFilter = "Test*"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithMonthsStartDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                    .With(o => o.StartDate = new DateTime(2000,1,30))
                    .With(o => o.EndDate = new DateTime(2000, 2, 1))
                .TheNext(30)
                    .With(o => o.StartDate = new DateTime(2000, 12, 1))
                    .With(o => o.EndDate = new DateTime(2000, 12, 1))
                .TheLast(40)
                    .With(o => o.StartDate = new DateTime(2000, 3, 1))
                    .With(o => o.EndDate = new DateTime(2000, 3, 1))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Months = new [] {1, 12},
                    MonthsComparison = ExtendedFilterDto.DateFilterComparisonDto.StartDate
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithMonthsEndDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                    .With(o => o.StartDate = new DateTime(2000, 1, 30))
                    .With(o => o.EndDate = new DateTime(2000, 1, 30))
                .TheNext(30)
                    .With(o => o.StartDate = new DateTime(2000, 11, 30))
                    .With(o => o.EndDate = new DateTime(2000, 12, 1))
                .TheLast(40)
                    .With(o => o.StartDate = new DateTime(2000, 3, 1))
                    .With(o => o.EndDate = new DateTime(2000, 3, 1))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Months = new[] { 1, 12 },
                    MonthsComparison = ExtendedFilterDto.DateFilterComparisonDto.EndDate
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithMonthsBothStartDateAndEndDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                    .With(o => o.StartDate = new DateTime(2000, 1, 1))
                    .With(o => o.EndDate = new DateTime(2000, 1, 1))
                .TheNext(30)
                    .With(o => o.StartDate = new DateTime(2000, 12, 1))
                    .With(o => o.EndDate = new DateTime(2000, 12, 1))
                .TheNext(20)
                    .With(o => o.StartDate = new DateTime(2000, 1, 30))
                    .With(o => o.EndDate = new DateTime(2000, 2, 1))
                .TheLast(20)
                    .With(o => o.StartDate = new DateTime(2000, 3, 1))
                    .With(o => o.EndDate = new DateTime(2000, 3, 1))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Months = new[] { 1, 12 },
                    MonthsComparison = ExtendedFilterDto.DateFilterComparisonDto.BothStartDateAndEndDate
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithStartDateEndDateMonthRange()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(20)
                    .With(o => o.StartDate = new DateTime(2000, 12, 30))
                    .With(o => o.EndDate = new DateTime(2001, 1, 1))
                .TheNext(20)
                    .With(o => o.StartDate = new DateTime(2000, 6, 1))
                    .With(o => o.EndDate = new DateTime(2000, 6, 1))
                .TheNext(20)
                    .With(o => o.StartDate = new DateTime(2000, 1, 24))
                    .With(o => o.EndDate = new DateTime(2000, 1, 25))
                .TheNext(20)
                    .With(o => o.StartDate = new DateTime(2000, 12, 30))
                    .With(o => o.EndDate = new DateTime(2000, 12, 31))
                .TheLast(20)
                    .With(o => o.StartDate = new DateTime(2000, 3, 1))
                    .With(o => o.EndDate = new DateTime(2000, 3, 1))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Months = new[] { 1, 6 },
                    MonthsComparison = ExtendedFilterDto.DateFilterComparisonDto.StartDateEndDateMonthRange
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithDontIncludeNotPresent()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                    .With(o => o.NotPresent = false)
                .TheNext(40)
                    .With(o => o.NotPresent = true)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                   NotPresentFilter = ExtendedFilterDto.SightingNotPresentFilterDto.DontIncludeNotPresent
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithOnlyNotPresent()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                    .With(o => o.NotPresent = true)
                .TheNext(40)
                    .With(o => o.NotPresent = false)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    NotPresentFilter = ExtendedFilterDto.SightingNotPresentFilterDto.OnlyNotPresent
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithObservedByUserId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                    .With(o => o.ObserversInternal = new[] { new Lib.Models.Shared.UserInternal { Id = 1 } })
                .TheNext(40)
                    .With(o => o.ObserversInternal = new[] { new Lib.Models.Shared.UserInternal { Id = 2 } })
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    ObservedByUserId = 1
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithObservedByUserServiceUserId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                    .With(o => o.ObserversInternal = new[] { new Lib.Models.Shared.UserInternal { UserServiceUserId = 1 } })
                .TheNext(40)
                    .With(o => o.ObserversInternal = new[] { new Lib.Models.Shared.UserInternal { UserServiceUserId = 2 } })
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    ObservedByUserServiceUserId = 1
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithOnlySecondHandInformation()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(20) // true
                    .With(o => o.Observers = "Via Other User")
                    .With(o => o.ObserversInternal = new [] { new Lib.Models.Shared.UserInternal{ Id = 1 }  } )
                    .With(o => o.ReportedByUserId = 1 )
                .TheNext(20) // true
                    .With(o => o.Observers = "Via Other User")
                    .With(o => o.ObserversInternal = new Lib.Models.Shared.UserInternal[] { })
                    .With(o => o.ReportedByUserId = 1)
                .TheNext(20) // true
                    .With(o => o.Observers = "Via Other User")
                    .With(o => o.ObserversInternal = null)
                    .With(o => o.ReportedByUserId = 1)
                .TheNext(20) // false - different reporter and observer
                    .With(o => o.Observers = "Via Other User")
                    .With(o => o.ObserversInternal = new[] { new Lib.Models.Shared.UserInternal { Id = 2 } })
                    .With(o => o.ReportedByUserId = 1)                
                .TheNext(20) // false - no Via prefix
                    .With(o => o.Observers = "Some User")
                    .With(o => o.ObserversInternal = new[] { new Lib.Models.Shared.UserInternal { Id = 1 } })
                    .With(o => o.ReportedByUserId = 1)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    OnlySecondHandInformation = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithOnlyWithBarcode()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                    .With(o => o.SightingBarcodeURL = "Barcodeurl")
                .TheNext(40)
                     .With(o => o.SightingBarcodeURL = null)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    OnlyWithBarcode = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithOnlyWithMedia()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                    .With(o => o.HasImages = true)
                    .With(o => o.FirstImageId = 1)
                .TheNext(40)
                     .With(o => o.HasImages = false)
                    .With(o => o.FirstImageId = 0)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    OnlyWithMedia = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithOnlyWithNotes()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                    .With(o => o.Comment = "My comment")
                .TheNext(40)
                     .With(o => o.Comment = null)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    OnlyWithNotes = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithOnlyWithNotesOfInterest()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                    .With(o => o.NoteOfInterest = true)
                .TheNext(40)
                     .With(o => o.NoteOfInterest = false)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    OnlyWithNotesOfInterest = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithOnlyWithUserComments()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                    .With(o => o.HasUserComments = true)
                .TheNext(40)
                     .With(o => o.HasUserComments = false)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    OnlyWithUserComments = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithTriggeredObservationRuleFrequencyIds()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                    .With(o => o.TriggeredObservationRuleFrequencyId = 1)
                .TheNext(30)
                     .With(o => o.TriggeredObservationRuleFrequencyId = 2)
                .TheNext(20)
                     .With(o => o.TriggeredObservationRuleFrequencyId = 3)
                .TheNext(20)
                     .With(o => o.TriggeredObservationRuleFrequencyId = null)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    TriggeredObservationRuleFrequencyIds = new [] {1,2}
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithTriggeredObservationRuleReproductionIds()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                    .With(o => o.TriggeredObservationRuleReproductionId = 1)
                .TheNext(30)
                     .With(o => o.TriggeredObservationRuleReproductionId = 2)
                .TheNext(20)
                     .With(o => o.TriggeredObservationRuleReproductionId = 3)
                .TheNext(20)
                     .With(o => o.TriggeredObservationRuleReproductionId = null)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    TriggeredObservationRuleReproductionIds = new[] { 1, 2 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithTypeFilterDoNotShowMerged()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.Aggregated)
                .TheNext(30)
                     .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.Ordinary)
                .TheNext(40)
                    .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.Assessment)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    TypeFilter = ExtendedFilterDto.SightingTypeFilterDto.DoNotShowMerged
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithTypeFilterDoNotShowSightingsInMerged()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.Assessment)
                .TheNext(30)
                     .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.Ordinary)
                .TheNext(40)
                    .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.AssessmentChild)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    TypeFilter = ExtendedFilterDto.SightingTypeFilterDto.DoNotShowSightingsInMerged
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithTypeFilterShowBoth()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.Assessment)
                .TheNext(30)
                     .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.Ordinary)
                .TheNext(40)
                    .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.Unknown)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    TypeFilter = ExtendedFilterDto.SightingTypeFilterDto.ShowBoth
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithTypeFilterShowOnlyMerged()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.Assessment)
                .TheNext(20)
                     .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.Ordinary)
                .TheNext(20)
                    .With(o => o.SightingTypeSearchGroupId = (int)Lib.Enums.Artportalen.SightingTypeSearchGroup.OwnBreedingAssessment)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    TypeFilter = ExtendedFilterDto.SightingTypeFilterDto.ShowOnlyMerged
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithUnspontaneousFilterNotUnspontaneous()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.Unspontaneous = false)
                .TheNext(40)
                     .With(o => o.Unspontaneous = true)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                   UnspontaneousFilter = ExtendedFilterDto.SightingUnspontaneousFilterDto.NotUnspontaneous
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithUnspontaneousFilterUnspontaneous()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.Unspontaneous = true)
                .TheNext(40)
                     .With(o => o.Unspontaneous = false)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    UnspontaneousFilter = ExtendedFilterDto.SightingUnspontaneousFilterDto.Unspontaneous, 
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithUsePeriodForAllYearsBetweenStartDateAndEndDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .With(o => o.StartDate = new DateTime(2000, 1, 2))
                    .With(o => o.EndDate = new DateTime(2000, 1, 2))
                .TheNext(30)
                    .With(o => o.StartDate = new DateTime(2001, 1, 15))
                    .With(o => o.EndDate = new DateTime(2001, 1, 15))
                .TheNext(40)
                    .With(o => o.StartDate = new DateTime(2000, 2, 2))
                    .With(o => o.EndDate = new DateTime(2000, 2, 2))
               .Build();
    
            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                Date = new DateFilterDto
                {
                    StartDate = new DateTime(2000, 1, 1),
                    EndDate = new DateTime(2000, 1, 30),
                    DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
                },
                ExtendedFilter = new ExtendedFilterDto
                {
                    UsePeriodForAllYears = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithUsePeriodForAllYearsOnlyStartDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .With(o => o.StartDate = new DateTime(2000, 1, 2))
                    .With(o => o.EndDate = new DateTime(2000, 2, 2))
                .TheNext(30)
                    .With(o => o.StartDate = new DateTime(2001, 1, 15))
                    .With(o => o.EndDate = new DateTime(2001, 1, 15))
                .TheNext(40)
                    .With(o => o.StartDate = new DateTime(1999, 12, 30))
                    .With(o => o.EndDate = new DateTime(2000, 1, 2))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                Date = new DateFilterDto
                {
                    StartDate = new DateTime(2000, 1, 1),
                    EndDate = new DateTime(2000, 1, 30),
                    DateFilterType = DateFilterTypeDto.OnlyStartDate
                },
                ExtendedFilter = new ExtendedFilterDto
                {
                    UsePeriodForAllYears = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithUsePeriodForAllYearsOnlyEndDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .With(o => o.StartDate = new DateTime(1999, 12, 30))
                    .With(o => o.EndDate = new DateTime(2000, 1, 2))
                .TheNext(30)
                    .With(o => o.StartDate = new DateTime(2001, 1, 15))
                    .With(o => o.EndDate = new DateTime(2001, 1, 30))
                .TheNext(40)
                    .With(o => o.StartDate = new DateTime(2000, 1, 15))
                    .With(o => o.EndDate = new DateTime(2000, 2, 1))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                Date = new DateFilterDto
                {
                    StartDate = new DateTime(2000, 1, 1),
                    EndDate = new DateTime(2000, 1, 30),
                    DateFilterType = DateFilterTypeDto.OnlyEndDate
                },
                ExtendedFilter = new ExtendedFilterDto
                {
                    UsePeriodForAllYears = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithUsePeriodForAllYearsOverlappingStartDateAndEndDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(20)
                    .With(o => o.StartDate = new DateTime(2000, 1, 30))
                    .With(o => o.EndDate = new DateTime(2000, 2, 28))
                .TheNext(20)
                    .With(o => o.StartDate = new DateTime(2001, 12, 30))
                    .With(o => o.EndDate = new DateTime(2002, 1, 3))
                .TheNext(20)
                    .With(o => o.StartDate = new DateTime(2004, 1, 15))
                    .With(o => o.EndDate = new DateTime(2004, 1, 30))
                .TheNext(40)
                    .With(o => o.StartDate = new DateTime(2000, 3, 15))
                    .With(o => o.EndDate = new DateTime(2000, 4, 1))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                Date = new DateFilterDto
                {
                    StartDate = new DateTime(2000, 1, 1),
                    EndDate = new DateTime(2000, 1, 30),
                    DateFilterType = DateFilterTypeDto.OverlappingStartDateAndEndDate
                },
                ExtendedFilter = new ExtendedFilterDto
                {
                    UsePeriodForAllYears = true, 
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithNotUnspontaneous()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.Unspontaneous = false)
                .TheNext(40)
                    .With(o => o.Unspontaneous = true)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    UnspontaneousFilter = ExtendedFilterDto.SightingUnspontaneousFilterDto.NotUnspontaneous,
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithUnspontaneous()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .With(o => o.Unspontaneous = true)
                .TheNext(40)
                    .With(o => o.Unspontaneous = false)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    UnspontaneousFilter = ExtendedFilterDto.SightingUnspontaneousFilterDto.Unspontaneous,
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithValidationStatusIds()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                   .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(10))
               .TheNext(30)
                   .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(20))
               .TheNext(20)
                   .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(60))
               .TheNext(20)
                   .With(o => o.ValidationStatus = null)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    VerificationStatusIds = new [] { 10, 20 },
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithVerificationStatusIds()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                   .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(10))
               .TheNext(30)
                   .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(20))
               .TheNext(20)
                   .With(o => o.ValidationStatus = new Lib.Models.Shared.Metadata<int>(60))
               .TheNext(20)
                   .With(o => o.ValidationStatus = null)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    VerificationStatusIds = new[] { 10, 20 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithWeightEq()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(60)
                   .With(o => o.Weight = 100)
               .TheNext(20)
                   .With(o => o.Weight = 80)
               .TheNext(20)
                   .With(o => o.Weight = 110)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Weight = 100,
                    WeightOperator = "eq"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithWeightGte()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                   .With(o => o.Weight = 100)
               .TheNext(30)
                   .With(o => o.Weight = 110)
               .TheNext(20)
                   .With(o => o.Weight = 80)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Weight = 100,
                    WeightOperator = "gte"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithWeightLte()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                   .With(o => o.Weight = 100)
               .TheNext(30)
                   .With(o => o.Weight = 80)
               .TheNext(20)
                   .With(o => o.Weight = 110)
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Weight = 100,
                    WeightOperator = "lte"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithYearsStartDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                    .With(o => o.StartDate = new DateTime(2000, 12, 30))
                    .With(o => o.EndDate = new DateTime(2001, 1, 1))
                .TheNext(30)
                    .With(o => o.StartDate = new DateTime(2001, 12, 1))
                    .With(o => o.EndDate = new DateTime(2001, 12, 1))
                .TheLast(40)
                    .With(o => o.StartDate = new DateTime(2002, 1, 1))
                    .With(o => o.EndDate = new DateTime(2002, 1, 1))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Years = new[] { 2000, 2001 },
                    YearsComparison = ExtendedFilterDto.DateFilterComparisonDto.StartDate
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithYearsEndDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                    .With(o => o.StartDate = new DateTime(2000, 1, 30))
                    .With(o => o.EndDate = new DateTime(2000, 1, 30))
                .TheNext(30)
                    .With(o => o.StartDate = new DateTime(2001, 11, 30))
                    .With(o => o.EndDate = new DateTime(2001, 12, 1))
                .TheLast(40)
                    .With(o => o.StartDate = new DateTime(2002, 3, 1))
                    .With(o => o.EndDate = new DateTime(2002, 3, 1))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Years = new[] { 2000, 2001 },
                    YearsComparison = ExtendedFilterDto.DateFilterComparisonDto.EndDate
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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
        public async Task GetObservationsWithYearsBothStartDateAndEndDate()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
               .All()
                   .HaveValuesFromPredefinedObservations()
               .TheFirst(30)
                    .With(o => o.StartDate = new DateTime(2000, 1, 1))
                    .With(o => o.EndDate = new DateTime(2000, 1, 1))
                .TheNext(30)
                    .With(o => o.StartDate = new DateTime(2001, 12, 1))
                    .With(o => o.EndDate = new DateTime(2001, 12, 1))
                .TheNext(20)
                    .With(o => o.StartDate = new DateTime(2001, 1, 30))
                    .With(o => o.EndDate = new DateTime(2002, 2, 1))
                .TheLast(20)
                    .With(o => o.StartDate = new DateTime(1999, 12, 30))
                    .With(o => o.EndDate = new DateTime(2000, 1, 1))
               .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterInternalDto
            {
                ExtendedFilter = new ExtendedFilterDto
                {
                    Years = new[] { 2000, 2001 },
                    YearsComparison = ExtendedFilterDto.DateFilterComparisonDto.BothStartDateAndEndDate
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearchInternal(
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