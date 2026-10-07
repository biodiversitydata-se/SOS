using Hangfire;
using SOS.Api.Contracts.Exports.Requests;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Lib.IO.Excel.Interfaces;

public interface IExcelFileWriter
{
    /// <summary>
    ///  Create export file
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="additionalFiles"></param>
    /// <param name="exportPath"></param>
    /// <param name="fileName"></param>
    /// <param name="culture"></param>
    /// <param name="propertyLabelType"></param>
    /// <param name="dynamicProjectDataFields"></param>
    /// <param name="gzip"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FileExportResult> CreateFileAync(
        SearchFilter filter,
        IEnumerable<(Stream Stream, string Filename)> additionalFiles,
        string exportPath,
        string fileName,
        string culture,
        PropertyLabelType propertyLabelType,
        bool dynamicProjectDataFields,
        bool gzip,
        IJobCancellationToken cancellationToken);

    Task<(Stream stream, string filename)> CreateFileInMemoryAsync(
        SearchFilter filter,
        IEnumerable<(Stream Stream, string Filename)> additionalFiles,
        string culture,
        PropertyLabelType propertyLabelType,
        bool dynamicProjectDataFields,
        bool gzip,
        IJobCancellationToken cancellationToken);
}
