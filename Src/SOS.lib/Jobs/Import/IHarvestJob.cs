using System.Threading.Tasks;
using Hangfire;

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
        /// <param name="incrementalHarvest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(bool incrementalHarvest, IJobCancellationToken cancellationToken);
    }
}