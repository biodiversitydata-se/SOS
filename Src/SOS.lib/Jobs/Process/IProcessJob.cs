using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;

namespace SOS.Lib.Jobs.Process
{
    public interface IProcessJob
    {
        /// <summary>
        /// Run process job
        /// </summary>
        /// <param name="dataProviderIdOrIdentifiers"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(
            List<string> dataProviderIdOrIdentifiers,
            JobRunModes mode,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Run full process job
        /// </summary>
        /// <param name="cleanStart"></param>
        /// <param name="copyFromActiveOnFail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(
            bool cleanStart,
            bool copyFromActiveOnFail,
            IJobCancellationToken cancellationToken);
    }
}