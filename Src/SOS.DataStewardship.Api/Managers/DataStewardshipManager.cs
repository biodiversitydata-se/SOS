using SOS.DataStewardship.Api.Extensions;
using SOS.DataStewardship.Api.Managers.Interfaces;
using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Models.SampleData;

namespace SOS.DataStewardship.Api.Managers;

public class DataStewardshipManager : IDataStewardshipManager
{    
    IObservationDatasetRepository _observationDatasetRepository;
    IProcessedObservationCoreRepository _processedObservationCoreRepository;
    private readonly ILogger<DataStewardshipManager> _logger;

    public DataStewardshipManager(IObservationDatasetRepository observationDatasetRepository,
        ILogger<DataStewardshipManager> logger)
    {
        _observationDatasetRepository = observationDatasetRepository;
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
        // 1. Hämta observation med EventId.
        // 2. Hämta ut OccurrenceIds genom scroll?

        // Eller skapa upp nytt Event-index i ES?

        return DataStewardshipArtportalenSampleData.EventBats1;
    }    
}