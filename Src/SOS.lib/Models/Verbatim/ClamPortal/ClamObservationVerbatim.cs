using System;
using System.Text.Json.Serialization;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.ClamPortal
{
    /// <summary>
    ///     Clams verbatim model
    /// </summary>
    public class ClamObservationVerbatim : IEntity<int>
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
        
        public string IdentificationVerificationStatus { get; set; }
        public string IndividualCount { get; set; }
        public string InstitutionCode { get; set; }
        public bool IsNaturalOccurrence { get; set; }
        public bool? IsNeverFoundObservation { get; set; }
        public bool? IsNotRediscoveredObservation { get; set; }
        public bool? IsPositiveObservation { get; set; }
        public string Language { get; set; }
        public string LifeStage { get; set; }
        public string LocationId { get; set; }
        public string Locality { get; set; }
        public string LocationRemarks { get; set; }
        
        [JsonConverter(typeof(StringToNullableDoubleConverter))]
        public double? MaximumDepthInMeters { get; set; }

        [JsonConverter(typeof(StringToNullableDateTimeConverter))]
        public DateTime? Modified { get; set; }

        [JsonConverter(typeof(StringToDateTimeConverter))]
        public DateTime ObservationDate { get; set; }
        
        public string OccurrenceId { get; set; }
        public string OccurrenceRemarks { get; set; }
        public string OccurrenceStatus { get; set; }
        public string Owner { get; set; }
        public string ProjectName { get; set; }

        [JsonConverter(typeof(StringToNullableIntConverter))]
        public int? Quantity { get; set; }

        public string QuantityUnit { get; set; }
        public string RecordedBy { get; set; }
        public string ReportedBy { get; set; }

        [JsonConverter(typeof(StringToDateTimeConverter))]
        public DateTime ReportedDate { get; set; }

        public string RightsHolder { get; set; }
        public string SurveyMethod { get; set; }

        public int UncertainDetermination { get; set; }
        
        public string VerbatimScientificName { get; set; }
        public string WaterBody { get; set; }
        public string SmallestLivingClam { get; set; }
        public int Id { get; set; }
    }
}