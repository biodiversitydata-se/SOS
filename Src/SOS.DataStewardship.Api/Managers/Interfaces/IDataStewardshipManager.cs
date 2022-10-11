using SOS.DataStewardship.Api.Models;

namespace SOS.DataStewardship.Api.Managers.Interfaces;

public interface IDataStewardshipManager
{
    Task<Dataset> GetDatasetByIdAsync(string id);
    Task<List<Dataset>> GetDatasetsBySearchAsync(DatasetFilter datasetFilter, int skip, int take);
    Task<EventModel> GetEventByIdAsync(string id);
    Task<List<EventModel>> GetEventsBySearchAsync(EventsFilter filter, int skip, int take);
    Task<OccurrenceModel> GetOccurrenceByIdAsync(string id);
    Task<List<OccurrenceModel>> GetOccurrencesBySearchAsync(OccurrenceFilter occurrenceFilter, int skip, int take);
}
