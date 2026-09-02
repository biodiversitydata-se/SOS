namespace SOS.Api.Contracts.Exports.Requests;

/// <summary>
/// File format requested from an SOS export endpoint.
/// </summary>
public enum SosExportType
{
    /// <summary>Comma-separated values.</summary>
    Csv,
    /// <summary>Darwin Core archive.</summary>
    DwC,
    /// <summary>Microsoft Excel workbook.</summary>
    Excel,
    /// <summary>GeoJSON feature collection.</summary>
    GeoJson
}
