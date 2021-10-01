using System;
using System.ComponentModel;
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
        [DisplayName("Export observations. Email={1}, Description={2}, ExportFormat={3}")]
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
                var success = await _observationManager.ExportAndSendAsync(filter, email, description, exportFormat, culture, flatOut, outputFieldSet, propertyLabelType, excludeNullValues, cancellationToken);
                
                _logger.LogInformation($"End export and send job. Success: {success}");

                return success ? true : throw new Exception("Export and send job failed");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export and send job was cancelled.");
                return false;
            }
            finally
            {
                var jobId = context?.BackgroundJob?.Id;
                var userExport = await _userExportRepository.GetAsync(userId);
                userExport.OnGoingJobIds.Remove(jobId);
                await _userExportRepository.UpdateAsync(userId, userExport);
            }
        }
    }
}