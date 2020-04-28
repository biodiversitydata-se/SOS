using System;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.Mvm
{
    public class MvmObservationVerbatim : IEntity<string>
    {
        public string Id
        {
            get => OccurrenceId;
            set => OccurrenceId = value;
        }

        public int? CoordinateUncertaintyInMeters { get; set; }
        
        public string County { get; set; }

        public double DecimalLatitude { get; set; }

        public double DecimalLongitude { get; set; }

        public int DyntaxaTaxonId { get; set; }

        public DateTime End { get; set; }

        public string IndividualId { get; set; }

        public string Locality { get; set; }

        public string LocationId { get; set; }

        public string Modified { get; set; }

        public string Municipality { get; set; }

        public string OccurrenceId { get; set; }

        public string Owner { get; set; }

        public string RecordedBy { get; set; }

        public string ReportedBy { get; set; }
        public string ScientificName { get; set; }

        public DateTime Start { get; set; }
    }
}
