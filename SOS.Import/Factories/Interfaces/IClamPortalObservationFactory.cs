using System.Threading.Tasks;
using Hangfire;

namespace SOS.Import.Factories.Interfaces
{
    /// <summary>
    /// Clam and tree portal observation factory interface
    /// </summary>
    public interface IClamPortalObservationFactory
    {
        /// <summary>
        /// Harvest clams.
        /// </summary>
        /// <returns></returns>
        Task<bool> HarvestClamsAsync(IJobCancellationToken cancellationToken);
    }
}
