using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Harvest.Services.Interfaces
{
    public interface ISharkObservationService
    {
        /// <summary>
        ///     Get data set from Shark
        /// </summary>
        /// <param name="dataSetName"></param>
        /// <returns></returns>
        Task<SharkJsonFile?> GetAsync(string dataSetName);

        /// <summary>
        ///     Get data sets available
        /// </summary>
        /// <returns></returns>
        Task<SharkJsonFile?> GetDataSetsAsync();
    }
}