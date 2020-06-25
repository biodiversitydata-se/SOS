using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.Validation;

namespace SOS.Process.Models
{
    public class CreateProcessedObservationResult
    {
        public bool Succeeded { get; set; }
        public ProcessedObservation ProcessedObservation { get; set; }
        public InvalidObservation InvalidObservation { get; set; }

        public static CreateProcessedObservationResult Success(ProcessedObservation observation)
        {
            return new CreateProcessedObservationResult()
            {
                Succeeded = true,
                ProcessedObservation = observation
            };
        }

        public static CreateProcessedObservationResult Invalid(InvalidObservation invalidObservation)
        {
            return new CreateProcessedObservationResult()
            {
                Succeeded = true,
                InvalidObservation = invalidObservation
            };
        }
    }
}