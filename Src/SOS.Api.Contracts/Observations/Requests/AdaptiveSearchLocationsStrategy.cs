namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>Controls when adaptive location search returns observations, representative locations or grid cells.</summary>
public enum AdaptiveSearchLocationsStrategy
{
    /// <summary>Return individual observations below the observation limit; otherwise return grid cells.</summary>
    ObservationsThenGrid = 1,
    /// <summary>Try individual observations, then representative locations, then grid cells as the result limits are exceeded.</summary>
    ObservationsThenLocationsThenGrid = 2,
    /// <summary>Try representative locations first; otherwise return grid cells.</summary>
    LocationsThenGrid = 3
}
