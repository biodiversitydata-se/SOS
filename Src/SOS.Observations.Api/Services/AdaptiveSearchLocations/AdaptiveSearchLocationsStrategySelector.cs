using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

internal static class AdaptiveSearchLocationsStrategySelector
{
    public static AdaptiveSearchLocationsStrategy Resolve(
        AdaptiveSearchLocationsRequest request,
        SearchFilterInternal filter)
    {
        if (request.AdaptiveSearchStrategy != AdaptiveSearchLocationsStrategy.Auto)
        {
            return request.AdaptiveSearchStrategy;
        }

        return !request.CreateSampleObservations &&
               CanDetermineRepresentativePresentationStatus(filter, request.CutoffYear)
            ? AdaptiveSearchLocationsStrategy.LocationsThenGrid
            : AdaptiveSearchLocationsStrategy.ObservationsThenGrid;
    }

    public static bool CanDetermineRepresentativePresentationStatus(SearchFilterInternal filter, int? cutoffYear)
    {
        if (cutoffYear.HasValue)
        {
            return false;
        }

        if (filter.PositiveSightings == true)
        {
            // PresentSensitive depends on every observation being sensitive.
            return filter.ExtendedAuthorization.ProtectionFilter is ProtectionFilter.Public or ProtectionFilter.Sensitive;
        }

        if (filter.PositiveSightings != false)
        {
            return false;
        }

        if (filter.InvasiveSpeciesTreatmentIds?.Any() == true &&
            filter.InvasiveSpeciesTreatmentIds.All(id => id >= 0))
        {
            return true;
        }

        return filter.HasInvasiveSpeciesTreatment == false &&
               filter.NotRecoveredFilter is SightingNotRecoveredFilter.OnlyNotRecovered or
                   SightingNotRecoveredFilter.DontIncludeNotRecovered;
    }
}
