using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IArtportalenHarvestJob : IHarvestJob
    {
        /// <summary>
        /// Artportalen harvest 
        /// </summary>
        /// <param name="incrementalHarvest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> RunAsync(bool incrementalHarvest, IJobCancellationToken cancellationToken);
    }
}