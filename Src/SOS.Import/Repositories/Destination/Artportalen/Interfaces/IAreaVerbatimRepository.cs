using System.Threading.Tasks;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Repositories.Destination.Artportalen.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAreaVerbatimRepository : IVerbatimRepository<Area, int>
    {
        /// <summary>
        /// Create search index
        /// </summary>
        /// <returns></returns>
        Task CreateIndexAsync();
    }
}
