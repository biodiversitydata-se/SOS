namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Output format
/// </summary>
public enum OutputFormatDto
{

    /// <summary>JSON observations.</summary>
    Json = 0,

    /// <summary>GeoJSON feature collection.</summary>
    GeoJson = 1,

    /// <summary>Flattened GeoJSON feature collection.</summary>
    GeoJsonFlat = 2,

    /// <summary>GeoJSON grouped by location.</summary>
    GeoJsonByLocation = 3,

    /// <summary>Flattened GeoJSON grouped by location.</summary>
    GeoJsonByLocationFlat = 4,

    /// <summary>JSON grouped by location.</summary>
    JsonByLocation = 5,

}


