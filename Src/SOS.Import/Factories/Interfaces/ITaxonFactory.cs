using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Factories.Interfaces
{
    /// <summary>
    /// Taxon factory
    /// </summary>
    public interface ITaxonFactory
    {
        /// <summary>
        /// Harvest taxa.
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestAsync();
    }
}
