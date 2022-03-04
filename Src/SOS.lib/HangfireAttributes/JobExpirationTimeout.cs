using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

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
            context.JobExpirationTimeout = TimeSpan.FromMinutes(Minutes);
        }

        public void OnStateUnapplied(
            ApplyStateContext context,
            IWriteOnlyTransaction transaction)
        {
        }
    }
}
