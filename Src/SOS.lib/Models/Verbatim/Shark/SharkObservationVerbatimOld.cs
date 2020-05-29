using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.Shark
{
    /// <summary>
    ///     Verbatim from Shark
    /// </summary>
    public class SharkObservationVerbatimold : IEntity<ObjectId>
    {
        /// <summary>
        ///     Analytical laboratory code
        /// </summary>
        public string AnalyticalLaboratoryCode { get; set; }

        /// <summary>
        ///     Class
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        ///     Dataset name
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        ///     Dataset file name
        /// </summary>
        public string DatasetFileName { get; set; }

        /// <summary>
        ///     Data Type
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        ///     Day
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        ///     Latitude
        /// </summary>
        public double DecimalLatitude { get; set; }

        /// <summary>
        ///     Longitude
        /// </summary>
        public double DecimalLongitude { get; set; }

        /// <summary>
        ///     Dyntaxa taxon id
        /// </summary>
        public int DyntaxaTaxonId { get; set; }

        /// <summary>
        ///     Date of event
        /// </summary>
        public DateTime EventDate { get; set; }

        /// <summary>
        ///     Family
        /// </summary>
        public string Family { get; set; }

        /// <summary>
        ///     Genus
        /// </summary>
        public string Genus { get; set; }

        /// <summary>
        ///     Kingdom
        /// </summary>
        public string Kingdom { get; set; }

        /// <summary>
        ///     Maximum depth in meters
        /// </summary>
        public double? MaximumDepthInMeters { get; set; }

        /// <summary>
        ///     Minimum depth in meters
        /// </summary>
        public double? MinimumDepthInMeters { get; set; }

        /// <summary>
        ///     Month
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        ///     Occurrence id
        /// </summary>
        public string OccurrenceId { get; set; }

        /// <summary>
        ///     Order
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        ///     Owner institution code
        /// </summary>
        public string OwnerInstitutionCode { get; set; }

        /// <summary>
        ///     Phylum
        /// </summary>
        public string Phylum { get; set; }

        /// <summary>
        ///     Reporting institution code
        /// </summary>
        public string ReportingInstitutionCode { get; set; }

        /// <summary>
        ///     Recorded by
        /// </summary>
        public string RecordedBy { get; set; }

        /// <summary>
        ///     Sampler type
        /// </summary>
        public string SamplerType { get; set; }

        /// <summary>
        ///     Taxon scientific name
        /// </summary>
        public string ScientificName { get; set; }

        /// <summary>
        ///     Scientific name authorship
        /// </summary>
        public string ScientificNameAuthorship { get; set; }

        /// <summary>
        ///     Species
        /// </summary>
        public string Species { get; set; }

        /// <summary>
        ///     Status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     Year
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        ///     Unique id
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; }
    }
}