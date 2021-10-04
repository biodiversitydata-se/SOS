using System;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.Mvm
{
    public class MvmObservationVerbatim : IEntity<int>
    {
        public string CatalogNumber { get; set; }
        public int? CoordinateUncertaintyInMeters { get; set; }

        public string County { get; set; }

        public double DecimalLatitude { get; set; }

        public double DecimalLongitude { get; set; }

        public int DyntaxaTaxonId { get; set; }

        public DateTime End { get; set; }
        public string Habitat { get; set; }

        public string IndividualCount { get; set; }

        public string IndividualId { get; set; }

        public bool IsPositiveObservation { get; set; }

        public bool IsPublic { get; set; }
        
        public string Locality { get; set; }

        public string LocationId { get; set; }

        public string LocationRemarks { get; set; }

        public DateTime? Modified { get; set; }

        public string Municipality { get; set; }

        public string OccurrenceId { get; set; }

        public string OccurrenceStatus { get; set; }

        public string Owner { get; set; }

        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        
        public string RecordedBy { get; set; }

        public string ReportedBy { get; set; }

        public DateTime ReportedDate { get; set; }

        public string ScientificName { get; set; }

        public DateTime Start { get; set; }

        public int Id { get; set; }

        public string ProductName { get; set; }
    }
}