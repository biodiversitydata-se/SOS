using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace SOS.Lib.HangfireAttributes
{
    public class PreserveOriginalQueue : JobFilterAttribute, IApplyStateFilter
    {
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            try
            {
                // Activating only when enqueueing a background job
                if (context.NewState is not EnqueuedState enqueuedState) {
                    return;
                };

                // Checking if an original queue is already set
                var originalQueue = SerializationHelper.Deserialize<string>(
                    context.Connection.GetJobParameter(
                        context.BackgroundJob.Id,
                        "OriginalQueue")
                );

                if (originalQueue != null)
                {
                    // Override any other queue value that is currently set (by other filters, for example)
                    enqueuedState.Queue = originalQueue;
                }
                else
                {
                    // Queueing for the first time, we should set the original queue
                    context.Connection.SetJobParameter(
                        context.BackgroundJob.Id,
                        "OriginalQueue",
                        SerializationHelper.Serialize(enqueuedState.Queue));
                }
            }
            catch { }
        }


        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            // Required to match IApplyStateFilter
        }
    }
}
