using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Harvest.Processors.Interfaces
{
    /// <summary>
    /// Interface for event factory
    /// </summary>
    public interface IEventFactory<TEntity> where TEntity : IEntity<int>
    {
        /// <summary>
        /// Cast verbatim to observation
        /// </summary>
        /// <param name="verbatim"></param>        
        /// <returns></returns>
        Lib.Models.Processed.DataStewardship.Event.Event CreateEventObservation(TEntity verbatim);
    }
}