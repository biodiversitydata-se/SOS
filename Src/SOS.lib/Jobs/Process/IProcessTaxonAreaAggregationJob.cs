using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Process
{
    /// <summary>
    /// Process check lists job
    /// </summary>
    public interface IProcessTaxonAreaAggregationJob
    {
        /// <summary>
        ///  Run full process job
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(
            IJobCancellationToken cancellationToken);
    }
}