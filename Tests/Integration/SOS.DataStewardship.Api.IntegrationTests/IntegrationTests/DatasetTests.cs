using SOS.DataStewardship.Api.IntegrationTests.Helpers;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DatasetTests : TestBase
{
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

    public DatasetTests(DataStewardshipApiWebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    [Fact]
    public async Task Get_DatasetById_Success()
    {
        // Arrange
        string identifier = "Abc";
        var datasets = GetDatasetTestData(identifier);
        await AddDatasetsToElasticsearchAsync(datasets);        

        // Act
        var response = await Client.GetFromJsonAsync<Dataset>($"datastewardship/datasets/{identifier}", jsonSerializerOptions);

        // Assert        
        response.Should().NotBeNull();
        response.Identifier.Should().Be(identifier);
    }  

    [Fact]
    public async Task Post_DatasetBySearch_Success()
    {
        // Arrange
        string identifier = "Abc";
        var datasets = GetDatasetTestData(identifier);
        await AddDatasetsToElasticsearchAsync(datasets);

        var observations = GetObservationTestData(identifier);
        await AddObservationsToElasticsearchAsync(observations);
        
        var body = new DatasetFilter { DatasetList = new List<string> { identifier } };
        
        // Act
        var response = await Client.PostAsJsonAsync($"datastewardship/datasets", body, jsonSerializerOptions);
        var pageResult = await response.Content.ReadFromJsonAsync<PagedResult<Dataset>>(jsonSerializerOptions);

        // Assert        
        pageResult.Records.First().Identifier.Should().Be(identifier);        
    }
}