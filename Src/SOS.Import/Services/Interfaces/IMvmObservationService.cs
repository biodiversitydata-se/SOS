using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MvmService;

namespace SOS.Import.Services.Interfaces
{
    public interface IMvmObservationService
    {
        /// <summary>
        ///     Get Mvm observations from specified id.
        /// </summary>
        /// <param name="getFromId"></param>
        /// <returns></returns>
        Task<Tuple<long, IEnumerable<WebSpeciesObservation>>> GetAsync(long getFromId);
    }
}