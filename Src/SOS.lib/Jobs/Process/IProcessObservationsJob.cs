using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;

namespace SOS.Lib.Jobs.Process
{
    public interface IProcessObservationsJob
    {
        /// <summary>
        /// Run process job
        /// </summary>
        /// <param name="dataProviderIdOrIdentifiers"></param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisplayName("Process Observations [Mode={1}]")]
        [Queue("high")]
        Task<bool> RunAsync(
            List<string> dataProviderIdOrIdentifiers,
            JobRunModes mode,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Run full process job
        /// </summary>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Queue("high")]
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);
    }
}