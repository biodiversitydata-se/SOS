using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Filters;

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
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="email"></param>
        /// <param name="description"></param>
        /// <param name="exportFormat"></param>
        /// <param name="culture"></param>
        /// <param name="flatOut"></param>
        /// <param name="propertyLabelType"></param>
        /// <param name="excludeNullValues"></param>
        /// <param name="sensitiveObservations"></param>
        /// <param name="sendMailFromZendTo"></param>
        /// <param name="encryptPassword"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobDisplayName("Export observations. Email={2}, Description={3}, ExportFormat={4}")]
        [AutomaticRetry(Attempts = 2, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        [Queue("medium")]
        Task<bool> RunAsync(SearchFilter filter,
            int userId,
            int? roleId,
            string authorizationApplicationIdentifier,
            string email,
            string description,
            ExportFormat exportFormat,
            string culture,
            bool flatOut,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            bool sensitiveObservations,
            bool sendMailFromZendTo,
            string encryptPassword,
            PerformContext context,
            IJobCancellationToken cancellationToken);
    }
}