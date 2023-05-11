using MvmService;

namespace SOS.Harvest.Services.Interfaces
{
    public interface IMvmObservationService
    {
        /// <summary>
        ///     Get Mvm observations from specified id.
        /// </summary>
        /// <param name="getFromId"></param>
        /// <returns></returns>
        Task<(long MaxChangeId, WebSpeciesObservation[] Observations)> GetAsync(long getFromId);
    }
}