using SOS.Api.Contracts.Areas.Responses;

namespace SOS.Api.Contracts.Areas.Requests;
/// <summary>
/// Area filter.
/// </summary>
public class AreaFilterDto
{

    /// <summary>Type of area to search.</summary>
    public AreaTypeDto AreaType { get; set; }

    /// <summary>
    /// Feature
    /// </summary>
    public string FeatureId { get; set; }

}


