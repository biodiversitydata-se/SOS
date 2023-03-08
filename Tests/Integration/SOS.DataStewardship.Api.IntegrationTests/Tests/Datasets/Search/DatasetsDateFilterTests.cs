using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Datasets.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DatasetsDateFilterTests : TestBase
{
    public DatasetsDateFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsExpectedDatasets_GivenOverlappingStartDateAndEndDateFilterType()
    {
        // Arrange
        var testDataSet = TestData.Create(8);
        testDataSet.Observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 03)) // hit                        
             .TheNext(1).WithDates(new DateTime(2021, 02, 03), new DateTime(2021, 02, 04)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 04), new DateTime(2021, 02, 05)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 04)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 05)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 01), new DateTime(2021, 02, 04)) // hit
             .TheNext(1).WithDate(new DateTime(2021, 02, 01)) // miss
             .TheNext(1).WithDate(new DateTime(2021, 02, 05)) // miss
            .Build().ToList();
        
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet);
        var searchFilter = new DatasetFilter {
            DateFilter = new DateFilter {
                StartDate = new DateTime(2021, 02, 02, 0, 0, 0),
                EndDate = new DateTime(2021, 02, 04, 23, 59, 59),
                DateFilterType = DateFilterType.OverlappingStartDateAndEndDate
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(6);
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsExpectedDatasets_GivenBetweenStartDateAndEndDateFilterType()
    {
        // Arrange
        var testDataSet = TestData.Create(6);
        testDataSet.Observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 03)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 03), new DateTime(2021, 02, 04)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 04), new DateTime(2021, 02, 05)) // miss
             .TheNext(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 04)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 05)) // miss
             .TheNext(1).WithDates(new DateTime(2021, 02, 01), new DateTime(2021, 02, 04)) // miss            
            .Build().ToList();
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet);
        var searchFilter = new DatasetFilter {
            DateFilter = new DateFilter {
                StartDate = new DateTime(2021, 02, 02, 0, 0, 0),
                EndDate = new DateTime(2021, 02, 04, 23, 59, 59),
                DateFilterType = DateFilterType.BetweenStartDateAndEndDate
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsExpectedDatasets_GivenOnlyStartDateFilterType()
    {
        // Arrange
        var testDataSet = TestData.Create(3);
        testDataSet.Observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithDates(new DateTime(2021, 02, 02), new DateTime(2021, 02, 20)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 03), new DateTime(2021, 02, 20)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 02, 04), new DateTime(2021, 02, 20)) // miss             
            .Build().ToList();
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet);
        var searchFilter = new DatasetFilter {
            DateFilter = new DateFilter {
                StartDate = new DateTime(2021, 02, 02, 0, 0, 0),
                EndDate = new DateTime(2021, 02, 03, 23, 59, 59),
                DateFilterType = DateFilterType.OnlyStartDate
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsExpectedDatasets_GivenOnlyEndDateFilterType()
    {
        // Arrange
        var testDataSet = TestData.Create(3);
        testDataSet.Observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithDates(new DateTime(2021, 01, 20), new DateTime(2021, 02, 02)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 01, 20), new DateTime(2021, 02, 03)) // hit
             .TheNext(1).WithDates(new DateTime(2021, 01, 20), new DateTime(2021, 02, 04)) // miss             
            .Build().ToList();
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet);
        var searchFilter = new DatasetFilter {
            DateFilter = new DateFilter {
                StartDate = new DateTime(2021, 02, 02, 0, 0, 0),
                EndDate = new DateTime(2021, 02, 03, 23, 59, 59),
                DateFilterType = DateFilterType.OnlyEndDate
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets?skip=0&take=1", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(2);        
    }
}