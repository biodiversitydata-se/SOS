using System.Threading.Tasks;

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
        Task<bool> HarvestAsync();
    }
}
