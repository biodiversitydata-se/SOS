using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Import.Services.Interfaces
{
    public interface INorsObservationService
    {
        /// <summary>
        ///  Get Nors observations from specified id.
        /// </summary>
        /// <param name="getFromId"></param>
        /// <returns></returns>
        Task<IEnumerable<NorsService.WebSpeciesObservation>> GetAsync(int getFromId);
    }
}