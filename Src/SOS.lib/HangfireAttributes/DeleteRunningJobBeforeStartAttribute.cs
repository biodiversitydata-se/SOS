using System;
using System.Linq;
using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace SOS.Lib.HangfireAttributes
{
    /// <summary>
    /// Stop currently runnings jobs 
    /// </summary>
    public class DeleteRunningJobBeforeStart : JobFilterAttribute, IClientFilter, IApplyStateFilter
    {
        public void OnCreating(CreatingContext context)
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            var ongoingJobs = monitoringApi.ProcessingJobs(0, (int)monitoringApi.ProcessingCount())?
                .Where(j => j.Value.InProcessingState &&
                          (j.Value?.Job?.Type?.Name.Equals(context.Job.Type.Name,
                              StringComparison.CurrentCultureIgnoreCase) ?? false) &&
                          (j.Value.Job?.Method.Name.Equals(context.Job.Method.Name, StringComparison.CurrentCultureIgnoreCase) ?? false));
            if (ongoingJobs?.Any() ?? false)
            {
                foreach (var ongoingJob in ongoingJobs)
                {
                    BackgroundJob.Delete(ongoingJob.Key);
                }
            }
        }

        public void OnCreated(CreatedContext context)
        {
            
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
        }
    }
}