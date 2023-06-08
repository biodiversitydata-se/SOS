using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using System;

namespace SOS.Lib.HangfireAttributes
{
    public class SkipConcurrentExecution : JobFilterAttribute, IServerFilter, IElectStateFilter
    {
        private readonly int _timeoutSeconds;
        private const string DistributedLock = "DistributedLock";

        public SkipConcurrentExecution(int timeOutSeconds)
        {
            if (timeOutSeconds < 0) throw new ArgumentException("Timeout argument value should be greater that zero.");
            _timeoutSeconds = timeOutSeconds;
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            if (!filterContext.Items.ContainsKey(DistributedLock))
                throw new InvalidOperationException("Can not release a distributed lock: it was not acquired.");

            var distributedLock = (IDisposable)filterContext.Items[DistributedLock];
            distributedLock?.Dispose();
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            var resource = $"{filterContext.BackgroundJob.Job.Type.FullName}.{filterContext.BackgroundJob.Job.Method.Name}";
           
            try
            {
                var distributedLock = filterContext.Connection.AcquireDistributedLock(resource, TimeSpan.FromSeconds(_timeoutSeconds));
                filterContext.Items[DistributedLock] = distributedLock;
            }
            catch
            {
                filterContext.Canceled = true;
            }

        }

        public void OnStateElection(ElectStateContext context)
        {
           
        }
    }
}
