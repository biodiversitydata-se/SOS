using SOS.Observations.Api.Services.AdaptiveSearch;

namespace SOS.Observations.Api.Services.Interfaces;

public interface IAdaptiveSearchService
{
    Task<AdaptiveSearchResult> SearchAsync(AdaptiveSearchRequest request);
}
