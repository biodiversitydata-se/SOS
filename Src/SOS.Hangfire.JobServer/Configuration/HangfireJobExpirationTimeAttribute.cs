using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace SOS.Hangfire.JobServer.Configuration
{
    public class HangfireJobExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
    {
        private readonly int _nrDays;

        public HangfireJobExpirationTimeAttribute(int nrDays)
        {
            _nrDays = nrDays;
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(_nrDays);
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(_nrDays);
        }
    }
}