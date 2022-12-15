using SOS.Lib.Models.Processed.Observation;

namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DatasetTests : TestBase
{
    public DatasetTests(DataStewardshipApiWebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    [Fact]
    public async Task Get_DatasetById_Success()
    {
        // Arrange data
        string identifier = "Abc";
        var datasets = Builder<ObservationDataset>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Identifier = identifier)
                .With(m => m.Creator = new Lib.Models.Processed.DataStewardship.Common.Organisation { OrganisationCode = "A", OrganisationID = "B" })
                .With(m => m.Purpose = null)
                .With(m => m.AccessRights = null)
            .Build();
        await AddDatasetsToElasticsearchAsync(datasets);        

        // Act
        var response = await Client.GetFromJsonAsync<Dataset>($"datastewardship/datasets/{identifier}");

        // Assert        
        response.Should().NotBeNull();
        response.Identifier.Should().Be("Abc");
    }

    /// <remarks>
    /// There is a bug when serializing enums
    /// </remarks>    
    [Fact]
    public async Task Bug_Get_DatasetById_with_json_enum_serialization()
    {
        // Arrange data
        var datasets = Builder<ObservationDataset>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Identifier = "Abc")
                .With(m => m.Creator = new Lib.Models.Processed.DataStewardship.Common.Organisation { OrganisationCode = "A", OrganisationID = "B" })
                .With(m => m.Purpose = null)
                .With(m => m.AccessRights = Lib.Models.Processed.DataStewardship.Enums.AccessRights.Publik)
            .Build();
        await AddDatasetsToElasticsearchAsync(datasets);
        string identifier = "abc";

        // Act
        var response = await Client.GetFromJsonAsync<Dataset>($"datastewardship/datasets/{identifier}");

        // Assert        
        response.Should().NotBeNull();
        response.Identifier.Should().Be("Abc");
    }

    [Fact]
    public async Task Post_DatasetBySearch_Success()
    {
        // Arrange data
        string identifier = "Abc";
        var datasets = Builder<ObservationDataset>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Identifier = identifier)
                .With(m => m.Creator = new Lib.Models.Processed.DataStewardship.Common.Organisation { OrganisationCode = "A", OrganisationID = "B" })
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
        var response = await Client.PostAsJsonAsync($"datastewardship/datasets", body);
        var pageResult = await response.Content.ReadFromJsonAsync<PagedResult<Dataset>>();

        // Assert        
        pageResult.Records.First().Identifier.Should().Be("Abc");        
    }
}