using SOS.Lib.Enums.VocabularyValues;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Events.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class EventsGeographicsFilterTests : TestBase
{
    public EventsGeographicsFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task EventsBySearch_ReturnsExpectedEvents_GivenUppsalaMunicipalityFilter()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        var observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithMunicipality(MunicipalityId.Uppsala)
             .TheNext(9).WithMunicipality(MunicipalityId.Knivsta)
            .Build();
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet.Events, observations);

        var searchFilter = new EventsFilter {
            Area = new GeographicsFilter {
                Municipality = Models.Enums.Municipality.Uppsala
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task EventsBySearch_ReturnsExpectedEvents_GivenUppsalaCountyFilter()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        var observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithCounty(CountyId.Uppsala)
             .TheNext(9).WithCounty(CountyId.Stockholm)
            .Build();
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet.Events, observations);

        var searchFilter = new EventsFilter {
            Area = new GeographicsFilter {
                County = Models.Enums.County.UppsalaL�n
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task EventsBySearch_ReturnsExpectedEvents_GivenUpplandProvinceFilter()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        var observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithProvince(ProvinceIds.Uppland)
             .TheNext(9).WithProvince(ProvinceIds.Sm�land)
            .Build();
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);

        var searchFilter = new EventsFilter {
            Area = new GeographicsFilter {
                Province = Models.Enums.Province.Uppland
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task EventsBySearch_ReturnsExpectedEvents_GivenUppsalaParishFilter()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        var observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithParish(ParishId.Uppsala)
             .TheNext(9).WithParish(ParishId.S�by)
            .Build();
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet.Events, observations);

        var searchFilter = new EventsFilter {
            Area = new GeographicsFilter {
                Parish = Models.Enums.Parish.Uppsala
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task EventsBySearch_ReturnsExpectedEvents_GivenPolygonFilter()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        var observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithPosition(58.0, 14.0)
             .TheNext(9).WithPosition(65.0, 20.0)
            .Build();
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet.Events, observations);

        var searchFilter = new EventsFilter {
            Area = new GeographicsFilter {
                Geometry = TestData.CreatePolygonFilterFromBbox(13.0, 57.0, 15.0, 59.0)
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task EventsBySearch_ReturnsExpectedEvents_GivenPointFilter()
    {
        // Arrange
        var testDataSet = TestData.Create(10);
        var observations = testDataSet.ObservationsBuilder
            .TheFirst(1).WithPosition(58.0, 14.0)
             .TheNext(9).WithPosition(65.0, 20.0)
            .Build();
        await ProcessFixture.AddDataToElasticsearchAsync(testDataSet.Events, observations);

        var searchFilter = new EventsFilter {
            Area = new GeographicsFilter {
                Geometry = TestData.CreateCircleFilterFromPoint(58.0, 14.0, 100)
            }
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<EventModel>, EventsFilter>(
            $"datastewardship/events?skip=0&take=0", searchFilter, jsonSerializerOptions);

        // Assert
        pageResult.TotalCount.Should().Be(1);
    }
}