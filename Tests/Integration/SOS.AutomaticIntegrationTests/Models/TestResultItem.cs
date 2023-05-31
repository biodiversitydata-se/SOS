using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.AutomaticIntegrationTests.Models
{
    public class TestResultItem
    {
        public ArtportalenObservationVerbatim VerbatimObservation { get; set; }
        public Observation ProcessedObservation { get; set; }
        public object VerbatimValue { get; set; }
        public object ProcessedValue { get; set; }
        public bool Hit { get; set; }

        public override string ToString()
        {
            return $"Value={ProcessedValue}, Hit={Hit}";
        }
    }
}