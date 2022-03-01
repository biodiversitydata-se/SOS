using SOS.Lib.Models.DarwinCore;

namespace SOS.Harvest.Services.Taxon.Interfaces
{
    /// <summary>
    ///     Interface for taxon service
    /// </summary>
    public interface ITaxonService
    {
        /// <summary>
        ///     Get all taxa
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<DarwinCoreTaxon>> GetTaxaAsync();

        /// <summary>
        ///     Gets the sort orders from the service
        /// </summary>
        /// <param name="taxonIds"></param>
        /// <returns></returns>        
    }
}