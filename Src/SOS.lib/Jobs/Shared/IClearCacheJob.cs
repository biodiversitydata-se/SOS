using Hangfire;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Shared
{
    public interface IClearCacheJob
    {
        /// <summary>
        /// Clear selected caches
        /// </summary>
        /// <param name="caches"></param>
        /// <returns></returns>
        [JobDisplayName("Clear caches")]
        [AutomaticRetry(Attempts = 3, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("high")]
        Task RunAsync(IEnumerable<Enums.Cache> caches);
    }
}
