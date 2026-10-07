using SOS.Api.Contracts.Exports.Requests;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Requests;

/// <summary>Options shared by synchronous and queued observation exports.</summary>
public class ExportDownloadOptions : SosRoleRequestOptions
{
    /// <summary>
    /// The fallback field set for non-Darwin Core exports. A field set specified by the search filter
    /// takes precedence.
    /// </summary>
    public OutputFieldSet OutputFieldSet { get; set; } = OutputFieldSet.None;
    /// <summary>Controls whether the API validates the search filter.</summary>
    public bool ValidateSearchFilter { get; set; }
    /// <summary>How CSV, Excel, and non-flat GeoJSON properties are labelled.</summary>
    public PropertyLabelType PropertyLabelType { get; set; } = PropertyLabelType.PropertyName;
    /// <summary>The culture used for translated labels in non-Darwin Core exports.</summary>
    public string CultureCode { get; set; } = "sv-SE";
    /// <summary>Controls whether a Darwin Core export is event-based.</summary>
    public bool EventBased { get; set; }
    /// <summary>Controls whether dynamic project fields are included in an Excel export.</summary>
    public bool DynamicProjectDataFields { get; set; }
    /// <summary>
    /// Controls whether CSV, Excel, and GeoJSON responses are returned as ZIP archives. Darwin Core
    /// responses are always ZIP archives.
    /// </summary>
    public bool Gzip { get; set; } = true;
    /// <summary>Controls whether the sensitive-observation index is exported.</summary>
    public bool SensitiveObservations { get; set; }
    /// <summary>Controls whether a GeoJSON export uses a flat property structure.</summary>
    public bool Flat { get; set; } = true;
    /// <summary>Controls whether null-valued properties are omitted from a flat GeoJSON export.</summary>
    public bool ExcludeNullValues { get; set; }
}
