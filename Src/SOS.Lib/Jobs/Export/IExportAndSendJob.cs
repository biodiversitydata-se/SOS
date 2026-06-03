using Hangfire;
using Hangfire.Server;
using SOS.Lib.Dtos.Enum;
using SOS.Lib.Dtos.Filter;
using SOS.Lib.Enums;
using SOS.Lib.HangfireAttributes;

namespace SOS.Lib.Jobs.Export;

/// <summary>
///     Interface for DOI export job
/// </summary>
public interface IExportAndSendJob
{
    /// <summary>
    /// Create a county occurrence report and send it to user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="roleId"></param>
    /// <param name="authorizationApplicationIdentifier"></param>
    /// <param name="email"></param>
    /// <param name="description"></param>
    /// <param name="encryptedPassword"></param>
    /// <param name="context"></param>
    /// <param name="taxonIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [JobDisplayName("County occurrence report. Email={3}, Description={4}")]
    [AutomaticRetry(Attempts = 2, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    [Queue("low")]
    [PreserveOriginalQueue]
    [OneAtTheTime(MaxAttempts = 0, RetryInSeconds = 60)]
    [HandleFileExportFailure]
    Task<bool> CreateAndSendCountyOccurrenceReportAsync(
        int userId,
        int? roleId,
        string authorizationApplicationIdentifier,
        string email,
        string description,
        string encryptedPassword,
        PerformContext context,
        IEnumerable<int> taxonIds,
        IJobCancellationToken cancellationToken);

    /// <summary>
    /// Exports observations that match the specified search criteria and parameters, and optionally sends an email
    /// notification upon completion.
    /// </summary>
    /// <param name="searchFilter">The filter criteria used to select which observations to export. Cannot be null.</param>
    /// <param name="protectionFilter">The filter criteria used to determine the protection status of observations. Cannot be null.</param>
    /// <param name="userId">The identifier of the user initiating the export operation. Must be a valid user ID.</param>
    /// <param name="roleId">The identifier of the role associated with the user. Specify null if no specific role is required.</param>
    /// <param name="authorizationApplicationIdentifier">The identifier of the application requesting the export, used for authorization purposes.</param>
    /// <param name="email">The email address to which a notification will be sent upon completion of the export.</param>
    /// <param name="description">A description of the export task, providing additional context for the operation.</param>
    /// <param name="exportFormat">The format in which the observations will be exported, such as CSV or Excel.</param>
    /// <param name="culture">The culture code to use for formatting exported data, such as 'en-US'.</param>
    /// <param name="flatOut">true to export data in a flat structure; otherwise, false to use a hierarchical structure.</param>
    /// <param name="propertyLabelType">Specifies the type of property labels to include in the export.</param>
    /// <param name="excludeNullValues">true to exclude properties with null values from the export; otherwise, false.</param>
    /// <param name="sensitiveObservations">true to include sensitive observations in the export; otherwise, false.</param>
    /// <param name="sendMailFromZendTo">true to send the notification email from the ZendTo system; otherwise, false.</param>
    /// <param name="encryptedPassword">The encrypted password used for securing the exported file, if applicable.</param>
    /// <param name="dynamicProjectDataFields">true to include dynamic project data fields in the export; otherwise, false.</param>
    /// <param name="context">The job execution context provided by the background job processor.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the export operation.</param>
    /// <returns>true if the export operation completes successfully; otherwise, false.</returns>
    [JobDisplayName("Export observations. Email={5}, Description={6}, ExportFormat={7}")]
    [AutomaticRetry(Attempts = 2, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    [Queue("low")]
    [PreserveOriginalQueue]
    [OneAtTheTime(MaxAttempts = 0, RetryInSeconds = 60)]
    [HandleFileExportFailure]
    Task<bool> RunAsync(
        SearchFilterBaseDto searchFilter,
        ProtectionFilterDto protectionFilter,
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
        string encryptedPassword,
        bool dynamicProjectDataFields,
        PerformContext context,
        IJobCancellationToken cancellationToken);

    /// <summary>
    /// Exports Area of Occupancy (AOO) and Extent of Occurrence (EOO) data based on the specified parameters and sends
    /// an email notification upon completion.
    /// </summary>
    /// <remarks>This method is intended to be executed as a background job. It supports cancellation and
    /// automatic retry on failure. The export process may be influenced by user role, filter criteria, and various
    /// calculation options provided as parameters.</remarks>
    /// <param name="userId">The unique identifier of the user initiating the export operation.</param>
    /// <param name="roleId">An optional identifier for the user's role, which may influence export permissions or behavior. Specify null if
    /// not applicable.</param>
    /// <param name="authorizationApplicationIdentifier">The identifier of the application requesting authorization for the export process.</param>
    /// <param name="filter">The search filter criteria to apply when selecting data for export.</param>
    /// <param name="protectionFilter">The protection filter criteria to apply when determining which data to include in the export.</param>
    /// <param name="gridCellsInMeters">The size, in meters, of the grid cells used for calculating AOO and EOO.</param>
    /// <param name="useCenterPoint">true to use the center point for calculations; otherwise, false.</param>
    /// <param name="alphaValues">A collection of alpha values used to influence the EOO calculation.</param>
    /// <param name="useEdgeLengthRatio">true to apply the edge length ratio in the calculation; otherwise, false.</param>
    /// <param name="allowHoles">true to allow holes in the calculated area; otherwise, false.</param>
    /// <param name="returnGridCells">true to include the grid cell geometries in the export; otherwise, false.</param>
    /// <param name="includeEmptyCells">true to include empty grid cells in the export; otherwise, false.</param>
    /// <param name="metricCoordinateSys">The metric coordinate system to use for spatial calculations.</param>
    /// <param name="coordinateSystem">The coordinate system to use for the exported data.</param>
    /// <param name="emailAddress">The email address to which the export completion notification will be sent.</param>
    /// <param name="description">A description of the export job, included in the notification.</param>
    /// <param name="exportFormat">The format in which to export the data.</param>
    /// <param name="sendMailFromZendTo">true to send the notification email from the ZendTo system; otherwise, false.</param>
    /// <param name="encryptedPassword">The encrypted password used for securing the exported file, if applicable.</param>
    /// <param name="context">The job execution context provided by the background processing framework.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the export operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the export and notification were successful; otherwise, false.</returns>
    [JobDisplayName("Export AOO EOO. Email={14}, Description={15}, ExportFormat={16}")]
    [AutomaticRetry(Attempts = 2, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    [Queue("low")]
    [PreserveOriginalQueue]
    [OneAtTheTime(MaxAttempts = 0, RetryInSeconds = 60)]
    [HandleFileExportFailure]
    Task<bool> RunAooEooAsync(
       int userId,
       int? roleId,
       string authorizationApplicationIdentifier,
       SearchFilterAggregationInternalDto filter,
       ProtectionFilterDto protectionFilter,
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
    /// Exports AOO and EOO data for Article 17 reporting, applying the specified filters and spatial parameters, and
    /// optionally sends an email notification upon completion.
    /// </summary>
    /// <param name="userId">The unique identifier of the user initiating the export operation.</param>
    /// <param name="roleId">An optional identifier representing the user's role, which may affect access permissions and data visibility.</param>
    /// <param name="authorizationApplicationIdentifier">A string that identifies the application requesting authorization for the export process.</param>
    /// <param name="filter">An object containing search filters that determine which data is included in the export.</param>
    /// <param name="protectionFilter">An object containing protection filters that determine the protection status of the data included in the export.</param>
    /// <param name="gridCellsInMeters">The size, in meters, of the grid cells used for spatial calculations during the export.</param>
    /// <param name="maxDistance">The maximum distance, in meters, to consider when filtering spatial data for the export.</param>
    /// <param name="metricCoordinateSys">The metric coordinate system used for spatial calculations in the export.</param>
    /// <param name="coordinateSystem">The coordinate system in which the exported data will be represented.</param>
    /// <param name="emailAddress">The email address to which a notification will be sent upon completion of the export, if email notifications are
    /// enabled.</param>
    /// <param name="description">A description of the export operation, used for logging or notification purposes.</param>
    /// <param name="exportFormat">The format in which the exported data will be generated.</param>
    /// <param name="sendMailFromZendTo">true to send the notification email using the ZendTo system; otherwise, false.</param>
    /// <param name="encryptedPassword">An encrypted password used for authentication or securing the exported data, if required.</param>
    /// <param name="context">The job execution context, providing access to background job information and logging.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the export operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the export and optional email
    /// notification complete successfully; otherwise, false.</returns>
    [JobDisplayName("Export AOO EOO Article 17. Email={9}, Description={10}, ExportFormat={11}")]
    [AutomaticRetry(Attempts = 2, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    [Queue("low")]
    [PreserveOriginalQueue]
    [OneAtTheTime(MaxAttempts = 0, RetryInSeconds = 60)]
    [HandleFileExportFailure]
    Task<bool> RunAooEooArticle17Async(
        int userId,
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilterBaseDto filter,
        ProtectionFilterDto protectionFilter,
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
