using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Import.Services.Interfaces
{
    public interface IKulObservationService
    {
        /// <summary>
        /// Get KUL observations between changedFrom and changedTo.
        /// </summary>
        /// <param name="changedFrom">From date.</param>
        /// <param name="changedTo">To date.</param>
        /// <returns></returns>
        Task<IEnumerable<KulService.WebSpeciesObservation>> GetAsync(DateTime changedFrom, DateTime changedTo);
    }
}