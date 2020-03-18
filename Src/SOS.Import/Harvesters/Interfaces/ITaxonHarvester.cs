using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Interfaces
{
    /// <summary>
    /// Taxon factory
    /// </summary>
    public interface ITaxonHarvester
    {
        /// <summary>
        /// Harvest taxa.
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestAsync();
    }
}
