using FizzWare.NBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsByCursorEndpoint;

[Collection(TestCollection.Name)]
public class ObservationsByCursorTests : TestBase
{
    public ObservationsByCursorTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task SearchByCursorEndpoint_PaginatesThrough100Observations_WhenTaking30AtATime()
    {
        // Arrange - Create 100 observations
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto 
        { 
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.Minimum }
        };

        // Act - Paginate through all observations, 30 at a time
        var allRecords = new List<Observation>();
        string? cursor = null;
        int pageCount = 0;
        const int take = 30;

        do
        {
            var url = $"/observations/searchbycursor?take={take}&sortBy=taxon.id&sortOrder=asc";
            if (!string.IsNullOrEmpty(cursor))
            {                
                url += $"&cursor={cursor}";
            }

            var response = await apiClient.PostAsync(url, JsonContent.Create(searchFilter));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();
            result.Should().NotBeNull();
            
            if (result!.Records?.Any() == true)
            {
                allRecords.AddRange(result.Records);
            }
            
            cursor = result.NextCursor;            
            pageCount++;            
            if (pageCount > 10) break; // Safety check to prevent infinite loop
        } while (!string.IsNullOrEmpty(cursor));

        // Assert
        allRecords.Should().HaveCount(100);
        pageCount.Should().Be(5); // 100 observations / 30 per page = 5 pages (30 + 30 + 30 + 10 + 0)
        
        // Verify no duplicates
        var uniqueIds = allRecords.Select(o => o.Occurrence?.OccurrenceId).Distinct().ToList();
        uniqueIds.Should().HaveCount(100);
    }

    [Fact]
    public async Task SearchByCursorEndpoint_ReturnsCorrectTotalCount_WhenPaginating()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto 
        { 
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present 
        };

        // Act
        var response = await apiClient.PostAsync(
            "/observations/searchbycursor?take=30&sortBy=taxon.id&sortOrder=asc", 
            JsonContent.Create(searchFilter));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();
        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(100);
        result.Take.Should().Be(30);
        result.Records.Should().HaveCount(30);
        result.NextCursor.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchByCursorEndpoint_ReturnsNullNextCursor_WhenLastPage()
    {
        // Arrange - Create only 20 observations
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(20)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto 
        { 
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present 
        };

        // Act - Request more than exists
        var response1 = await apiClient.PostAsync(
            "/observations/searchbycursor?take=30&sortBy=taxon.id&sortOrder=asc", 
            JsonContent.Create(searchFilter));        
        var result1 = await response1.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();        
        result1!.Records.Should().HaveCount(20);
        result1.NextCursor.Should().NotBeNull();

        // Second request will return no records and NextCursor null
        var cursor = result1!.NextCursor;
        var response2 = await apiClient.PostAsync(
            $"/observations/searchbycursor?take=30&sortBy=taxon.id&sortOrder=asc&cursor={cursor}",
            JsonContent.Create(searchFilter));
        var result2 = await response2.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Assert - Last page should have no records and null NextCursor
        response2.StatusCode.Should().Be(HttpStatusCode.OK);        
        result2!.Records.Should().HaveCount(0);
        result2.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task SearchByCursorEndpoint_MaintainsSortOrder_WhenPaginatingAscending()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        { 
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.Minimum }
        };

        // Act - Get first two pages
        var response1 = await apiClient.PostAsync(
            "/observations/searchbycursor?take=30&sortBy=taxon.id&sortOrder=asc", 
            JsonContent.Create(searchFilter));
        var result1 = await response1.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        var cursor = result1!.NextCursor;
        var response2 = await apiClient.PostAsync(
            $"/observations/searchbycursor?take=30&sortBy=taxon.id&sortOrder=asc&cursor={cursor}", 
            JsonContent.Create(searchFilter));
        var result2 = await response2.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Assert - Verify sort order is maintained across pages
        var allTaxonIds = result1!.Records!
            .Concat(result2!.Records!)
            .Select(o => o.Taxon?.Id ?? 0)
            .ToList();

        allTaxonIds.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task SearchByCursorEndpoint_MaintainsSortOrder_WhenPaginatingDescending()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto 
        { 
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.Minimum }
        };

        // Act - Get first two pages with descending order
        var response1 = await apiClient.PostAsync(
            "/observations/searchbycursor?take=30&sortBy=taxon.id&sortOrder=desc", 
            JsonContent.Create(searchFilter));
        var result1 = await response1.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        var cursor = result1!.NextCursor;
        var response2 = await apiClient.PostAsync(
            $"/observations/searchbycursor?take=30&sortBy=taxon.id&sortOrder=desc&cursor={cursor}", 
            JsonContent.Create(searchFilter));
        var result2 = await response2.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

        // Assert - Verify sort order is maintained across pages
        var allTaxonIds = result1!.Records!
            .Concat(result2!.Records!)
            .Select(o => o.Taxon?.Id ?? 0)
            .ToList();

        allTaxonIds.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task SearchByCursorEndpoint_ReturnsNoDuplicates_WhenAllObservationsHaveSameModifiedDate()
    {
        // Arrange - Create observations with same modified date to test tiebreaker
        var sameDate = new DateTime(2024, 12, 24);
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
                .With(m => m.EditDate = sameDate)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto 
        { 
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present,
            Output = new OutputFilterExtendedDto { FieldSet = Lib.Enums.OutputFieldSet.All }
        };

        // Act - Paginate through all observations
        var allOccurrenceIds = new HashSet<string>();
        string? cursor = null;
        int totalRecords = 0;

        do
        {
            var url = $"/observations/searchbycursor?take=30&sortBy=modified&sortOrder=asc";
            if (!string.IsNullOrEmpty(cursor))
            {
                url += $"&cursor={cursor}";
            }

            var response = await apiClient.PostAsync(url, JsonContent.Create(searchFilter));            
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();
            
            if (result?.Records?.Any() == true)
            {
                foreach (var obs in result.Records)
                {
                    // Each occurrence ID should be unique
                    allOccurrenceIds.Should().NotContain(obs.Occurrence?.OccurrenceId, 
                        "Duplicate found: {0}", obs.Occurrence?.OccurrenceId);
                    allOccurrenceIds.Add(obs.Occurrence?.OccurrenceId!);
                }
                totalRecords += result.Records.Count();
            }

            cursor = result?.NextCursor;           
        } while (!string.IsNullOrEmpty(cursor));

        // Assert
        allOccurrenceIds.Should().HaveCount(100);
    }

    [Fact]
    public async Task SearchByCursorEndpoint_ReturnsBadRequest_WhenInvalidCursorFormat()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(10)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto 
        { 
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present 
        };

        // Act - Send invalid cursor value
        var response = await apiClient.PostAsync(
            "/observations/searchbycursor?take=30&sortBy=taxon.id&cursor=invalid_cursor", 
            JsonContent.Create(searchFilter));

        // Assert        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchByCursorEndpoint_ReturnsBadRequest_WhenTakeExceeds5000()
    {
        // Arrange
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto 
        { 
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present 
        };

        // Act
        var response = await apiClient.PostAsync(
            "/observations/searchbycursor?take=5001&sortBy=taxon.id", 
            JsonContent.Create(searchFilter));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchByCursorEndpoint_WorksWithFilter_WhenFilteringByTaxon()
    {
        // Arrange - Create observations with different taxon IDs
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(50).With(m => m.TaxonId = 100001)
            .TheNext(50).With(m => m.TaxonId = 100002)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        
        // Filter for only taxon 100001
        var searchFilter = new SearchFilterDto 
        { 
            OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present,
            Taxon = new TaxonFilterDto { Ids = [100001] }
        };

        // Act - Paginate through filtered results
        var allRecords = new List<Observation>();
        string? cursor = null;

        do
        {
            var url = $"/observations/searchbycursor?take=30&sortBy=taxon.id&sortOrder=asc";
            if (!string.IsNullOrEmpty(cursor))
            {
                url += $"&cursor={cursor}";
            }

            var response = await apiClient.PostAsync(url, JsonContent.Create(searchFilter));
            var result = await response.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();
            
            if (result?.Records?.Any() == true)
            {
                allRecords.AddRange(result.Records);
            }

            cursor = result?.NextCursor;

            if (allRecords.Count >= 50) break;

        } while (!string.IsNullOrEmpty(cursor));

        // Assert - Should only get 50 observations with taxon 100001
        allRecords.Should().HaveCount(50);
        allRecords.Should().AllSatisfy(o => o.Taxon?.Id.Should().Be(100001));
    }
}