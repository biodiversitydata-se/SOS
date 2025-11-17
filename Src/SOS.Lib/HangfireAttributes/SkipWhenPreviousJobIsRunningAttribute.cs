using Hangfire.Client;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using SOS.Lib.Managers;
using System;
using System.Collections.Generic;

namespace SOS.Lib.HangfireAttributes;

public class SkipWhenPreviousJobIsRunning : JobFilterAttribute, IClientFilter, IApplyStateFilter
{
    public void OnCreating(CreatingContext context)
    {
        try
        {
            var connection = context.Connection as JobStorageConnection;
            
            // We can't handle old storages
            if (connection == null) return;

            // We should run this filter only for background jobs based on 
            // recurring ones
            if (!context.Parameters.ContainsKey("RecurringJobId")) return;

            var recurringJobId = context.Parameters["RecurringJobId"] as string;
            
            // RecurringJobId is malformed. This should not happen, but anyway.
            if (string.IsNullOrWhiteSpace(recurringJobId))
            {
                LogManager.LogError("SkipWhenPreviousJobIsRunning.OnCreating(). RecurringJobId is malformed.");
                return;
            }

            var running = connection.GetValueFromHash($"recurring-job:{recurringJobId}", "Running");
            if ("yes".Equals(running, StringComparison.OrdinalIgnoreCase))
            {
                context.Canceled = true;
                LogManager.LogInformation($"SkipWhenPreviousJobIsRunning.OnCreating(). Cancel new job because previous is still running. RecurringJobId={recurringJobId}");
            }
        }
        catch (Exception ex)
        {
            LogManager.LogError(ex, "SkipWhenPreviousJobIsRunning.OnCreating() error");
        }
    }

    public void OnCreated(CreatedContext filterContext) { }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        try
        {
            var newState = context.NewState?.Name ?? "<null>";
            var oldState = context.OldStateName ?? "<null>";
            var jobId = context.BackgroundJob?.Id ?? "<unknown>";

            LogManager.LogInformation($"SkipWhenPreviousJobIsRunning.OnStateApplied() JobId={jobId}, OldState={oldState}, NewState={newState}");

            var recurringJobId = SerializationHelper.Deserialize<string>(
                context.Connection.GetJobParameter(context.BackgroundJob.Id, "RecurringJobId"));

            if (string.IsNullOrWhiteSpace(recurringJobId)) return;

            if (context.NewState is EnqueuedState)
            {
                transaction.SetRangeInHash(
                    $"recurring-job:{recurringJobId}",
                    new[] { new KeyValuePair<string, string>("Running", "yes") });

                LogManager.LogInformation($"Set Running=yes for RecurringJobId={recurringJobId}");
            }
            else if (context.NewState.IsFinal)
            {
                // Final state => always set "Running" to "no"
                transaction.SetRangeInHash(
                    $"recurring-job:{recurringJobId}",
                    new[] { new KeyValuePair<string, string>("Running", "no") });

                LogManager.LogInformation($"Set Running=no for RecurringJobId={recurringJobId}, FinalState={newState}");
            }
        }
        catch (Exception ex)
        {
            LogManager.LogError(ex, "SkipWhenPreviousJobIsRunning.OnStateApplied() error");
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) { }
}