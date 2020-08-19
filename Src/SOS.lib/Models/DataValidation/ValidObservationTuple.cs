namespace SOS.Lib.Models.DataValidation
{
    public class ValidObservationTuple<TVerbatim, TProcessed>
    {
        public TVerbatim VerbatimObservation { get; set; }
        public TProcessed ProcessedObservation { get; set; }
    }
}