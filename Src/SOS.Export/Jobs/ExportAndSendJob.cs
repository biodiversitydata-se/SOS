using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Managers.Interfaces;
using SOS.Export.Models.ZendTo;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Export.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class ExportAndSendJob : IExportAndSendJob
    {
        private readonly ICryptoService _cryptoService;
        private readonly IObservationManager _observationManager;
        private readonly IUserExportRepository _userExportRepository;
        private readonly ILogger<ExportAndSendJob> _logger;

        /// <summary>
        /// Remove expired and old jobs
        /// </summary>
        /// <param name="userExport"></param>
        private void RemoveExpiredJobs(UserExport userExport)
        {
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

                var jobInfo = userExport.Jobs.FirstOrDefault(m => m.Id == jobId);
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
                var jobInfo = userExport.Jobs.FirstOrDefault(m => m.Id == jobId);
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
        public ExportAndSendJob(IObservationManager observationManager, IUserExportRepository userExportRepository, ICryptoService cryptoService, ILogger<ExportAndSendJob> logger)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _userExportRepository =
                userExportRepository ?? throw new ArgumentNullException(nameof(userExportRepository));
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(
            SearchFilter filter, 
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
            IJobCancellationToken cancellationToken)
        {
            var userId = filter?.ExtendedAuthorization?.UserId ?? 0;

            try
            {
                
                _logger.LogInformation("Start export and send job");
                Thread.Sleep(TimeSpan.FromSeconds(1)); // wait for job info to be inserted in MongoDb.
                await UpdateJobInfoStartProcessing(userId, context?.BackgroundJob?.Id);
                var password = await _cryptoService.DecryptAsync(encryptedPassword);
                var response = await _observationManager.ExportAndSendAsync(roleId, authorizationApplicationIdentifier, filter, email, description, exportFormat, culture, flatOut, propertyLabelType, excludeNullValues, sensitiveObservations, sendMailFromZendTo, password, cancellationToken);
                
                _logger.LogInformation($"End export and send job. Success: {response.Success}");
                await UpdateJobInfoEndProcessing(userId, context?.BackgroundJob?.Id, response);
                return response.Success ? true : throw new Exception("Export and send job failed");
            }
            catch (JobAbortedException e)
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
    }
}