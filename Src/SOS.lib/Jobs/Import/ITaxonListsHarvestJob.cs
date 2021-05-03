using System.ComponentModel;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    public interface ITaxonListsHarvestJob
    {
        /// <summary>
        ///     Run harvest taxa lists job.
        /// </summary>
        /// <returns></returns>
        [DisplayName("Harvest taxon lists from TaxonListService")]
        Task<bool> RunHarvestTaxonListsAsync();
    }
}