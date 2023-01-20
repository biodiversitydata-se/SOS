using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DatasetTests : TestBase
{
    public DatasetTests(ApiWebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    [Fact]
    public async Task Get_DatasetById_Success()
    {
        // Arrange
        string identifier = "Abc";
        var datasets = Builder<ObservationDataset>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Identifier = identifier)                
                .With(m => m.Purpose = null)
                .With(m => m.AccessRights = null)
            .Build();
        await AddDatasetsToElasticsearchAsync(datasets);        

        // Act
        var response = await Client.GetFromJsonAsync<Dataset>($"datastewardship/datasets/{identifier}", jsonSerializerOptions);

        // Assert        
        response.Should().NotBeNull();
        response.Identifier.Should().Be("Abc");
    }  

    [Fact]
    public async Task Post_DatasetBySearch_Success()
    {
        // Arrange
        string identifier = "Abc";
        var datasets = Builder<ObservationDataset>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Identifier = identifier)                
                .With(m => m.Purpose = null)
                .With(m => m.AccessRights = null)
            .Build();
        await AddDatasetsToElasticsearchAsync(datasets);

        var observations = Builder<Observation>.CreateListOfSize(1)
            .TheFirst(1)                
                .With(m => m.DataStewardshipDatasetId = identifier)
            .Build();
        await AddObservationsToElasticsearchAsync(observations);
        
        var body = new DatasetFilter { DatasetList = new List<string> { "Abc" } };
        
        // Act
        var response = await Client.PostAsJsonAsync($"datastewardship/datasets", body, jsonSerializerOptions);
        var pageResult = await response.Content.ReadFromJsonAsync<PagedResult<Dataset>>(jsonSerializerOptions);

        // Assert        
        pageResult.Records.First().Identifier.Should().Be("Abc");        
    }
}