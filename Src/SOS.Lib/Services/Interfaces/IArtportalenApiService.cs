using SOS.Lib.Models.ArtportalenApiService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Services.Interfaces;

/// <summary>
///     Interface for Artportalen API
/// </summary>
public interface IArtportalenApiService
{
    /// <summary>
    ///  Get sighting by id.
    /// </summary>
    /// <returns></returns>
    Task<SightingOutput> GetSightingByIdAsync(int sightingId);

    /// <summary>
    /// Get media by sightingId
    /// </summary>
    /// <param name="sightingId"></param>
    /// <returns></returns>
    Task<List<MediaFile>> GetMediaBySightingIdAsync(int sightingId);
}