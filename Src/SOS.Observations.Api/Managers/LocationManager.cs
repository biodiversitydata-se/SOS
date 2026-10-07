using SOS.Api.Contracts.Locations.Responses;
using Microsoft.Extensions.Logging;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Managers;

/// <summary>
///     Observation manager class
/// </summary>
public class LocationManager : ILocationManager
{
    private readonly IProcessedLocationRepository _processedLocationRepository;
    private readonly IFilterManager _filterManager;
    private readonly ILogger<LocationManager> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="processedLocationRepository"></param>
    /// <param name="filterManager"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public LocationManager(
        IProcessedLocationRepository processedLocationRepository,
        IFilterManager filterManager,
        ILogger<LocationManager> logger)
    {
        _processedLocationRepository = processedLocationRepository ??
                                          throw new ArgumentNullException(nameof(processedLocationRepository));
        _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Make sure we are working with live data
        _processedLocationRepository.LiveMode = true;
    }


    /// <inheritdoc />
    public Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<string> locationIds) =>
        _processedLocationRepository.GetLocationsAsync(locationIds);

    /// <inheritdoc />
    public async Task<Dictionary<string, NetTopologySuite.Geometries.Geometry>> GetLocationGeometryAsync(HashSet<string> locationIds)
    {
        return await _processedLocationRepository.GetLocationGeometryAsync(locationIds);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LocationSearchResultDto>> SearchAsync(int? roleId, string authorizationApplicationIdentifier,
        SearchFilter filter, int skip, int take)
    {
        try
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            var result = await _processedLocationRepository.SearchAsync(filter, skip, take);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to search for locations.");
            throw;
        }
    }
}
