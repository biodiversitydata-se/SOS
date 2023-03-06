using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;
using SOS.Lib.Helpers;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Occurrences.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class OccurrencesDateFilterTests : TestBase
{
    public OccurrencesDateFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task OccurrencesBySearch_ReturnsOccurrencesInDateRange_GivenOverlappingStartDateAndEndDateFilterType()
    {
        // Arrange
        var testDataSet = TestData.Create(8);
        var observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 03)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 03), new DateTime(2021, 02, 04)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 04), new DateTime(2021, 02, 05)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 04)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 05)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 01), new DateTime(2021, 02, 04)) // hit
             .TheNext(1).WithDate(new DateTime(2021, 02, 01)) // miss
             .TheNext(1).WithDate(new DateTime(2021, 02, 05)) // miss
            .Build();
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new OccurrenceFilter() {
            DateFilter = new DateFilter {
                StartDate = new DateTime(2021, 02, 02, 0, 0, 0),
                EndDate = new DateTime(2021, 02, 04, 23, 59, 59),
                DateFilterType = DateFilterType.OverlappingStartDateAndEndDate
            }
        };
        
        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(6);
    }

    [Fact]
    public async Task OccurrencesBySearch_ReturnsOccurrencesInDateRange_GivenBetweenStartDateAndEndDateFilterType()
    {
        // Arrange
        var testDataSet = TestData.Create(6);
        var observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 03)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 03), new DateTime(2021, 02, 04)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 04), new DateTime(2021, 02, 05)) // miss
             .TheNext(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 04)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 05)) // miss
             .TheNext(1).WithDates(new DateTime(2021, 02, 01), new DateTime(2021, 02, 04)) // miss            
            .Build();
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new OccurrenceFilter()
        {
            DateFilter = new DateFilter
            {
                StartDate = new DateTime(2021, 02, 02, 0, 0, 0),
                EndDate = new DateTime(2021, 02, 04, 23, 59, 59),
                DateFilterType = DateFilterType.BetweenStartDateAndEndDate
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task OccurrencesBySearch_ReturnsOccurrencesInDateRange_GivenOnlyStartDateFilterType()
    {
        // Arrange
        var testDataSet = TestData.Create(3);
        var observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 20)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 03), new DateTime(2021, 02, 20)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 04), new DateTime(2021, 02, 20)) // miss             
            .Build();
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new OccurrenceFilter()
        {
            DateFilter = new DateFilter
            {
                StartDate = new DateTime(2021, 02, 02, 0, 0, 0),
                EndDate = new DateTime(2021, 02, 03, 23, 59, 59),
                DateFilterType = DateFilterType.OnlyStartDate
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task OccurrencesBySearch_ReturnsOccurrencesInDateRange_GivenOnlyEndDateFilterType()
    {
        // Arrange
        var testDataSet = TestData.Create(3);
        var observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithDates(new DateTime(2021, 01, 20), new DateTime(2021, 02, 02)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 01, 20), new DateTime(2021, 02, 03)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 01, 20), new DateTime(2021, 02, 04)) // miss             
            .Build();
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new OccurrenceFilter()
        {
            DateFilter = new DateFilter
            {
                StartDate = new DateTime(2021, 02, 02, 0, 0, 0),
                EndDate = new DateTime(2021, 02, 03, 23, 59, 59),
                DateFilterType = DateFilterType.OnlyEndDate
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<OccurrenceModel>, OccurrenceFilter>(
            $"datastewardship/occurrences?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(2);
    }
}