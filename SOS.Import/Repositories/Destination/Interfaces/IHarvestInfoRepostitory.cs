using System;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Repositories.Destination.Interfaces
{
    public interface IHarvestInfoRepository : IVerbatimRepository<HarvestInfo, string>
    {
        /// <summary>
        /// Add or update harvest info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="provider"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="sightingCount"></param>
        /// <returns></returns>
        Task<bool> UpdateHarvestInfoAsync(string id, DataProviderId provider, DateTime start, DateTime end, int sightingCount);
    }
}
