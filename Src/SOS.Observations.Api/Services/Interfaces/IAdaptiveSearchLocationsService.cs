using SOS.Observations.Api.Services.AdaptiveSearchLocations;

namespace SOS.Observations.Api.Services.Interfaces;

public interface IAdaptiveSearchLocationsService
{
    Task<AdaptiveSearchLocationsResult> SearchAsync(AdaptiveSearchLocationsRequest request);
}
