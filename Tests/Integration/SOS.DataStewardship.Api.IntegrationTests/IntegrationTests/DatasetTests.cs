using SOS.DataStewardship.Api.IntegrationTests.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
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
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        string identifier = "Abc";
        var datasets = Builder<ObservationDataset>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Identifier = identifier)
                .With(m => m.Purpose = null)
                .With(m => m.AccessRights = null)
            .Build();
        await ProcessFixture.AddDatasetsToElasticsearchAsync(datasets);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var dataset = await ApiClient.GetFromJsonAsync<Dataset>($"datastewardship/datasets/{identifier}", jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        dataset.Should().NotBeNull();
        dataset.Identifier.Should().Be("Abc");
    }

    [Fact]
    public async Task Post_DatasetBySearch_Success()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        string identifier = "Abc";
        var datasets = Builder<ObservationDataset>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.Identifier = identifier)
                .With(m => m.Purpose = null)
                .With(m => m.AccessRights = null)
            .Build();
        await ProcessFixture.AddDatasetsToElasticsearchAsync(datasets);

        var observations = Builder<Observation>.CreateListOfSize(1)
            .TheFirst(1)
                .With(m => m.DataStewardshipDatasetId = identifier)
            .Build();
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);

        var searchFilter = new DatasetFilter { 
            DatasetList = new List<string> { "Abc" } 
        };

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var pageResult = await ApiClient.GetFromJsonPostAsync<PagedResult<Dataset>, DatasetFilter>($"datastewardship/datasets", searchFilter, jsonSerializerOptions);

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        pageResult.Records.First().Identifier.Should().Be("Abc");
    }
}