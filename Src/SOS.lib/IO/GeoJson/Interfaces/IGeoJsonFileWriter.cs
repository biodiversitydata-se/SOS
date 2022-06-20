using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Lib.IO.GeoJson.Interfaces
{
    public interface IGeoJsonFileWriter
    {
        /// <summary>
        /// Create export file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="exportPath"></param>
        /// <param name="fileName"></param>
        /// <param name="culture"></param>
        /// <param name="flatOut"></param>
        /// <param name="propertyLabelType"></param>
        /// <param name="excludeNullValues"></param>
        /// <param name="gzip"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FileExportResult> CreateFileAync(SearchFilter filter, 
            string exportPath,
            string fileName, 
            string culture, 
            bool flatOut,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            bool gzip,
            IJobCancellationToken cancellationToken);
    }
}
