using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SOS.Lib.IO.Excel.Interfaces;

/// <summary>
/// Csv file writer
/// </summary>
public interface ICsvFileWriter
{
    /// <summary>
    /// Create count occurrence report
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="authorizationApplicationIdentifier"></param>
    /// <param name="fileName"></param>
    /// <param name="reportPath"></param>
    /// <param name="taxonIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FileExportResult> CreateCountyOccurrenceReportAsync(
        int? roleId,
        string authorizationApplicationIdentifier,
        string fileName,
        string reportPath,
        IEnumerable<int> taxonIds,
        IJobCancellationToken cancellationToken);

    /// <summary>
    ///  Create export file
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="exportPath"></param>
    /// <param name="fileName"></param>
    /// <param name="culture"></param>
    /// <param name="propertyLabelType"></param>
    /// <param name="gzip"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FileExportResult> CreateFileAync(SearchFilter filter,
        string exportPath,
        string fileName,
        string culture,
        PropertyLabelType propertyLabelType,
        bool gzip,
        IJobCancellationToken cancellationToken);

    Task<byte[]> CreateFileInMemoryAsync(SearchFilter filter,            
        string culture,
        PropertyLabelType propertyLabelType,
        bool gzip,
        IJobCancellationToken cancellationToken);

    Task<(Stream stream, string filename)> CreateFileInMemoryAsZipStreamAsync(SearchFilter filter,
       string culture,
       PropertyLabelType propertyLabelType,           
       IJobCancellationToken cancellationToken);
}
