using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Jobs.Export
{
    /// <summary>
    ///     Interface for DOI export job
    /// </summary>
    public interface IExportAndSendJob
    {
        /// <summary>
        /// Run export job
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="userId"></param>
        /// <param name="email"></param>
        /// <param name="description"></param>
        /// <param name="exportFormat"></param>
        /// <param name="culture"></param>
        /// <param name="flatOut"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisplayName("Export observations. Email={1}, Description={2}, ExportFormat={3}")]
        [Queue("medium")]
        Task<bool> RunAsync(SearchFilter filter,
            int userId,
            string email,
            string description,
            ExportFormat exportFormat,
            string culture,
            bool flatOut,
            PerformContext context,
            IJobCancellationToken cancellationToken);
    }
}