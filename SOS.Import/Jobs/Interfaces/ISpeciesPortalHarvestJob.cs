using System.Threading.Tasks;
using Hangfire;

namespace SOS.Import.Jobs.Interfaces
{
    public interface ISpeciesPortalHarvestJob
    {
        /// <summary>
        /// Run species portal harvest
        /// </summary>
        /// <returns></returns>
        Task<bool> Run(IJobCancellationToken cancellationToken);
    }
}
