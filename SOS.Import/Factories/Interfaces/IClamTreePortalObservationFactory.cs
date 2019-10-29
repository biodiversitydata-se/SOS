using System.Threading.Tasks;

namespace SOS.Import.Factories.Interfaces
{
    /// <summary>
    /// Clam and tree portal observation factory interface
    /// </summary>
    public interface IClamTreePortalObservationFactory
    {
        /// <summary>
        /// Harvest clams.
        /// </summary>
        /// <returns></returns>
        Task<bool> HarvestClamsAsync();

        /// <summary>
        /// Harvest trees.
        /// </summary>
        /// <returns></returns>
        Task<bool> HarvestTreesAsync();
    }
}
