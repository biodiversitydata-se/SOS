using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Repositories.Source.Interfaces
{
    public interface IHarvestInfoRepository : IVerbatimBaseRepository<HarvestInfo, string>
    {
        /// <summary>
        /// Get harvest info by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<HarvestInfo> GetAsync(string id);

        
    }
}
