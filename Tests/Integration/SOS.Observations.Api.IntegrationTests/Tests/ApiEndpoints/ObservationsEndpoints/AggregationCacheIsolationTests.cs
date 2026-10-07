using FluentAssertions.Execution;
using FizzWare.NBuilder;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints;

[Collection(TestCollection.Name)]
public class AggregationCacheIsolationTests(TestFixture fixture, ITestOutputHelper output) : TestBase(fixture, output)
{
    [Fact]
    public async Task Aggregation_DoesNotShareMyObservationResultsBetweenUsers()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations().Build();
        for (var index = 0; index < observations.Count; index++)
        {
            var userId = index == 0 ? 101 : 202;
            observations[index].TaxonId = 100011 + index;
            observations[index].ProtectedBySystem = false;
            observations[index].ReportedByUserServiceUserId = userId;
            observations[index].ObserversInternal =
            [new UserInternal { Id = userId, PersonId = userId, UserServiceUserId = userId, ViewAccess = true }];
        }
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        foreach (var taxonAggregation in new[] { false, true })
        {
            foreach (var observedByMe in new[] { false, true })
            {
                SearchFilterBaseDto filter = taxonAggregation
                    ? new SearchFilterAggregationInternalDto { ProtectionFilter = ProtectionFilterDto.Public }
                    : new SearchFilterAggregationDto();
                filter.ObservedByMe = observedByMe;
                filter.ReportedByMe = !observedByMe;
                filter.Taxon = new TaxonFilterDto { Ids = [100011, 100012, 100013] };
                var endpoint = taxonAggregation ? "/observations/internal/taxonaggregation" : "/observations/geogridaggregation";

                // The JSON filter is identical, but the authenticated user changes its meaning.
                foreach (var (userId, expectedCount) in new[] { (101, 1), (202, 2), (101, 1) })
                {
                    using var client = TestFixture.CreateApiClient();
                    client.DefaultRequestHeaders.Add(TestAuthHandler.UserId, userId.ToString());
                    using var response = await client.PostAsync(endpoint,
                        JsonContent.Create(filter, filter.GetType()), TestContext.Current.CancellationToken);
                    var context = $"endpoint={endpoint}, observedByMe={observedByMe}, userId={userId}";
                    response.StatusCode.Should().Be(HttpStatusCode.OK, context);
                    using var scope = new AssertionScope(context);
                    if (taxonAggregation)
                    {
                        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<TaxonAggregationItemDto>>(
                            TestContext.Current.CancellationToken);
                        result!.TotalCount.Should().Be(expectedCount);
                    }
                    else
                    {
                        var result = await response.Content.ReadFromJsonAsync<GeoGridResultDto>(TestContext.Current.CancellationToken);
                        result!.GridCells.Sum(cell => cell.ObservationsCount).Should().Be(expectedCount);
                    }
                }
            }
        }
    }
}
