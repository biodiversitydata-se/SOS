using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.Application.Managers.Interfaces;

public interface IDataStewardshipManager
{
    Task<Dataset> GetDatasetByIdAsync(string id);
    Task<Contracts.Models.PagedResult<Dataset>> GetDatasetsBySearchAsync(DatasetFilter datasetFilter, int skip, int take);
    Task<Contracts.Models.Event> GetEventByIdAsync(string id, CoordinateSystem responseCoordinateSystem);
    Task<Contracts.Models.PagedResult<Contracts.Models.Event>> GetEventsBySearchAsync(EventsFilter filter, int skip, int take, CoordinateSystem responseCoordinateSystem);
    Task<Contracts.Models.Occurrence> GetOccurrenceByIdAsync(string id, CoordinateSystem responseCoordinateSystem);
    Task<Contracts.Models.PagedResult<Contracts.Models.Occurrence>> GetOccurrencesBySearchAsync(OccurrenceFilter occurrenceFilter, int skip, int take, CoordinateSystem responseCoordinateSystem);
}
