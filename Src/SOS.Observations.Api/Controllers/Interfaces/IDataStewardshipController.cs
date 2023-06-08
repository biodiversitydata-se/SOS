using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos.DataStewardship.Enums;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Controllers.Interfaces
{
    public interface IDataStewardshipController
    {
        /// <summary>
        /// Get a single dataset by id
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="id"></param>
        /// <param name="exportMode"></param>
        /// <returns></returns>
        Task<IActionResult> GetDatasetByIdAsync(int? roleId, string authorizationApplicationIdentifier, string id, DsExportMode exportMode);

        /// <summary>
        /// Search for datasets
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="exportMode"></param>
        /// <returns></returns>
        Task<IActionResult> GetDatasetsBySearchAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilterBaseDto filter, bool validateSearchFilter, int skip, int take, DsExportMode exportMode);

        /// <summary>
        /// Get a single event by id
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="id"></param>
        /// <param name="responseCoordinateSystem"></param>
        /// <param name="exportMode"></param>
        /// <returns></returns>
        Task<IActionResult> GetEventByIdAsync(int? roleId, string authorizationApplicationIdentifier, string id, CoordinateSys responseCoordinateSystem, DsExportMode exportMode);

        /// <summary>
        /// Search for events
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="responseCoordinateSystem"></param>
        /// <param name="exportMode"></param>
        /// <returns></returns>
        Task<IActionResult> GetEventsBySearchAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilterBaseDto filter, bool validateSearchFilter, int skip, int take, CoordinateSys responseCoordinateSystem, DsExportMode exportMode);

        /// <summary>
        /// Get a occurrence by id
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="id"></param>
        /// <param name="responseCoordinateSystem"></param>
        /// <param name="exportMode"></param>
        /// <returns></returns>
        Task<IActionResult> GetOccurrenceByIdAsync(int? roleId, string authorizationApplicationIdentifier, string id, CoordinateSys responseCoordinateSystem, DsExportMode exportMode);

        /// <summary>
        /// Search for occurrences
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="responseCoordinateSystem"></param>
        /// <param name="exportMode"></param>
        /// <returns></returns>
        Task<IActionResult> GetOccurrencesBySearchAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilterBaseDto filter, bool validateSearchFilter, int skip, int take, CoordinateSys responseCoordinateSystem, DsExportMode exportMode);
    }
}
