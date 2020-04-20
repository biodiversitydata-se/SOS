using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IMvmHarvestJob
    {
        /// <summary>
        /// Run MVM harvest.
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);
    }
}
