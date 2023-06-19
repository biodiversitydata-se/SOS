using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using SOS.Lib.Enums;
using SOS.Lib.HangfireAttributes;
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
        /// <param name="encryptedPassword"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobDisplayName("Export observations. Email={3}, Description={4}, ExportFormat={5}")]
        [AutomaticRetry(Attempts = 2, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        [Queue("low")]
        [PreserveOriginalQueue]
        [OneAtTheTime(MaxAttempts = 0, RetryInSeconds = 60)]
        [HandleFileExportFailure]
        Task<bool> RunAsync(SearchFilter filter,
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
            string encryptedPassword,
            PerformContext context,
            IJobCancellationToken cancellationToken);
    }
}