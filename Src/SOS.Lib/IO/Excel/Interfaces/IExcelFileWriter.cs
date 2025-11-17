using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;
using System.IO;
using System.Threading.Tasks;

namespace SOS.Lib.IO.Excel.Interfaces;

public interface IExcelFileWriter
{
    /// <summary>
    ///  Create export file
    /// </summary>
    /// <param name="filter"></param>
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
        string exportPath,
        string fileName,
        string culture,
        PropertyLabelType propertyLabelType,
        bool dynamicProjectDataFields,
        bool gzip,
        IJobCancellationToken cancellationToken);

    Task<(Stream stream, string filename)> CreateFileInMemoryAsync(
        SearchFilter filter,
        string culture,
        PropertyLabelType propertyLabelType,
        bool dynamicProjectDataFields,
        bool gzip,
        IJobCancellationToken cancellationToken);
}
