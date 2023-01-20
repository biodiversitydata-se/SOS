using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.DataStewardship.Api.IntegrationTests.Setup;

//[Collection("MyCollection")]
public class TestBase
{
    protected HttpClient Client => _factory.CreateClient();
    protected readonly ApiWebApplicationFactory<Program> _factory;
    protected readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters = {
            new JsonStringEnumConverter(),
            new GeoShapeConverter(),
            new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
        }
    };

    public TestBase(ApiWebApplicationFactory<Program> webApplicationFactory)
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
        await Task.Delay(1000);
    }

    protected async Task AddEventsToElasticsearchAsync(IEnumerable<ObservationEvent> events, bool clearExistingObservations = true)
    {
        if (clearExistingObservations)
        {
            await _factory.ObservationEventRepository.DeleteAllDocumentsAsync();
        }
        await _factory.ObservationEventRepository.DisableIndexingAsync();
        await _factory.ObservationEventRepository.AddManyAsync(events);
        await _factory.ObservationEventRepository.EnableIndexingAsync();
        await Task.Delay(1000);
    }

    protected async Task AddObservationsToElasticsearchAsync(IEnumerable<Observation> observations, bool protectedIndex = false, bool clearExistingObservations = true)
    {
        if (clearExistingObservations)
        {
            await _factory.ProcessedObservationCoreRepository.DeleteAllDocumentsAsync(protectedIndex);
        }
        await _factory.ProcessedObservationCoreRepository.DisableIndexingAsync(protectedIndex);
        await _factory.ProcessedObservationCoreRepository.AddManyAsync(observations, protectedIndex);
        await _factory.ProcessedObservationCoreRepository.EnableIndexingAsync(protectedIndex);
        await Task.Delay(1000);
    }
}
