using NetTopologySuite.Geometries;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Dtos;
using SOS.Lib.Dtos.Location;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Managers.Interfaces;

/// <summary>
///     Observation manager interface
/// </summary>
public interface ILocationManager
{
    /// <summary>
    /// Get locations by id
    /// </summary>
    /// <param name="locationIds"></param>
    /// <returns></returns>
    Task<IEnumerable<LocationDto>> GetLocationsAsync(IEnumerable<string> locationIds);

    /// <summary>
    /// Get point-with-buffer geometry by location id.
    /// </summary>
    /// <param name="locationIds"></param>
    /// <returns></returns>
    Task<Dictionary<string, Geometry>> GetLocationGeometryAsync(HashSet<string> locationIds);

    /// <summary>
    /// Search for locations
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="authorizationApplicationIdentifier"></param>
    /// <param name="filter"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    Task<IEnumerable<LocationSearchResultDto>> SearchAsync(int? roleId, string authorizationApplicationIdentifier,
        SearchFilter filter, int skip, int take);
}