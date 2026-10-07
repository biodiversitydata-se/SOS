using SOS.Api.Contracts.Observations.Requests;
using Hangfire;
using SOS.Api.Contracts.Exports.Responses;
using SOS.Api.Contracts.Exports.Requests;

namespace SOS.Observations.Api.Managers.Interfaces;

/// <summary>
/// Export manager interface
/// </summary>
public interface IExportManager
{
    /// <summary>
    /// Asynchronously creates an export file in memory as a ZIP stream, applying the specified filters, formatting, and
    /// export options.
    /// </summary>
    /// <remarks>The returned stream must be disposed by the caller after use. The export operation applies
    /// all provided filters and formatting options before packaging the data as a ZIP file. If GZip compression is
    /// enabled, the ZIP stream is further compressed.</remarks>
    /// <param name="userId">The optional user identifier used to filter the exported data. If null, data for all users is included.</param>
    /// <param name="roleId">The optional role identifier used to filter the exported data by user role. If null, data for all roles is
    /// included.</param>
    /// <param name="authorizationApplicationIdentifier">The identifier of the application requesting the export, used to authorize the operation.</param>
    /// <param name="searchFilter">An object containing search criteria to filter the data included in the export.</param>
    /// <param name="protectionFilter">An object specifying protection criteria to apply to the exported data.</param>
    /// <param name="exportFormat">The format in which to structure the exported data, such as CSV or JSON.</param>
    /// <param name="culture">The culture code to use for formatting data in the export, such as 'en-US'.</param>
    /// <param name="flatOut">true to export data in a flat structure; otherwise, false to use a hierarchical structure.</param>
    /// <param name="propertyLabelType">Specifies the type of property labels to use in the export output.</param>
    /// <param name="excludeNullValues">true to exclude properties with null values from the export; otherwise, false.</param>
    /// <param name="dynamicProjectDataFields">true to include dynamic project data fields in the export; otherwise, false.</param>
    /// <param name="gzip">true to apply GZip compression to the resulting ZIP stream; otherwise, false.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the export operation.</param>
    /// <param name="useSearchAfterWithoutPit">Use search_after without a PIT snapshot. False preserves existing paging.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the ZIP stream of the
    /// exported data and the generated filename.</returns>
    Task<(Stream stream, string filename)> CreateExportFileInMemoryAsZipStreamAsync(
        int? userId,
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilterBaseDto searchFilter,
        ProtectionFilterDto? protectionFilter,
        ExportFormat exportFormat,
        string culture,
        bool flatOut,
        PropertyLabelType propertyLabelType,
        bool excludeNullValues,
        bool dynamicProjectDataFields,
        bool gzip,
        IJobCancellationToken cancellationToken,
        bool useSearchAfterWithoutPit = false);

    /// <summary>
    /// Asynchronously generates a Markdown representation of the specified search filter.
    /// </summary>
    /// <param name="filter">The search filter to be converted to Markdown format. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a stream with the generated Markdown
    /// content.</returns>
    Task<MemoryStream> CreateFilterMarkdownAsync(SearchFilterBaseDto filter, CancellationToken cancellationToken);
}
