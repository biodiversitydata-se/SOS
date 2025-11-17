using Hangfire;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import;

public interface ITaxonListsHarvestJob
{
    /// <summary>
    ///     Run harvest taxa lists job.
    /// </summary>
    /// <returns></returns>
    [JobDisplayName("Harvest taxon lists from TaxonListService")]
    [Queue("high")]
    Task<bool> RunHarvestTaxonListsAsync();
}