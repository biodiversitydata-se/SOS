using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Managers.Interfaces;
using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Models.SampleData;
using SOS.Lib.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.DataStewardship.Api.Managers;

public class DataStewardshipManager : IDataStewardshipManager
{    
    private readonly IObservationDatasetRepository _observationDatasetRepository;
    private readonly IProcessedObservationCoreRepository _processedObservationCoreRepository;
    private readonly ILogger<DataStewardshipManager> _logger;

    public DataStewardshipManager(IObservationDatasetRepository observationDatasetRepository,
        IProcessedObservationCoreRepository processedObservationCoreRepository,
        ILogger<DataStewardshipManager> logger)
    {
        _observationDatasetRepository = observationDatasetRepository;
        _processedObservationCoreRepository = processedObservationCoreRepository;
        _logger = logger;
    }

    public async Task<Dataset> GetDatasetByIdAsync(string id)
    {
        var observationDataset = await _observationDatasetRepository.GetDatasetById(id);
        if (observationDataset == null) return null;
        var dataset = observationDataset.ToDataset();        
        return dataset;
    }

    public async Task<List<Dataset>> GetDatasetsBySearchAsync(DatasetFilter datasetFilter, int skip, int take)
    {
        List<int> taxonIds = datasetFilter?.Taxon?.Ids;
        DateTime? startDate = datasetFilter?.Datum?.StartDate;
        DateTime? endDate = datasetFilter?.Datum?.EndDate;
        DatumFilterType? dateFilterType = datasetFilter?.Datum?.DatumFilterType;
        // datasetFilter.Area.

        return null;
    }

    public async Task<EventModel> GetEventByIdAsync(string id)
    {
        var filter = new SearchFilter(0);
        filter.EventIds = new List<string> { id };
        var processedObservations = await _processedObservationCoreRepository.GetChunkAsync(filter, 0, 1);
        var observation = processedObservations.Records.FirstOrDefault();
        Observation obs = CastDynamicToObservation(observation);
        var occurrenceIds = await _processedObservationCoreRepository.GetAllAggregationItemsAsync(filter, "occurrence.occurrenceId");
        var ev = obs.ToEventModel(occurrenceIds.Select(m => m.AggregationKey));
        return ev;
        
        // Skapa upp nytt Event-index i ES?
        return DataStewardshipArtportalenSampleData.EventBats1;
    }

    public async Task<OccurrenceModel> GetOccurrenceByIdAsync(string id)
    {
        var filter = new SearchFilter(0);                
        IEnumerable<dynamic> observations = await _processedObservationCoreRepository.GetObservationAsync(id, filter);
        var observation = observations.FirstOrDefault();
        Observation obs = CastDynamicToObservation(observation);
        var occurrence = obs.ToOccurrenceModel();
        return occurrence;
    }

    private List<Observation> CastDynamicsToObservations(IEnumerable<dynamic> dynamicObjects)
    {
        if (dynamicObjects == null) return null;
        return JsonSerializer.Deserialize<List<Observation>>(JsonSerializer.Serialize(dynamicObjects),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private Observation CastDynamicToObservation(dynamic dynamicObject)
    {
        if (dynamicObject == null) return null;        
        return JsonSerializer.Deserialize<Observation>(JsonSerializer.Serialize(dynamicObject),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        //return JsonSerializer.Deserialize<Observation>(JsonSerializer.Serialize(dynamicObject, _jsonSerializerOptions), _jsonSerializerOptions);
    }

    private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new GeoShapeConverter(),
            new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
        }
    };
}