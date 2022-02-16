using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Export.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class ExportAndSendJob : IExportAndSendJob
    {
        
        private readonly IObservationManager _observationManager;
        private readonly IUserExportRepository _userExportRepository;
        private readonly ILogger<ExportAndSendJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="userExportRepository"></param>
        /// <param name="logger"></param>
        public ExportAndSendJob(IObservationManager observationManager, IUserExportRepository userExportRepository, ILogger<ExportAndSendJob> logger)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _userExportRepository =
                userExportRepository ?? throw new ArgumentNullException(nameof(userExportRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(SearchFilter filter, 
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
            IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start export and send job");
                Thread.Sleep(TimeSpan.FromSeconds(1)); // wait for job info to be inserted in MongoDb.
                await UpdateJobInfoStartProcessing(userId, context?.BackgroundJob?.Id);
                
                var success = await _observationManager.ExportAndSendAsync(filter, email, description, exportFormat, culture, flatOut, outputFieldSet, propertyLabelType, excludeNullValues, cancellationToken);
                
                _logger.LogInformation($"End export and send job. Success: {success}");
                await UpdateJobInfoEndProcessing(userId, context?.BackgroundJob?.Id);
                return success ? true : throw new Exception("Export and send job failed");
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
                throw ex;
            }
            finally
            {
                var jobId = context?.BackgroundJob?.Id;
                var userExport = await _userExportRepository.GetAsync(userId);
                userExport.OnGoingJobIds.Remove(jobId);
                await _userExportRepository.UpdateAsync(userId, userExport);
            }
        }

        private async Task UpdateJobInfoStartProcessing(int userId, string jobId)
        {
            try
            {                
                var userExport = await _userExportRepository.GetAsync(userId);
                var jobInfo = userExport.Jobs.FirstOrDefault(m => m.Id == jobId);
                if (jobInfo == null) return;

                jobInfo.ProcessStartDate = DateTime.UtcNow;
                jobInfo.Status = Lib.Models.Export.ExportJobStatus.Processing;
                await _userExportRepository.UpdateAsync(userId, userExport);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Couldn't update job info");
            }
        }

        private async Task UpdateJobInfoEndProcessing(int userId, string jobId)
        {
            try
            {
                var userExport = await _userExportRepository.GetAsync(userId);
                var jobInfo = userExport.Jobs.FirstOrDefault(m => m.Id == jobId);
                if (jobInfo == null) return;
                jobInfo.ProcessEndDate = DateTime.UtcNow;
                jobInfo.ProcessingTime = jobInfo.ProcessEndDate.Value - jobInfo.ProcessStartDate.Value;
                jobInfo.Status = Lib.Models.Export.ExportJobStatus.Succeeded;                
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
                jobInfo.Status = Lib.Models.Export.ExportJobStatus.Failed;
                jobInfo.ErrorMsg = errorMsg;
                await _userExportRepository.UpdateAsync(userId, userExport);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Couldn't update job info");
            }
        }
    }
}