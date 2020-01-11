using System.Threading.Tasks;
using Hangfire;

namespace SOS.Import.Jobs.Interfaces
{
    public interface IClamPortalHarvestJob
    {
        /// <summary>
        /// Run species portal harvest
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);
    }
}
