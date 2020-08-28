using System.Collections.Generic;

namespace SOS.Lib.Models.DataValidation
{
    public class DwcaDataValidationSummary<TVerbatim, TProcessed>
    {
        public int TotalNumberOfObservationsInFile { get; set; }
        public int NrObservationsProcessed { get; set; }
        public int NrValidObservations { get; set; }
        public int NrInvalidObservations { get; set; }
        public List<string> Remarks { get; set; }
        public List<ValidObservationTuple<TVerbatim, TProcessed>> ValidObservations { get; set; }
        public List<InvalidObservationTuple<TVerbatim>> InvalidObservations { get; set; }
    }
}