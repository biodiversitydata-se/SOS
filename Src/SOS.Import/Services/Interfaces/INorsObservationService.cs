using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NorsService;

namespace SOS.Import.Services.Interfaces
{
    public interface INorsObservationService
    {
        /// <summary>
        ///     Get Nors observations from specified id.
        /// </summary>
        /// <param name="getFromId"></param>
        /// <returns></returns>
        Task<Tuple<long, IEnumerable<WebSpeciesObservation>>> GetAsync(long getFromId);
    }
}