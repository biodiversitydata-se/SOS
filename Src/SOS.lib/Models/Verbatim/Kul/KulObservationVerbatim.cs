using System;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.Kul
{
    public class KulObservationVerbatim : IEntity<string>
    {
        public string Id
        {
            get => OccurrenceId;
            set => OccurrenceId = value;
        }
        public string OccurrenceId { get; set; }
        public int DyntaxaTaxonId { get; set; }
        public double DecimalLongitude { get; set; }
        public double DecimalLatitude { get; set; }
        public int? CoordinateUncertaintyInMeters { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string ReportedBy { get; set; }
        public string RecordedBy { get; set; }
        public string Owner { get; set; }
        public string Locality { get; set; }
        public string VerbatimScientificName { get; set; }
        public string TaxonRemarks { get; set; }
        public int? IndividualCount { get; set; }
        public string CountryCode { get; set; }
        public string AssociatedOccurrences { get; set; }
    }
}
