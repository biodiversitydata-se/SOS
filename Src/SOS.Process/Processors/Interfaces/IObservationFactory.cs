using System.Collections.Generic;
using System.Threading.Tasks;
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
        Task<IEnumerable<Observation>> CreateProcessedObservationsAsync(
            IEnumerable<TEntity> verbatims);

        /// <summary>
        /// Cast verbatim to observation
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        Task<Observation> CreateProcessedObservationAsync(TEntity verbatim);
    }
}
