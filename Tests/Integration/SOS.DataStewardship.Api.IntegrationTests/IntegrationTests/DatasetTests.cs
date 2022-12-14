namespace SOS.DataStewardship.Api.IntegrationTests.IntegrationTests;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DatasetTests : TestBase
{
    public DatasetTests(DataStewardshipApiWebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    [Fact]
    public async Task Get_DatasetById_Success()
    {
        // Arrange data
        var datasets = Builder<ObservationDataset>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Identifier = "Abc")
                .With(m => m.Creator = new Lib.Models.Processed.DataStewardship.Common.Organisation { OrganisationCode = "A", OrganisationID = "B" })
                .With(m => m.Purpose = null)
                .With(m => m.AccessRights = null)
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
    public async Task Get_DatasetById_Issue_with_json_enum_serialization()
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
    public async Task Get_DatasetById_Issue_with_lowercase()
    {
        // Arrange data
        var datasets = Builder<ObservationDataset>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Identifier = "Abc")
                .With(m => m.Creator = new Lib.Models.Processed.DataStewardship.Common.Organisation { OrganisationCode = "A", OrganisationID = "B" })
                .With(m => m.Purpose = null)
                .With(m => m.AccessRights = null)
            .Build();
        await AddDatasetsToElasticsearchAsync(datasets);
        string identifier = "Abc"; // this currently doesn't work. It works with lowercase "abc"

        // Act
        var response = await Client.GetFromJsonAsync<Dataset>($"datastewardship/datasets/{identifier}");

        // Assert        
        response.Should().NotBeNull();
        response.Identifier.Should().Be("Abc");
    }
}