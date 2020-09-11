using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
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
        /// <param name="observations"></param>
        /// <returns>Invalid items</returns>
        ICollection<InvalidObservation> ValidateObservations(ref ICollection<ProcessedObservation> observations);

        /// <summary>
        /// Checks if an observation is valid or not.
        /// </summary>
        /// <param name="observation"></param>
        /// <returns></returns>
        public InvalidObservation ValidateObservation(ProcessedObservation observation);

        /// <summary>
        /// Make sure we have a invalid items collection
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        Task VerifyCollectionAsync(JobRunModes mode);
    }
}
