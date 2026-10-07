using SOS.Api.Contracts.Exports.Requests;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Requests;

/// <summary>Options for adaptive observation search.</summary>
public sealed class AdaptiveSearchOptions : SosRoleRequestOptions
{
    /// <summary>The observation count above which grid aggregation is returned.</summary>
    public int ObservationsLimit { get; set; } = 10000;

    /// <summary>The maximum grid zoom level.</summary>
    public int MaxZoom { get; set; } = 21;

    /// <summary>The maximum number of grid cells.</summary>
    public int MaxGridCells { get; set; } = 10000;

    /// <summary>The labels used for properties in GeoJSON responses.</summary>
    public PropertyLabelType GeoJsonPropertyLabelType { get; set; } = PropertyLabelType.PropertyPath;

    /// <summary>The response format.</summary>
    public OutputFormatDto OutputFormat { get; set; } = OutputFormatDto.GeoJsonFlat;

    /// <summary>Controls omission of properties with null values.</summary>
    public bool ExcludeNullValues { get; set; }

    /// <summary>Controls inclusion of areas from the search filter.</summary>
    public bool IncludeFilterAreasInResult { get; set; }

    /// <summary>Controls use of the input bounding box instead of one derived from the result.</summary>
    public bool ReturnInputBBox { get; set; }

    /// <summary>Controls validation of search-filter values.</summary>
    public bool ValidateSearchFilter { get; set; }

    /// <summary>The culture used to translate labels.</summary>
    public string TranslationCultureCode { get; set; } = "sv-SE";

    /// <summary>Controls the use of the sensitive-observation index.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>The polygon rendering mode.</summary>
    public CreatePolygonsMode CreatePolygons { get; set; } = CreatePolygonsMode.No;

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .Add("observationsLimit", ObservationsLimit)
            .Add("maxZoom", MaxZoom)
            .Add("maxGridCells", MaxGridCells)
            .AddEnum("geoJsonPropertyLabelType", GeoJsonPropertyLabelType)
            .AddEnum("outputFormat", OutputFormat)
            .Add("excludeNullValues", ExcludeNullValues)
            .Add("includeFilterAreasInResult", IncludeFilterAreasInResult)
            .Add("returnInputBBox", ReturnInputBBox)
            .Add("validateSearchFilter", ValidateSearchFilter)
            .Add("translationCultureCode", TranslationCultureCode)
            .Add("sensitiveObservations", SensitiveObservations)
            .AddEnum("createPolygons", CreatePolygons)
            .ToString();
    }
}
