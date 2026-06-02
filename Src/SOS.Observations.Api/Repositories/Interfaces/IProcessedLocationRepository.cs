using NetTopologySuite.Geometries;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Observation = SOS.Lib.Models.Processed.Observation.Observation;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Observations.Api.Repositories.Interfaces;

/// <summary>
/// </summary>
public interface IProcessedLocationRepository : IProcessRepositoryBase<Observation, string>
{
    /// <summary>
    /// Get locations by id
    /// </summary>
    /// <param name="locationIds"></param>
    /// <returns></returns>
    Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<string> locationIds);

    /// <summary>
    /// Get point-with-buffer geometry by location id.
    /// </summary>
    /// <param name="locationIds"></param>
    /// <returns></returns>
    Task<Dictionary<string, Geometry>> GetLocationGeometryAsync(HashSet<string> locationIds);

    /// <summary>
    /// Search for locations
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    Task<IEnumerable<LocationSearchResult>> SearchAsync(SearchFilter filter, int skip,
        int take);
}