using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Models.Enums;

namespace SOS.DataStewardship.Api.Managers.Interfaces;

public interface IDataStewardshipManager
{
    Task<Dataset> GetDatasetByIdAsync(string id);
    Task<Models.PagedResult<Dataset>> GetDatasetsBySearchAsync(DatasetFilter datasetFilter, int skip, int take);
    Task<EventModel> GetEventByIdAsync(string id);
    Task<Models.PagedResult<EventModel>> GetEventsBySearchAsync(EventsFilter filter, int skip, int take);
    Task<OccurrenceModel> GetOccurrenceByIdAsync(string id, CoordinateSystem responseCoordinateSystem);
    Task<Models.PagedResult<OccurrenceModel>> GetOccurrencesBySearchAsync(OccurrenceFilter occurrenceFilter, int skip, int take, CoordinateSystem responseCoordinateSystem);
}
