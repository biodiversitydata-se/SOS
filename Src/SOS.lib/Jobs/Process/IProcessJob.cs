using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Process
{
    public interface IProcessJob
    {
        Task<bool> RunAsync(
            List<string> dataProviderIdOrIdentifiers,
            bool cleanStart,
            bool incrementalMode,
            bool copyFromActiveOnFail,
            bool toggleInstanceOnSuccess,
            IJobCancellationToken cancellationToken);

        Task<bool> RunAsync(
            bool cleanStart,
            bool copyFromActiveOnFail,
            bool toggleInstanceOnSuccess,
            IJobCancellationToken cancellationToken);
    }
}