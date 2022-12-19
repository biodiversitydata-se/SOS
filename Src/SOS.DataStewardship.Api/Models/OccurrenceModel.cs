using SOS.DataStewardship.Api.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// A specific observation of an organism or a uniform group of organisms made at an event.
    /// </summary>
    [DataContract]
    public class OccurrenceModel
    { 
        /// <summary>
        /// An identifier for the Occurrence (as opposed to a particular digital record of the occurrence). In the absence of a persistent global unique identifier, construct one from a combination of identifiers in the record that will most closely make the occurrenceID globally unique. (Source: Darwin Core quick reference guide)
        /// </summary>
        [Required]
        [DataMember(Name="occurrenceID")]
        public string OccurrenceID { get; set; }        

        /// <summary>
        /// The type of species observation the record deals with (e.g. human observation, material sample etc.)
        /// </summary>
        [Required]
        [DataMember(Name="basisOfRecord")]
        public BasisOfRecord? BasisOfRecord { get; set; }

        /// <summary>
        /// The specific time for the specific species observation (local time). (A general time/date for all species observations made during a survey event is given in the object inventeringstillfälle.)
        /// </summary>
        [DataMember(Name="observationTime")]
        public DateTime? ObservationTime { get; set; }

        /// <summary>
        /// Date and Time for when the survey started (local time).
        /// </summary>
        /// <remarks>
        /// Newly added property
        /// </remarks>
        /// <value>Date and Time for when the survey started (local time).</value>
        [Required]
        [DataMember(Name = "eventStartDate")]
        public DateTime? EventStartDate { get; set; }

        /// <summary>
        /// Date and Time for when the survey ended (local time).
        /// </summary>
        /// <remarks>
        /// Newly added property
        /// </remarks>
        [Required]
        [DataMember(Name = "eventEndDate")]
        public DateTime? EventEndDate { get; set; }

        /// <summary>
        /// The specific place where an organism was located when it was observed.
        /// </summary>
        [DataMember(Name="observationPoint")]
        public IGeoShape ObservationPoint { get; set; }

        // Decide if to observationPoint should use IGeoShape or GeometryObject data type.
        [DataMember(Name = "observationPointTest")]
        public GeometryObject ObservationPointTest { get; set; }

        /// <summary>
        /// Taxonomic information about the observation. States which species (or subspecies, species aggregation, genus, family etc) was observed and identified.
        /// </summary>
        [Required]
        [DataMember(Name="taxon")]
        public TaxonModel Taxon { get; set; }        

        /// <summary>
        /// States whether a specific taxon was observed or not. Observations with \&quot;Förekomst\&quot; &#x3D; \&quot;inte observerad\&quot; are so called zero observations.
        /// </summary>
        [Required]
        [DataMember(Name="occurrenceStatus")]
        public OccurrenceStatus? OccurrenceStatus { get; set; }

        /// <summary>
        /// States how the quantity of the observed species (or other taxonomic rank) was counted or measured.
        /// </summary>
        [DataMember(Name="quantityVariable")]
        public QuantityVariable? QuantityVariable { get; set; }

        /// <summary>
        /// Amount of organisms of a certain unit (given in the attribute unit).
        /// </summary>
        [DataMember(Name="quantity")]
        public decimal? Quantity { get; set; }

        /// <summary>
        /// Unit for a certain amount of organisms (given in the attribute quantity).
        /// </summary>
        [DataMember(Name="unit")]
        public Unit? Unit { get; set; }

        /// <summary>
        /// Organism
        /// </summary>
        [DataMember(Name="organism")]
        public OrganismVariable Organism { get; set; }

        /// <summary>
        /// Comment (freetext) about the species observation.
        /// </summary>
        [DataMember(Name="occurrenceRemarks")]
        public string OccurrenceRemarks { get; set; }

        /// <summary>
        /// States the radius within which the actual observation was made from the most specific coordinates given, i.e. the coordinates for the site, subsite or observation point.
        /// </summary>
        [DataMember(Name="observationCertainty")]
        public decimal? ObservationCertainty { get; set; }

        /// <summary>
        /// States the quality of a species observation, i.e. whether its verified by an expert or similar. Quality categories are chosen from a codelist.
        /// </summary>
        [DataMember(Name="identificationVerificationStatus")]
        public IdentificationVerificationStatus? IdentificationVerificationStatus { get; set; }

        /// <summary>
        /// Attached information about the species observation, e.g. media files like images, sound recordings etc. Can also be collected physical samples that can be referenced.
        /// </summary>
        [DataMember(Name="associatedMedia")]
        public List<AssociatedMedia> AssociatedMedia { get; set; }

        /// <summary>
        /// A unique identity of the event where the observation were made.
        /// </summary>
        [DataMember(Name="eventID")]
        public string EventID { get; set; }

        /// <summary>
        /// Data stewardship dataset identifier.
        /// </summary>
        /// <remarks>
        /// Newly added property
        /// </remarks>
        public string DatasetIdentifier { get; set; }
    }
}
