using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IVirtualHerbariumHarvestJob
    {
        /// <summary>
        /// Run Virtual Herbarium harvest.
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);
    }
}
