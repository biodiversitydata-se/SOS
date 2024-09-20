using Hangfire;
using Hangfire.Server;
using SOS.Lib.Enums;
using SOS.Lib.HangfireAttributes;
using SOS.Lib.Models.Search.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// <param name="dynamicProjectDataFields"></param>
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
            bool dynamicProjectDataFields,
            PerformContext context,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Export AOO EOO.
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <param name="useCenterPoint"></param>
        /// <param name="alphaValues"></param>
        /// <param name="useEdgeLengthRatio"></param>
        /// <param name="allowHoles"></param>
        /// <param name="returnGridCells"></param>
        /// <param name="includeEmptyCells"></param>
        /// <param name="metricCoordinateSys"></param>
        /// <param name="coordinateSystem"></param>
        /// <param name="emailAddress"></param>
        /// <param name="description"></param>
        /// <param name="exportFormat"></param>
        /// <param name="sendMailFromZendTo"></param>
        /// <param name="encryptedPassword"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobDisplayName("Export AOO EOO. Email={12}, Description={13}")]
        [AutomaticRetry(Attempts = 2, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        [Queue("low")]
        [PreserveOriginalQueue]
        [OneAtTheTime(MaxAttempts = 0, RetryInSeconds = 60)]
        [HandleFileExportFailure]
        Task<bool> RunAooEooAsync(
           int? roleId,
           string authorizationApplicationIdentifier,
           SearchFilter filter,
           int gridCellsInMeters,
           bool useCenterPoint,
           IEnumerable<double> alphaValues,
           bool useEdgeLengthRatio,
           bool allowHoles,
           bool returnGridCells,
           bool includeEmptyCells,
           MetricCoordinateSys metricCoordinateSys,
           CoordinateSys coordinateSystem,
           string emailAddress,
           string description,
           ExportFormat exportFormat,           
           bool sendMailFromZendTo,
           string encryptedPassword,
           PerformContext context,
           IJobCancellationToken cancellationToken);

        /// <summary>
        /// Export AOO EOO Article 17.
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <param name="maxDistance"></param>
        /// <param name="metricCoordinateSys"></param>
        /// <param name="coordinateSystem"></param>
        /// <param name="emailAddress"></param>
        /// <param name="description"></param>
        /// <param name="exportFormat"></param>
        /// <param name="sendMailFromZendTo"></param>
        /// <param name="encryptedPassword"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobDisplayName("Export AOO EOO Article 17. Email={7}, Description={8}")]
        [AutomaticRetry(Attempts = 2, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        [Queue("low")]
        [PreserveOriginalQueue]
        [OneAtTheTime(MaxAttempts = 0, RetryInSeconds = 60)]
        [HandleFileExportFailure]
        Task<bool> RunAooEooArticle17Async(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int gridCellsInMeters,
            int maxDistance,
            MetricCoordinateSys metricCoordinateSys,
            CoordinateSys coordinateSystem,
            string emailAddress,
            string description,
            ExportFormat exportFormat,            
            bool sendMailFromZendTo,
            string encryptedPassword,
            PerformContext context,
            IJobCancellationToken cancellationToken);
    }
}