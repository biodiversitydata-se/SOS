using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Process.Processors.Interfaces
{
    /// <summary>
    /// Interface for observation factory
    /// </summary>
    public interface IObservationFactory<TEntity> where TEntity : IEntity<int>
    {
        /// <summary>
        /// Cast verbatim to observation
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="diffuseIfSupported"></param>
        /// <returns></returns>
        Observation CreateProcessedObservation(TEntity verbatim, bool diffuseIfSupported);
    }
}