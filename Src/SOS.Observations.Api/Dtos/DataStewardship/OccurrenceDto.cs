using Nest;
using SOS.Observations.Api.Dtos.DataStewardship.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace SOS.Observations.Api.Dtos.DataStewardship
{
    /// <summary>
    /// A specific observation of an organism or a uniform group of organisms made at an event.
    /// </summary>
    
    [SwaggerSchema("Metadata about a occurrence.", Required = new[] { "Identifier" })]
    public class OccurrenceDto
    {
        /// <summary>
        /// An identifier for the Occurrence (as opposed to a particular digital record of the occurrence). In the absence of a persistent global unique identifier, construct one from a combination of identifiers in the record that will most closely make the occurrenceID globally unique. (Source: Darwin Core quick reference guide)
        /// </summary>
        [Required]
        public string OccurrenceID { get; set; }

        /// <summary>
        /// The type of species observation the record deals with (e.g. human observation, material sample etc.)
        /// </summary>
        [Required]
        public BasisOfRecord? BasisOfRecord { get; set; }

        /// <summary>
        /// The specific time for the specific species observation (local time). (A general time/date for all species observations made during a survey event is given in the object inventeringstillfälle.)
        /// </summary>
        public DateTime? ObservationTime { get; set; }

        /// <summary>
        /// Date and Time for when the survey started (local time).
        [Required]
        public DateTime? EventStartDate { get; set; }

        /// <summary>
        /// Date and Time for when the survey ended (local time).
        /// </summary>
        [Required]
        public DateTime? EventEndDate { get; set; }

        /// <summary>
        /// The specific place where an organism was located when it was observed.
        /// </summary>
        public IGeoShape ObservationPoint { get; set; }

        /// <summary>
        /// Taxonomic information about the observation. States which species (or subspecies, species aggregation, genus, family etc) was observed and identified.
        /// </summary>
        [Required]
        public TaxonDto Taxon { get; set; }

        /// <summary>
        /// States whether a specific taxon was observed or not. Observations with "Förekomst" = "inte observerad" are so called zero observations.
        /// </summary>
        [Required]
        public OccurrenceStatus? OccurrenceStatus { get; set; }

        /// <summary>
        /// States how the quantity of the observed species (or other taxonomic rank) was counted or measured.
        /// </summary>
        public QuantityVariable? QuantityVariable { get; set; }

        /// <summary>
        /// Amount of organisms of a certain unit (given in the attribute unit).
        /// </summary>
        public double? Quantity { get; set; }

        /// <summary>
        /// Unit for a certain amount of organisms (given in the attribute quantity).
        /// </summary>
        public Enums.Unit? Unit { get; set; }

        /// <summary>
        /// Organism
        /// </summary>
        public OrganismVariableDto Organism { get; set; }

        /// <summary>
        /// Comment (freetext) about the species observation.
        /// </summary>
        public string OccurrenceRemarks { get; set; }

        /// <summary>
        /// States the radius within which the actual observation was made from the most specific coordinates given, i.e. the coordinates for the site, subsite or observation point.
        /// </summary>
        public double? ObservationCertainty { get; set; }

        /// <summary>
        /// States the quality of a species observation, i.e. whether its verified by an expert or similar.
        /// </summary>
        public string IdentificationVerificationStatus { get; set; }

        /// <summary>
        /// Attached information about the species observation, e.g. media files like images, sound recordings etc. Can also be collected physical samples that can be referenced.
        /// </summary>
        public List<AssociatedMediaDto> AssociatedMedia { get; set; }

        /// <summary>
        /// A unique identity of the event where the observation were made.
        /// </summary>
        public string EventID { get; set; }

        /// <summary>
        /// Data stewardship dataset identifier.
        /// </summary>        
        public DatasetInfoDto Dataset { get; set; }
    }
}
