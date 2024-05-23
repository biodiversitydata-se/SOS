using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using SOS.Lib.Managers;
using System;

namespace SOS.Lib.HangfireAttributes
{
    /// <summary>
    /// Zero set job expiration timeout to minimize time in success log
    /// </summary>
    public class JobExpirationTimeout : JobFilterAttribute, IApplyStateFilter
    {
        /// <summary>
        /// Job Expiration Timeout
        /// </summary>
        public int Minutes { get; set; }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            LogManager.LogInformation("START JobExpirationTimeout.OnStateApplied()");
            context.JobExpirationTimeout = TimeSpan.FromMinutes(Minutes);
            LogManager.LogInformation("STOP JobExpirationTimeout.OnStateApplied()");
        }

        public void OnStateUnapplied(
            ApplyStateContext context,
            IWriteOnlyTransaction transaction)
        {
        }
    }
}
