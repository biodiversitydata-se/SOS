using SOS.DataStewardship.Api.Managers.Interfaces;
using SOS.DataStewardship.Api.Models;

namespace SOS.DataStewardship.Api.Managers;

public class DataStewardshipManager : IDataStewardshipManager
{    
    IObservationDatasetRepository _observationDatasetRepository;
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

        var dataset = new Dataset();
        dataset.Identifier = observationDataset.Identifier;
        dataset.StartDate = observationDataset.StartDate;

        return dataset;
    }
}