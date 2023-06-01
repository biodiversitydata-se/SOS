using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using SOS.Lib.Context;
using SOS.Lib.Models.Export;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.HangfireAttributes
{
    public class HandleFileExportFailureAttribute : JobFilterAttribute, IApplyStateFilter
    {
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            try
            {
                // Unlike IElectStateFilter.OnStateElection, this function only gets called with a failed status after
                // all retries have been used up
                var failedState = context.NewState as FailedState;
                var deletedState = context.NewState as DeletedState;

                if (failedState != null || deletedState != null)
                {
                    var searchFilter = context?.BackgroundJob?.Job?.Args?.ElementAtOrDefault(0) as Models.Search.Filters.SearchFilter;
                    var userId = searchFilter?.ExtendedAuthorization?.UserId;
                    if (userId.HasValue)
                    {
                        UpdateJobInfoError(userId.Value, context.BackgroundJob.Id, failedState?.Exception?.Message).Wait();
                    }
                }
            }
            catch (Exception) { }
        }

        private async Task UpdateJobInfoError(int userId, string jobId, string errorMsg)
        {
            if (HangfireJobServerContext.Host == null)
            {
                return;
            }
            var userExportRepository = HangfireJobServerContext.Host.Services.GetService(typeof(IUserExportRepository)) as IUserExportRepository;
            var userExport = await userExportRepository.GetAsync(userId);
            var jobInfo = userExport.Jobs.FirstOrDefault(m => m.Id == jobId);
            if (jobInfo == null) return;
            jobInfo.ProcessEndDate = DateTime.UtcNow;
            jobInfo.Status = ExportJobStatus.Failed;
            jobInfo.ErrorMsg = errorMsg;
            await userExportRepository.UpdateAsync(userId, userExport);            
            var logger = HangfireJobServerContext.Host.Services.GetService(typeof(ILogger<HandleFileExportFailureAttribute>)) as ILogger<HandleFileExportFailureAttribute>;
            logger.LogInformation($"Set status to Failed for export job with Id={jobId} for UserId={userId}. ErrorMsg={errorMsg}");
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            // Required to match IApplyStateFilter
        }        
    }
}
