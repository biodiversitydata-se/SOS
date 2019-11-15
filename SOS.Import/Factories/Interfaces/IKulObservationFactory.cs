using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using SOS.Import.Models;

namespace SOS.Import.Factories.Interfaces
{
    public interface IKulObservationFactory
    {
        /// <summary>
        /// Aggregate sightings.
        /// </summary>
        /// <returns></returns>
        Task<bool> HarvestObservationsAsync(IJobCancellationToken cancellationToken);
    }
}
