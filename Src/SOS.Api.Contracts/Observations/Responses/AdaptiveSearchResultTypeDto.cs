namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>The result representation selected by adaptive search.</summary>
public enum AdaptiveSearchResultTypeDto
{
    /// <summary>Individual observations.</summary>
    Observations = 0,

    /// <summary>Geographic grid cells.</summary>
    GeoGrid = 1
}
