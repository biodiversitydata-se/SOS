using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Shared.Api.Dtos;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Data provider manager.
    /// </summary>
    public interface IDataProviderManager
    {
        /// <summary>
        /// Get data providers
        /// </summary>
        /// <param name="includeInactive"></param>
        /// <param name="cultureCode"></param>
        /// <param name="includeProvidersWithNoObservations"></param>
        /// <param name="categories">Category/ies to match. DataHostesship, RegionalInventory, CitizenSciencePlatform, Atlas, 
        /// Terrestrial, Freshwater, Marine, Vertebrates, Arthropods, Microorganisms, Plants_Bryophytes_Lichens,
        /// Fungi, Algae</param>
        /// <returns></returns>
        Task<IEnumerable<DataProviderDto>> GetDataProvidersAsync(bool includeInactive, string cultureCode, bool includeProvidersWithNoObservations = true, IEnumerable<DataProviderCategory> categories = null);

        /// <summary>
        /// Get provider EML file
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<byte[]> GetEmlFileAsync(int providerId);
    }
}