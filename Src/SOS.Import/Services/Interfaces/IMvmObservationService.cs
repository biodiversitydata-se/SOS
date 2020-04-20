using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Import.Services.Interfaces
{
    public interface IMvmObservationService
    {
        /// <summary>
        ///  Get Mvm observations from specified id.
        /// </summary>
        /// <param name="getFromId"></param>
        /// <returns></returns>
        Task<IEnumerable<MvmService.WebSpeciesObservation>> GetAsync(int getFromId);
    }
}