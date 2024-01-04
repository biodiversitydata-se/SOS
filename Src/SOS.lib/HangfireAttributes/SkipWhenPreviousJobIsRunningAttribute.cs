using Hangfire.Client;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using System;
using System.Collections.Generic;

namespace SOS.Lib.HangfireAttributes
{
    public class SkipWhenPreviousJobIsRunning : JobFilterAttribute, IClientFilter, IApplyStateFilter
    {
        public void OnCreating(CreatingContext context)
        {
            var connection = context.Connection as JobStorageConnection;

            // We can't handle old storages
            if (connection == null)
            {
                return;
            };

            // We should run this filter only for background jobs based on 
            // recurring ones
            if (!context.Parameters.ContainsKey("RecurringJobId"))
            {
                return;
            };

            var recurringJobId = context.Parameters["RecurringJobId"] as string;

            // RecurringJobId is malformed. This should not happen, but anyway.
            if (string.IsNullOrWhiteSpace(recurringJobId))
            {
                return;
            };

            var running = connection.GetValueFromHash($"recurring-job:{recurringJobId}", "Running");
            if ("yes".Equals(running, StringComparison.OrdinalIgnoreCase))
            {
                context.Canceled = true;
            }
        }

        public void OnCreated(CreatedContext filterContext)
        {
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            if (context.NewState is EnqueuedState)
            {
                var recurringJobId = SerializationHelper.Deserialize<string>(context.Connection.GetJobParameter(context.BackgroundJob.Id, "RecurringJobId"));
                if (String.IsNullOrWhiteSpace(recurringJobId))
                {
                    return;
                };

                transaction.SetRangeInHash(
                    $"recurring-job:{recurringJobId}",
                    new[] { new KeyValuePair<string, string>("Running", "yes") });
            }
            else if ((context.NewState.IsFinal && !FailedState.StateName.Equals(context.OldStateName, StringComparison.OrdinalIgnoreCase)) ||
                        (context.NewState is FailedState))
            {
                var recurringJobId = SerializationHelper.Deserialize<string>(context.Connection.GetJobParameter(context.BackgroundJob.Id, "RecurringJobId"));
                if (string.IsNullOrWhiteSpace(recurringJobId))
                {
                    return;
                };

                transaction.SetRangeInHash(
                    $"recurring-job:{recurringJobId}",
                    new[] { new KeyValuePair<string, string>("Running", "no") });
            }
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
        }
    }
}
