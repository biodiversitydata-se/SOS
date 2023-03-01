using SOS.DataStewardship.Api.IntegrationTests.Extensions;
using SOS.DataStewardship.Api.IntegrationTests.Helpers;
using SOS.Lib.Models.Processed.Observation;
using Xunit.Abstractions;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DatasetTests : TestBase
{
    public DatasetTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }    

    [Fact]
    public async Task Get_DatasetById_Success()
    {
        // Arrange
        string identifier = "Abc";
        var datasets = GetDatasetTestData(identifier);
        await ProcessFixture.AddDatasetsToElasticsearchAsync(datasets);

        // Act
        var dataset = await ApiClient.GetFromJsonAsync<Dataset>($"datastewardship/datasets/{identifier}", jsonSerializerOptions);

        // Assert        
        dataset.Should().NotBeNull();
        dataset.Identifier.Should().Be(identifier);
    }

    [Fact]
    public async Task Post_DatasetBySearch_Success()
    {
        // Arrange
        string identifier = "Abc";
        var datasets = GetDatasetTestData(identifier);
        await ProcessFixture.AddDatasetsToElasticsearchAsync(datasets);
        var observations = GetObservationTestData(identifier);
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        var searchFilter = new DatasetFilter {
            DatasetIds = new List<string> { identifier } 
        };

        // Act
        var pageResult = await ApiClient.PostAndReturnAsJsonAsync<PagedResult<Dataset>, DatasetFilter>(
            $"datastewardship/datasets", searchFilter, jsonSerializerOptions);        

        // Assert        
        pageResult.Records.First().Identifier.Should().Be(identifier);
    }

    private IEnumerable<ObservationDataset> GetDatasetTestData(string firstKey)
    {
        var datasets = Builder<ObservationDataset>.CreateListOfSize(10)
            .TheFirst(1)
                .With(m => m.Identifier = firstKey)
                .With(m => m.Purpose = null)
                .With(m => m.AccessRights = null)
            .TheNext(9)
                .With(m => m.Identifier = DataHelper.RandomString(3, new[] { firstKey }))
                .With(m => m.Purpose = null)
                .With(m => m.AccessRights = null)
           .Build();

        return datasets;
    }

    private IEnumerable<Observation> GetObservationTestData(string firstKey)
    {
        var observations = Builder<Observation>.CreateListOfSize(10)
            .TheFirst(1)
                .With(m => m.DataStewardshipDatasetId = firstKey)
            .TheNext(9)
                .With(m => m.DataStewardshipDatasetId = DataHelper.RandomString(3, new[] { firstKey }))
            .Build();

        return observations;
    }
}