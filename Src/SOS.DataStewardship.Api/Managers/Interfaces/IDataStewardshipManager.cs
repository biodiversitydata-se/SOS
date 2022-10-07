using SOS.DataStewardship.Api.Models;

namespace SOS.DataStewardship.Api.Managers.Interfaces;

public interface IDataStewardshipManager
{
    Task<Dataset> GetDatasetByIdAsync(string id);

    Task<List<Dataset>> GetDatasetsBySearchAsync(DatasetFilter datasetFilter, int skip, int take);
    Task<EventModel> GetEventByIdAsync(string id);
}
