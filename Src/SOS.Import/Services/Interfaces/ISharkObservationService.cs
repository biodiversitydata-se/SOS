using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Import.Services.Interfaces
{
    public interface ISharkObservationService
    {
        /// <summary>
        ///  Get data set from Shark
        /// </summary>
        /// <param name="dataSetName"></param>
        /// <returns></returns>
        Task<SharkJsonFile> GetAsync(string dataSetName);

        /// <summary>
        /// Get data sets available
        /// </summary>
        /// <returns></returns>
        Task<SharkJsonFile> GetDataSetsAsync();
    }
}