using SOS.Api.Contracts.Common.Geo;
using Hangfire;
using Hangfire.Server;
using SOS.Api.Contracts.Exports.Responses;
using SOS.Api.Contracts.Exports.Requests;
using Microsoft.Extensions.Logging;
using SOS.Export.Managers.Interfaces;
using SOS.Export.Models.ZendTo;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.Export;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Aggregations.Requests;

namespace SOS.Export.Jobs;

/// <summary>
/// Export and send job.
/// </summary>
public class ExportAndSendJob : IExportAndSendJob
{
    private readonly IMarkdownHelper _markdownHelper;
    private readonly ICryptoService _cryptoService;
    private readonly IObservationManager _observationManager;
    private readonly IUserExportRepository _userExportRepository;
    private readonly ILogger<ExportAndSendJob> _logger;
    private readonly TimeProvider _timeProvider;

    private Task WaitForJobInfoAsync() => Task.Delay(TimeSpan.FromSeconds(1), _timeProvider);

    /// <summary>
    /// Remove expired and old jobs
    /// </summary>
    /// <param name="userExport"></param>
    private void RemoveExpiredJobs(UserExport userExport)
    {
        if (userExport == null) return;
        userExport.Jobs = userExport.Jobs.Where(j =>
            !(
                (j.Status.Equals(ExportJobStatus.Succeeded) && DateTime.Now.ToUniversalTime() > (j.ProcessEndDate.HasValue ? j.ProcessEndDate.Value.AddDays(j.LifetimeDays) : null)) || // Remove succeded jobs where expire date has passed
                (j.ProcessStartDate.HasValue && DateTime.Now.ToUniversalTime() > j.ProcessStartDate.Value.AddMonths(1)) // Remove jobs older than one month
            )
        )?.ToList();
    }

    private async Task UpdateJobInfoStartProcessing(int userId, string jobId)
    {
        try
        {
            var userExport = await _userExportRepository.GetAsync(userId);
            RemoveExpiredJobs(userExport);

            var jobInfo = userExport?.Jobs?.FirstOrDefault(m => m.Id == jobId);
            if (jobInfo == null) return;

            jobInfo.ProcessStartDate = DateTime.UtcNow;
            jobInfo.Status = ExportJobStatus.Processing;

            await _userExportRepository.UpdateAsync(userId, userExport);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Couldn't update job info");
        }
    }

    private async Task UpdateJobInfoEndProcessing(int userId, string jobId, ZendToResponse response)
    {
        try
        {
            var userExport = await _userExportRepository.GetAsync(userId);
            var jobInfo = userExport?.Jobs?.FirstOrDefault(m => m.Id == jobId);
            if (jobInfo == null) return;
            jobInfo.ProcessEndDate = DateTime.UtcNow;
            jobInfo.Status = ExportJobStatus.Succeeded;
            jobInfo.LifetimeDays = response.LifetimeDays;
            jobInfo.PickUpUrl = response.Recipients?.FirstOrDefault()?.Link;
            await _userExportRepository.UpdateAsync(userId, userExport);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Couldn't update job info");
        }
    }

    private async Task UpdateJobInfoError(int userId, string jobId, string errorMsg)
    {
        try
        {
            var userExport = await _userExportRepository.GetAsync(userId);
            var jobInfo = userExport.Jobs.FirstOrDefault(m => m.Id == jobId);
            if (jobInfo == null) return;
            jobInfo.ProcessEndDate = DateTime.UtcNow;
            jobInfo.Status = ExportJobStatus.Failed;
            jobInfo.ErrorMsg = errorMsg;
            await _userExportRepository.UpdateAsync(userId, userExport);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Couldn't update job info");
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="observationManager"></param>
    /// <param name="userExportRepository"></param>
    /// <param name="cryptoService"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ExportAndSendJob(IObservationManager observationManager, IUserExportRepository userExportRepository, ICryptoService cryptoService, IMarkdownHelper filterHelper, ILogger<ExportAndSendJob> logger)
        : this(observationManager, userExportRepository, cryptoService, filterHelper, logger, TimeProvider.System)
    {
    }

    public ExportAndSendJob(IObservationManager observationManager, IUserExportRepository userExportRepository, ICryptoService cryptoService, IMarkdownHelper filterHelper, ILogger<ExportAndSendJob> logger, TimeProvider timeProvider)
    {
        _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        _userExportRepository =
            userExportRepository ?? throw new ArgumentNullException(nameof(userExportRepository));
        _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
        _markdownHelper = filterHelper ?? throw new ArgumentNullException(nameof(filterHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc />
    public async Task<bool> CreateAndSendCountyOccurrenceReportAsync(
        int userId,
        int? roleId,
        string authorizationApplicationIdentifier,
        string email,
        string description,
        string encryptedPassword,
        PerformContext context,
        IEnumerable<int> taxonIds,
        IJobCancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Start creating county occurrence report send job");
            await WaitForJobInfoAsync(); // wait for job info to be inserted in MongoDb.
            await UpdateJobInfoStartProcessing(userId, context?.BackgroundJob?.Id);
            var password = await _cryptoService.DecryptAsync(encryptedPassword);
            var response = await _observationManager.CreateAndSendCountyOccurrenceReportAsync(
                roleId,
                authorizationApplicationIdentifier,
                email,
                description,
                password,
                context,
                taxonIds,
                cancellationToken);
            _logger.LogInformation($"End county occurrence report send job. Success: {response.Success}");
            await UpdateJobInfoEndProcessing(userId, context?.BackgroundJob?.Id, response);
            return response.Success ? true : throw new Exception("County occurrence report send job failed");
        }
        catch (JobAbortedException)
        {
            await UpdateJobInfoError(userId, context?.BackgroundJob?.Id, "County occurrence report send job was cancelled.");
            _logger.LogInformation("County occurrence report send job was cancelled.");
            return false;
        }
        catch (Exception ex)
        {
            await UpdateJobInfoError(userId, context?.BackgroundJob?.Id, ex.Message);
            _logger.LogError(ex, "County occurrence report send job failure.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RunAsync(
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
        bool sendMailFromZendTo,
        string encryptedPassword,
        bool dynamicProjectDataFields,
        PerformContext context,
        IJobCancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Start export and send job");
            var filter = searchFilter.ToSearchFilter(userId, protectionFilter, culture);
            var exportGeometries = filter.Location?.Geometries?.Geometries;
            await WaitForJobInfoAsync(); // wait for job info to be inserted in MongoDb.
            await UpdateJobInfoStartProcessing(userId, context?.BackgroundJob?.Id);
            var password = await _cryptoService.DecryptAsync(encryptedPassword);

            var additionalFiles = new List<(Stream stream, string filename)>();
            var filterMarkdownStream = await _markdownHelper.GetFilterMarkDownAsync(searchFilter, CancellationToken.None);
            additionalFiles.Add((filterMarkdownStream, "filter.md"));
            var datasetsResult = await _markdownHelper.GetDatasetsMarkDownAsync(filter, CancellationToken.None);
            additionalFiles.Add((datasetsResult.FileStream, "datasets.md"));
            var citationMarkdownStream = await _markdownHelper.GetCitationMarkDownAsync(authorizationApplicationIdentifier, datasetsResult.DataProviderIds, CancellationToken.None);
            additionalFiles.Add((citationMarkdownStream, "citation.md"));
            

            if (exportGeometries?.Count > 0)
            {
                var geoJsonStream = _markdownHelper.TryGetFeatureCollection(exportGeometries);
                if (geoJsonStream != null)
                {
                    additionalFiles.Add((geoJsonStream, "geometries.geojson"));
                }
            }
           
            var response = await _observationManager.ExportAndSendAsync(roleId, authorizationApplicationIdentifier, filter, additionalFiles, email, description, exportFormat, culture, flatOut, propertyLabelType, excludeNullValues, protectionFilter != ProtectionFilterDto.Public, sendMailFromZendTo, password, dynamicProjectDataFields, cancellationToken);

            _logger.LogInformation($"End export and send job. Success: {response.Success}");
            await UpdateJobInfoEndProcessing(userId, context?.BackgroundJob?.Id, response);
            return response.Success ? true : throw new Exception("Export and send job failed");
        }
        catch (JobAbortedException)
        {
            await UpdateJobInfoError(userId, context?.BackgroundJob?.Id, "Export and send job was cancelled.");
            _logger.LogInformation("Export and send job was cancelled.");
            return false;
        }
        catch (Exception ex)
        {
            await UpdateJobInfoError(userId, context?.BackgroundJob?.Id, ex.Message);
            _logger.LogError(ex, "Export failure.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RunAooEooAsync(
        int userId,
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilterAggregationInternalDto searchFilter,
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
        IJobCancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Start AOO EOO export and send job");
            var filter = searchFilter.ToSearchFilter(userId, protectionFilter, "en-GB");
            var exportGeometries = filter.Location?.Geometries?.Geometries;
            await WaitForJobInfoAsync(); // wait for job info to be inserted in MongoDb.
            await UpdateJobInfoStartProcessing(userId, context?.BackgroundJob?.Id);
            var password = await _cryptoService.DecryptAsync(encryptedPassword);

            var additionalFiles = new List<(Stream stream, string filename)>();
            using var filterMarkdownStream = await _markdownHelper.GetAnalysisMarkDownAsync(
                searchFilter,
                alphaValues: alphaValues?.ToArray(),
                gridCellSizeInMeters: gridCellsInMeters,
                useCenterPoint: useCenterPoint,
                useEdgeLengthRatio: useEdgeLengthRatio,
                allowHoles: allowHoles,
                returnGridCells: returnGridCells,
                includeEmptyCells: includeEmptyCells,
                metricCoordinateSys: metricCoordinateSys,
                coordinateSystem: coordinateSystem,
                cancellationToken: CancellationToken.None);
            additionalFiles.Add((filterMarkdownStream, "filter.md"));
            var datasetsResult = await _markdownHelper.GetDatasetsMarkDownAsync(filter, CancellationToken.None);
            additionalFiles.Add((datasetsResult.FileStream, "datasets.md"));
            var citationMarkdownStream = await _markdownHelper.GetCitationMarkDownAsync(authorizationApplicationIdentifier, datasetsResult.DataProviderIds, CancellationToken.None);
            additionalFiles.Add((citationMarkdownStream, "citation.md"));
            if (exportGeometries?.Count > 0)
            {
                var geoJsonStream = _markdownHelper.TryGetFeatureCollection(exportGeometries);
                if (geoJsonStream != null)
                {
                    additionalFiles.Add((geoJsonStream, "geometries.geojson"));
                }
            }

            var response = await _observationManager.ExportAooEooAndSendAsync(
                roleId, 
                authorizationApplicationIdentifier, 
                filter,
                additionalFiles,
                gridCellsInMeters,
                useCenterPoint,
                alphaValues,
                useEdgeLengthRatio,
                allowHoles,
                returnGridCells,
                includeEmptyCells,
                metricCoordinateSys,
                coordinateSystem,                    
                emailAddress,
                description,
                exportFormat,
                sendMailFromZendTo,
                password,
                cancellationToken);
            _logger.LogInformation($"End AOO EOO export and send job. Success: {response.Success}");
            await UpdateJobInfoEndProcessing(userId, context?.BackgroundJob?.Id, response);
            return response.Success ? true : throw new Exception("AOO EOO export and send job failed");
        }
        catch (JobAbortedException)
        {
            await UpdateJobInfoError(userId, context?.BackgroundJob?.Id, "AOO EOO export and send job was cancelled.");
            _logger.LogInformation("AOO EOO export and send job was cancelled.");
            return false;
        }
        catch (Exception ex)
        {
            await UpdateJobInfoError(userId, context?.BackgroundJob?.Id, ex.Message);
            _logger.LogError(ex, "AOO EOO export failure.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RunAooEooArticle17Async(
        int userId,
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilterBaseDto searchFilter,
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
        IJobCancellationToken cancellationToken)
    {


        try
        {
            _logger.LogInformation("Start AOO EOO Article 17 export and send job");
            var filter = searchFilter.ToSearchFilter(userId, protectionFilter, "en-GB");
            var exportGeometries = filter.Location?.Geometries?.Geometries;
            await WaitForJobInfoAsync(); // wait for job info to be inserted in MongoDb.
            await UpdateJobInfoStartProcessing(userId, context?.BackgroundJob?.Id);
            var password = await _cryptoService.DecryptAsync(encryptedPassword);

            var additionalFiles = new List<(Stream stream, string filename)>();
            using var filterMarkdownStream = await _markdownHelper.GetAnalysisMarkDownAsync(
                 searchFilter,
                 maxDistance: maxDistance,
                 gridCellSizeInMeters: gridCellsInMeters,
                 metricCoordinateSys: metricCoordinateSys,
                 coordinateSystem: coordinateSystem,
                 cancellationToken: CancellationToken.None);
            additionalFiles.Add((filterMarkdownStream, "filter.md"));
            var datasetsResult = await _markdownHelper.GetDatasetsMarkDownAsync(filter, CancellationToken.None);
            additionalFiles.Add((datasetsResult.FileStream, "datasets.md"));
            var citationMarkdownStream = await _markdownHelper.GetCitationMarkDownAsync(authorizationApplicationIdentifier, datasetsResult.DataProviderIds, CancellationToken.None);
            additionalFiles.Add((citationMarkdownStream, "citation.md"));
            if (exportGeometries?.Count > 0)
            {
                var geoJsonStream = _markdownHelper.TryGetFeatureCollection(exportGeometries);
                if (geoJsonStream != null)
                {
                    additionalFiles.Add((geoJsonStream, "geometries.geojson"));
                }
            }

            var response = await _observationManager.ExportAooAndEooArticle17AndSendAsync(
                roleId,
                authorizationApplicationIdentifier,
                filter,
                additionalFiles,
                gridCellsInMeters,
                maxDistance,
                metricCoordinateSys,
                coordinateSystem,
                emailAddress,
                description,
                exportFormat,
                sendMailFromZendTo,
                password,
                cancellationToken);
            _logger.LogInformation($"End AOO EOO Article 17 export and send job. Success: {response.Success}");
            await UpdateJobInfoEndProcessing(userId, context?.BackgroundJob?.Id, response);
            return response.Success ? true : throw new Exception("AOO EOO Article 17 export and send job failed");
        }
        catch (JobAbortedException)
        {
            await UpdateJobInfoError(userId, context?.BackgroundJob?.Id, "AOO EOO Article 17 export and send job was cancelled.");
            _logger.LogInformation("AOO EOO Article 17 export and send job was cancelled.");
            return false;
        }
        catch (Exception ex)
        {
            await UpdateJobInfoError(userId, context?.BackgroundJob?.Id, ex.Message);
            _logger.LogError(ex, "AOO EOO Article 17 export failure.");
            throw;
        }
    }
}
