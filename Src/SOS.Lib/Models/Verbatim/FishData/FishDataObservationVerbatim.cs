using SOS.Lib.Models.Interfaces;
using System;

namespace SOS.Lib.Models.Verbatim.FishData
{
    public class FishDataObservationVerbatim : IEntity<int>
    {
        public int? CoordinateUncertaintyInMeters { get; set; }

        public string County { get; set; }

        public double DecimalLatitude { get; set; }

        public double DecimalLongitude { get; set; }

        public int DyntaxaTaxonId { get; set; }

        public DateTime End { get; set; }

        public int Id { get; set; }
        public int? IndividualId { get; set; }

        public int? LocationId { get; set; }

        public string Locality { get; set; }

        public DateTime? Modified { get; set; }

        public string Municipality { get; set; }

        public string OccurrenceId { get; set; }

        public string Owner { get; set; }

        public string RecordedBy { get; set; }

        public string ReportedBy { get; set; }

        public string ScientificName { get; set; }

        public DateTime Start { get; set; }
    }
}