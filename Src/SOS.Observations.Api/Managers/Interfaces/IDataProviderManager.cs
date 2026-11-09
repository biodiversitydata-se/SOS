using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Observations.Api.Managers.Interfaces;

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
    /// <param name="categories">Category/ies to match. NationalInventory, RegionalInventory, MuseumCollections, HerbariaCollections, CitizenSciencePlatform, Atlas, 
    /// Terrestrial, Freshwater, Marine, Vertebrates, Invertebrates, Arthropods, Microorganisms, Plants_Bryophytes_Lichens,
    /// Fungi, Algae, DataStewardship</param>
    /// <returns></returns>
    Task<IEnumerable<DataProviderDto>> GetDataProvidersAsync(bool includeInactive, string cultureCode, bool includeProvidersWithNoObservations = true, IEnumerable<SOS.Lib.Enums.DataProviderCategory> categories = null);

    /// <summary>
    /// Get provider EML file
    /// </summary>
    /// <param name="providerId"></param>
    /// <returns></returns>
    Task<byte[]> GetEmlFileAsync(int providerId);
}
