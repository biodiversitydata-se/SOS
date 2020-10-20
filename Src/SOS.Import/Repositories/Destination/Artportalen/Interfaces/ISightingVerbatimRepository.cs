using System.Threading.Tasks;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Import.Repositories.Destination.Artportalen.Interfaces
{
    /// <summary>
    /// </summary>
    public interface ISightingVerbatimRepositoryOld : IVerbatimRepository<ArtportalenObservationVerbatim, int>
    {
        Task<int> GetMaxIdAsync();
    }
}