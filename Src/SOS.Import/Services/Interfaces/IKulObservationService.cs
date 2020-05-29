using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KulService;

namespace SOS.Import.Services.Interfaces
{
    public interface IKulObservationService
    {
        /// <summary>
        ///     Get KUL observations between changedFrom and changedTo.
        /// </summary>
        /// <param name="changedFrom">From date.</param>
        /// <param name="changedTo">To date.</param>
        /// <returns></returns>
        Task<IEnumerable<WebSpeciesObservation>> GetAsync(DateTime changedFrom, DateTime changedTo);
    }
}