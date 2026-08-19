namespace SOS.Api.Contracts.Exports.Responses;

/// <summary>
/// Format of a queued export job.
/// </summary>
public enum ExportFormatDto
{
    /// <summary>Comma-separated values.</summary>
    Csv,
    /// <summary>Darwin Core archive.</summary>
    DwC,
    /// <summary>Event-based Darwin Core archive.</summary>
    DwCEvent,
    /// <summary>GeoJSON feature collection.</summary>
    GeoJson,
    /// <summary>Microsoft Excel workbook.</summary>
    Excel,
    /// <summary>Area of occupancy and extent of occurrence result.</summary>
    AooEoo,
    /// <summary>Article 17 area of occupancy and extent of occurrence result.</summary>
    AooEooArticle17
}
