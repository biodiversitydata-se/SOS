using System;

namespace SOS.Harvest.Entities.ObservationsDatabase
{
    public class ObservationEntity
    {
        public int? CoordinateUncertaintyInMeters { get; set; }

        public double? CoordinateX { get; set; }

        public double? CoordinateY { get; set; }

        public string? CollectionCode { get; set; }

        public string? CollectionId { get; set; }

        public string? County { get; set; }
        
        public DateTime EditDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Habitat { get; set; }

        public int Id { get; set; }

        public string? IndividualCount { get; set; }

        public bool IsNeverFoundObservation { get; set; }

        public bool IsNotRediscoveredObservation { get; set; }

        public string? Locality { get; set; }

        public string? Municipality { get; set; }

        public string? Observers { get; set; }

        public string? OccurrenceRemarks { get; set; }

        public string? Origin { get; set; }

        public string? Parish { get; set; }

        public int ProtectionLevel { get; set; }

        public string? Province { get; set; }

        public DateTime RegisterDate { get; set; }

        public string? SCI_code { get; set; }

        public string? SCI_name { get; set; }

        public string? Stadium { get; set; }

        public DateTime? StartDate { get; set; }

        public string? Substrate { get; set; }

        public int TaxonId { get; set; }

        public string? VerifiedBy { get; set; }
    }
}
