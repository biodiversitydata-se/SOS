using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Import.Models;

namespace SOS.Import.Factories.Interfaces
{
    public interface IKulObservationFactory
    {
        /// <summary>
        /// Aggregate sightings.
        /// </summary>
        /// <returns></returns>
        Task<bool> HarvestObservationsAsync();

        /// <summary>
        /// Aggregate sightings.
        /// </summary>
        /// <param name="options">Options used in aggregation.</param>
        /// <returns></returns>
        Task<bool> HarvestObservationsAsync(KulAggregationOptions options);
    }
}
