using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Process.Processors.Interfaces
{
    /// <summary>
    /// Interface for observation factory
    /// </summary>
    public interface IObservationFactory<TEntity> where TEntity: IEntity<int>
    {
        /// <summary>
        /// Cast verbatims to observations
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        IEnumerable<Observation> CreateProcessedObservations(
            IEnumerable<TEntity> verbatims);

        /// <summary>
        /// Cast verbatim to observation
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        Observation CreateProcessedObservation(TEntity verbatim);
    }
}
