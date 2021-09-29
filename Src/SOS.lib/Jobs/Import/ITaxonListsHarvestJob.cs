using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface ITaxonListsHarvestJob
    {
        /// <summary>
        ///     Run harvest taxa lists job.
        /// </summary>
        /// <returns></returns>
        [DisplayName("Harvest taxon lists from TaxonListService")]
        [Queue("high")]
        Task<bool> RunHarvestTaxonListsAsync();
    }
}