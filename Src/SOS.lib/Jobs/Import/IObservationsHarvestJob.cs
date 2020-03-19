using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IObservationsHarvestJob
    {
        /// <summary>
        /// Harvest multiple sources and start processing when done
        /// </summary>
        /// <param name="harvestSources"></param>
        /// <param name="processSources"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(int harvestSources, int processSources, IJobCancellationToken cancellationToken);
    }
}
