using System;
using MongoDB.Bson;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.ClamTreePortal
{
    /// <summary>
    /// Tree verbatim model
    /// </summary>
    public class TreeObservationVerbatim : IEntity<ObjectId>
    {
         public string AccessRights { get; set; }
        public string BasisOfRecord { get; set; }
        public int CatalogNumber { get; set; }
        public int? CoordinateUncertaintyInMeters { get; set; }
        public decimal CoordinateX { get; set; }
        public decimal CoordinateY { get; set; }
        public string CountryCode { get; set; }
        public int? CountyId { get; set; }
        public int DataProviderId { get; set; }
        public double DecimalLatitude { get; set; }
        public double DecimalLongitude { get; set; }
        public int? DyntaxaTaxonId { get; set; }
        public string EstablishmentMeans { get; set; }
        public ObjectId Id { get; set; }
        public string IdentificationVerificationStatus { get; set; }
        public string IndividualCount { get; set; }
        public string InstitutionCode { get; set; }
        public bool IsNaturalOccurrence { get; set; }
        public bool IsNeverFoundObservation { get; set; }
        public bool? IsNotRediscoveredObservation { get; set; }
        public bool? IsPositiveObservation { get; set; }
        public string Language { get; set; }
        public string LifeStage { get; set; }
        public string LocationId { get; set; }
        public string Locality { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime ObservationDate { get; set; }
        public string OccurrenceId { get; set; }
        public string OccurrenceRemarks { get; set; }
        public string OccurrenceStatus { get; set; }
        public string Owner { get; set; }
        public string ProjectName { get; set; }
        public string Quantity { get; set; }
        public string QuantityUnit { get; set; }
        public string RecordedBy { get; set; }
        public string ReportedBy { get; set; }
        public DateTime ReportedDate { get; set; }
        public string RightsHolder { get; set; }
        public int UncertainDetermination { get; set; }
        public string VerbatimScientificName { get; set; }
        public string Circumference { get; set; }
        public string Reasons { get; set; }
        public string UndefinedTreeType { get; set; }
    }
}
