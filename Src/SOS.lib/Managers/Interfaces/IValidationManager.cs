using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.Validation;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Managers.Interfaces
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
        /// <param name="dataProvider"></param>
        /// <returns>Invalid items</returns>
        ICollection<InvalidObservation> ValidateObservations(ref ICollection<Observation> observations, DataProvider dataProvider);

        /// <summary>
        /// Checks if an observation is valid or not.
        /// </summary>
        /// <param name="observation"></param>
        /// <param name="dataProvider"></param>
        /// <returns></returns>
        public InvalidObservation ValidateObservation(Observation observation, DataProvider dataProvider);

        /// <summary>
        /// Make sure we have a invalid items collection
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        Task VerifyCollectionAsync(JobRunModes mode);
    }
}
