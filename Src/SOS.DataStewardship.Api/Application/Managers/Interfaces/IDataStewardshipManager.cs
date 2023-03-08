using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Application.Managers.Interfaces;

public interface IDataStewardshipManager
{
    Task<Dataset> GetDatasetByIdAsync(string id);
    Task<Contracts.Models.PagedResult<Dataset>> GetDatasetsBySearchAsync(DatasetFilter datasetFilter, int skip, int take);
    Task<EventModel> GetEventByIdAsync(string id, CoordinateSystem responseCoordinateSystem);
    Task<Contracts.Models.PagedResult<EventModel>> GetEventsBySearchAsync(EventsFilter filter, int skip, int take, CoordinateSystem responseCoordinateSystem);
    Task<OccurrenceModel> GetOccurrenceByIdAsync(string id, CoordinateSystem responseCoordinateSystem);
    Task<Contracts.Models.PagedResult<OccurrenceModel>> GetOccurrencesBySearchAsync(OccurrenceFilter occurrenceFilter, int skip, int take, CoordinateSystem responseCoordinateSystem);
}
