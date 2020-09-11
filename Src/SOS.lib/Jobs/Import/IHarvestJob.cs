using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;

namespace SOS.Lib.Jobs.Import
{
    public interface IHarvestJob
    {
        /// <summary>
        /// Run harvest job
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        /// Run job
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken);
    }
}