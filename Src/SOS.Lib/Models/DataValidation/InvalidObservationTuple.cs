using System.Collections.Generic;

namespace SOS.Lib.Models.DataValidation
{
    public class InvalidObservationTuple<TVerbatim>
    {
        public TVerbatim VerbatimObservation { get; set; }
        public ICollection<string> ProcessedObservationDefects { get; set; }
    }
}