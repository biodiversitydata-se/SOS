using SOS.Lib.Models.Processed.DataStewardship.Dataset;

namespace SOS.DataStewardship.Api.IntegrationTests.Setup;

//[Collection("MyCollection")]
public class TestBase
{
    protected HttpClient Client => _factory.CreateClient();
    protected readonly DataStewardshipApiWebApplicationFactory<Program> _factory;

    public TestBase(DataStewardshipApiWebApplicationFactory<Program> webApplicationFactory)
    {
        _factory = webApplicationFactory;
        //DataStewardshipApiWebApplicationFactory<Program>.SetupDatabase(webApplicationFactory.PostgresContainer.ConnectionString);
    }

    protected async Task AddDatasetsToElasticsearchAsync(IEnumerable<ObservationDataset> datasets, bool clearExistingObservations = true)
    {
        if (clearExistingObservations)
        {
            await _factory.ObservationDatasetRepository.DeleteAllDocumentsAsync();
        }
        await _factory.ObservationDatasetRepository.DisableIndexingAsync();
        await _factory.ObservationDatasetRepository.AddManyAsync(datasets);
        await _factory.ObservationDatasetRepository.EnableIndexingAsync();
    }
}
