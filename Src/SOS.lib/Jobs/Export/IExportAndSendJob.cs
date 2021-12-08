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
        /// <param name="outputFieldSet"></param>
        /// <param name="propertyLabelType"></param>
        /// <param name="excludeNullValues"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisplayName("Export observations. Email={2}, Description={3}, ExportFormat={4}")]        
        [AutomaticRetry(Attempts = 2, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        [Queue("medium")]
        Task<bool> RunAsync(SearchFilter filter,
            int userId,
            string email,
            string description,
            ExportFormat exportFormat,
            string culture,
            bool flatOut,
            OutputFieldSet outputFieldSet,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            PerformContext context,
            IJobCancellationToken cancellationToken);
    }
}