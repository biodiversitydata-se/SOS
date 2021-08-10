using System.Threading.Tasks;
using Hangfire;
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> CreateFileAync(SearchFilter filter, string exportPath, string fileName,
            IJobCancellationToken cancellationToken);
    }
}
