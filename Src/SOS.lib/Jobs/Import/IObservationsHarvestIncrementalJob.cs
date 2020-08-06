using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IObservationsHarvestIncrementalJob
    {
        /// <summary>
        ///     Harvest multiple sources and start processing when done
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);
    }
}