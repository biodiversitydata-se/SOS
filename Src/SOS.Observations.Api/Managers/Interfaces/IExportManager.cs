using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    /// Export manager interface
    /// </summary>
    public interface IExportManager
    {
        /// <summary>
        /// Create an export file and return path to the created file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="exportFormat"></param>
        /// <param name="exportPath"></param>
        /// <param name="culture"></param>
        /// <param name="flatOut">Only applicable when GeoJson is selected as export format</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> CreateExportFileAsync(SearchFilter filter,
            ExportFormat exportFormat,
            string exportPath,
            string culture,
            bool flatOut,
            IJobCancellationToken cancellationToken);
    }
}
