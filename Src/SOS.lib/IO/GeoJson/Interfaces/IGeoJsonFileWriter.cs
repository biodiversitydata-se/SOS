using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;

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
        /// <param name="outputFieldSet"></param>
        /// <returns></returns>
        /// <param name="propertyLabelType"></param>
        /// <param name="excludeNullValues"></param>
        /// <param name="gzip"></param>
        /// <param name="cancellationToken"></param>
        Task<string> CreateFileAync(SearchFilter filter, 
            string exportPath,
            string fileName, 
            string culture, 
            bool flatOut,
            OutputFieldSet outputFieldSet, 
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            bool gzip,
            IJobCancellationToken cancellationToken);
    }
}
