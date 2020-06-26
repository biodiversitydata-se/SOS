using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Import.Factories.Harvest.Interfaces
{
    /// <summary>
    /// Harvest factory interface
    /// </summary>
    public interface IHarvestFactory<in TE, TV>
    {
        /// <summary>
        /// Cast entities to verbatim
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TV>> CastEntitiesToVerbatimsAsync(TE entities);
    }
}
