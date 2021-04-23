using System.Threading.Tasks;
using System.Xml.Linq;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Interfaces;

namespace SOS.Lib.Repositories.Resource.Interfaces
{
    /// <summary>
    /// Data provider repository
    /// </summary>
    public interface IDataProviderRepository : IRepositoryBase<DataProvider, int>
    {
        /// <summary>
        /// Clear all Eml files
        /// </summary>
        /// <returns></returns>
        Task<bool> ClearEmlAsync();

        /// <summary>
        /// Get EML
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<XDocument> GetEmlAsync(int providerId);

        /// <summary>
        /// Store EML
        /// </summary>
        /// <param name="providerId"></param>
        /// <param name="eml"></param>
        /// <returns></returns>
        Task<bool> StoreEmlAsync(int providerId, XDocument eml);
    }
}