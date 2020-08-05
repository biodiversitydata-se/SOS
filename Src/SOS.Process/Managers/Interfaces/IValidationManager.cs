using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.Validation;

namespace SOS.Process.Managers.Interfaces
{
    public interface IValidationManager
    {
        /// <summary>
        /// Save invalid observations.
        /// </summary>
        /// <param name="invalidObservations"></param>
        /// <returns></returns>
        Task<bool> AddInvalidObservationsToDb(ICollection<InvalidObservation> invalidObservations);

        /// <summary>
        ///     Validate observations.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>Invalid items</returns>
        ICollection<InvalidObservation> ValidateObservations(ref ICollection<ProcessedObservation> items);

        /// <summary>
        /// Make sure we have a invalid items collection
        /// </summary>
        /// <param name="incrementalMode"></param>
        /// <returns></returns>
        Task VerifyCollectionAsync(bool incrementalMode);
    }
}
