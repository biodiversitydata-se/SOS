using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.DataStewardship;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Managers.Interfaces
{
    public interface IDataStewardshipManager
    {
        /// <summary>
        /// Get a single dataset by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<DatasetDto> GetDatasetByIdAsync(string id);

        /// <summary>
        /// Search for datasets
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResultDto<DatasetDto>> GetDatasetsBySearchAsync(SearchFilter filter, int skip, int take);

        /// <summary>
        /// Get a single event by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="responseCoordinateSystem"></param>
        /// <returns></returns>
        Task<EventDto> GetEventByIdAsync(string id, CoordinateSys responseCoordinateSystem);

        /// <summary>
        /// Search for events
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="responseCoordinateSystem"></param>
        /// <returns></returns>
        Task<PagedResultDto<EventDto>> GetEventsBySearchAsync(SearchFilter filter, int skip, int take, CoordinateSys responseCoordinateSystem);

        /// <summary>
        /// Get a occurrence by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="responseCoordinateSystem"></param>
        /// <returns></returns>
        Task<OccurrenceDto> GetOccurrenceByIdAsync(string id, CoordinateSys responseCoordinateSystem);

        /// <summary>
        /// Search for occurrences
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="responseCoordinateSystem"></param>
        /// <returns></returns>
        Task<PagedResultDto<OccurrenceDto>> GetOccurrencesBySearchAsync(SearchFilter filter, int skip, int take, CoordinateSys responseCoordinateSystem);
    }
}
