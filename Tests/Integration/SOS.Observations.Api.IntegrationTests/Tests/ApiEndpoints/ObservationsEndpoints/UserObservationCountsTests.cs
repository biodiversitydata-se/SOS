using FizzWare.NBuilder;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints;

[Collection(TestCollection.Name)]
public class UserObservationCountsTests(TestFixture testFixture, ITestOutputHelper output) : TestBase(testFixture, output)
{
    [Fact]
    public async Task CurrentUserCounts_ReturnOnlyOwnObservations_AndEmptyDayPageBeyondResults()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        for (var index = 0; index < observations.Count; index++)
        {
            var userId = index < 3 ? TestAuthHandler.DefaultTestUserId : 999;
            observations[index].ObserversInternal =
            [
                new UserInternal { Id = userId, PersonId = userId, UserServiceUserId = userId, ViewAccess = true }
            ];
            observations[index].Observers = "Test observer";
            observations[index].ReportedByUserServiceUserId = userId;
            observations[index].StartDate = new DateTime(2000, 6, index < 2 ? 16 : 15);
            observations[index].EndDate = observations[index].StartDate;
            observations[index].StartTime = observations[index].EndTime = TimeSpan.FromHours(12);
            observations[index].TaxonId = index == 1 ? 100012 : 100011;
            observations[index].Site.Id = index == 1 ? 4060924 : 4060923;
            observations[index].Site.Name = index == 1 ? "Second locality" : "First locality";
        }
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        using var apiClient = TestFixture.CreateApiClient();
        var filter = new SearchFilterAggregationInternalDto();

        using var yearResponse = await apiClient.PostAsync("/Observations/Internal/CurrentUser/YearCount",
            JsonContent.Create(filter), TestContext.Current.CancellationToken);
        yearResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var years = await yearResponse.Content.ReadFromJsonAsync<List<YearCountResultDto>>(
            JsonSerializerOptions, TestContext.Current.CancellationToken);
        years.Should().BeEquivalentTo(new[] { new YearCountResultDto { Year = 2000, Count = 3, TaxonCount = 2 } });

        using var monthResponse = await apiClient.PostAsync("/Observations/Internal/CurrentUser/YearMonthCount",
            JsonContent.Create(filter), TestContext.Current.CancellationToken);
        monthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var months = await monthResponse.Content.ReadFromJsonAsync<List<YearMonthCountResultDto>>(
            JsonSerializerOptions, TestContext.Current.CancellationToken);
        months.Should().BeEquivalentTo(new[] { new YearMonthCountResultDto { Year = 2000, Month = 6, Count = 3, TaxonCount = 2 } });

        using var pageResponse = await apiClient.PostAsync("/Observations/Internal/CurrentUser/YearMonthDayCount?skip=2&take=1",
            JsonContent.Create(filter), TestContext.Current.CancellationToken);
        pageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await pageResponse.Content.ReadFromJsonAsync<List<YearMonthDayCountResultDto>>(
            JsonSerializerOptions, TestContext.Current.CancellationToken);
        page.Should().BeEmpty();
    }
}
