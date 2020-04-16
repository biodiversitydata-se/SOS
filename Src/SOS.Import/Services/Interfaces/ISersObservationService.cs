using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Import.Services.Interfaces
{
    public interface ISersObservationService
    {
        /// <summary>
        /// Get Nors observations between changedFrom and changedTo.
        /// </summary>
        /// <param name="changedFrom">From date.</param>
        /// <param name="changedTo">To date.</param>
        /// <returns></returns>
        Task<IEnumerable<SersService.WebSpeciesObservation>> GetAsync(int getFromId);
    }
}