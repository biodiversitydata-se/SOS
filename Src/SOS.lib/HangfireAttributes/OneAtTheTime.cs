using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace SOS.Lib.HangfireAttributes
{
    public class OneAtTheTime : JobFilterAttribute, IElectStateFilter, IApplyStateFilter
    {
        private IDisposable AcquireDistributedSetLock(IStorageConnection connection, string type, string method) => connection.AcquireDistributedLock(GetDistributedLockKey(type, method), TimeSpan.FromMinutes(1));

        private static DeletedState CreateDeletedState(string blockedBy) => new DeletedState
        {
            Reason = $"Execution was blocked by background job {blockedBy}, all attempts exhausted"
        };
 
        private IState CreateScheduledState(string blockedBy, int currentAttempt)
        {
            var reason = $"Execution is blocked by background job {blockedBy}, retry attempt: {currentAttempt}";

            if (MaxAttempts > 0)
            {
                reason += $"/{MaxAttempts}";
            }

            return new ScheduledState(TimeSpan.FromSeconds(RetryInSeconds))
            {
                Reason = reason
            };
        }

        private string GetDistributedLockKey(string type, string method) => $"extension:job-mutex:lock:{type}-{method}";

        private string GetResourceKey(string type, string method) => $"extension:job-mutex:set:{type}-{method}";

        /// <summary>
        /// Constructor
        /// </summary>
        public OneAtTheTime()
        {
            RetryInSeconds = 60;
        }

        /// <summary>
        /// Max times to retry running job. 0 - infinite
        /// </summary>
        public int MaxAttempts { get; set; }

        /// <summary>
        /// Time between retry attempts
        /// </summary>
        public int RetryInSeconds { get; set; } 

        /// <summary>
        /// On state election event
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="NotSupportedException"></exception>
        public void OnStateElection(ElectStateContext context)
        {
            // We are intercepting transitions to the Processed state, that is performed by
            // a worker just before processing a job. During the state election phase we can
            // change the target state to another one, causing a worker not to process the
            // backgorund job.

            if (context.CandidateState.Name != ProcessingState.StateName ||
                context.BackgroundJob.Job == null)
            {
                return;
            }

            // This filter requires an extended set of storage operations. It's supported
            // by all the official storages, and many of the community-based ones.
            if (!(context.Connection is JobStorageConnection storageConnection))
            {
                throw new NotSupportedException("This version of storage doesn't support extended methods. Please try to update to the latest version.");
            }

            string blockedBy;

            try
            {
                // Distributed lock is needed here only to prevent a race condition, when another
                // worker picks up a background job with the same resource between GET and SET
                // operations.
                // There will be no race condition, when two or more workers pick up background job
                // with the same id, because state transitions are protected with distributed lock
                // themselves.
                using (AcquireDistributedSetLock(context.Connection, context.BackgroundJob.Job.Type.Name, context.BackgroundJob.Job.Method.Name))
                {
                    var resourceKey = GetResourceKey(context.BackgroundJob.Job.Type.Name, context.BackgroundJob.Job.Method.Name);

                    // Resource set contains a background job id that acquired a mutex for the resource.
                    // We are getting only one element to see what background job blocked the invocation.
                    var range = storageConnection.GetRangeFromSet(
                        resourceKey,
                        0,
                        0);

                    blockedBy = range.Count > 0 ? range[0] : null;

                    // We should permit an invocation only when the set is empty, or if current background
                    // job is already owns a resource. This may happen, when the localTransaction succeeded,
                    // but outer transaction was failed.
                    if (blockedBy == null || blockedBy == context.BackgroundJob.Id)
                    {
                        // We need to commit the changes inside a distributed lock, otherwise it's
                        // useless. So we create a local transaction instead of using the
                        // context.Transaction property.
                        var localTransaction = context.Connection.CreateWriteTransaction();

                        // Add the current background job identifier to a resource set. This means
                        // that resource is owned by the current background job. Identifier will be
                        // removed only on failed state, or in one of final states (succeeded or
                        // deleted).
                        localTransaction.AddToSet(resourceKey, context.BackgroundJob.Id);
                        localTransaction.Commit();

                        // Invocation is permitted, and we did all the required things.

                        return;

                    }
                }
            }
            catch (DistributedLockTimeoutException)
            {
                // We weren't able to acquire a distributed lock within a specified window. This may
                // be caused by network delays, storage outages or abandoned locks in some storages.
                // Since it is required to expire abandoned locks after some time, we can simply
                // postpone the invocation.

                context.CandidateState = new ScheduledState(TimeSpan.FromSeconds(RetryInSeconds))
                {
                    Reason = "Couldn't acquire a distributed lock for mutex: timeout exceeded"
                };

                return;
            }

            // Background job execution is blocked. We should change the target state either to
            // the Scheduled or to the Deleted one, depending on current retry attempt number.
            var currentAttempt = context.GetJobParameter<int>("MutexAttempt") + 1;
            context.SetJobParameter("MutexAttempt", currentAttempt);

            context.CandidateState = MaxAttempts == 0 || currentAttempt <= MaxAttempts
                ? CreateScheduledState(blockedBy, currentAttempt)
                : CreateDeletedState(blockedBy);

        }

        /// <summary>
        /// On state applited event
        /// </summary>
        /// <param name="context"></param>
        /// <param name="transaction"></param>
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            if (context.BackgroundJob.Job == null || context.OldStateName != ProcessingState.StateName) {
                return;
            };

            using (AcquireDistributedSetLock(context.Connection, context.BackgroundJob.Job.Type.Name, context.BackgroundJob.Job.Method.Name))
            {
                var localTransaction = context.Connection.CreateWriteTransaction();
                localTransaction.RemoveFromSet(GetResourceKey(context.BackgroundJob.Job.Type.Name, context.BackgroundJob.Job.Method.Name), context.BackgroundJob.Id);
                localTransaction.Commit();
            }
        }

        /// <summary>
        /// On state unapplied event
        /// </summary>
        /// <param name="context"></param>
        /// <param name="transaction"></param>
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {

        }
    }
}
