using SOS.DataStewardship.Api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// 
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
        public enum BasisOfRecordEnum
        {
            /// <summary>
            /// fysiskt prov
            /// </summary>
            [EnumMember(Value = "fysiskt prov")]
            FysisktProv = 0,
            /// <summary>
            /// maskinell observation
            /// </summary>
            [EnumMember(Value = "maskinell observation")]
            MaskinellObservation = 1,
            /// <summary>
            /// mänsklig observation
            /// </summary>
            [EnumMember(Value = "mänsklig observation")]
            MänskligObservation = 2,
            /// <summary>
            /// okänt
            /// </summary>
            [EnumMember(Value = "okänt")]
            Okänt = 3
        }

        /// <summary>
        /// The type of species observation the record deals with (e.g. human observation, material sample etc.)
        /// </summary>
        [Required]
        [DataMember(Name="basisOfRecord")]
        public BasisOfRecordEnum? BasisOfRecord { get; set; }

        /// <summary>
        /// The specific time for the specific species observation (local time). (A general time/date for all species observations made during a survey event is given in the object inventeringstillfälle.)
        /// </summary>
        [DataMember(Name="observationTime")]
        public DateTime? ObservationTime { get; set; }

        /// <summary>
        /// The specific place where an organism was located when it was observed.
        /// </summary>
        [DataMember(Name="observationPoint")]
        public Object ObservationPoint { get; set; }

        /// <summary>
        /// Taxon
        /// </summary>
        [Required]
        [DataMember(Name="taxon")]
        public TaxonModel Taxon { get; set; }

        /// <summary>
        /// States whether a specific taxon was observed or not. Observations with \"Förekomst\" = \"inte observerad\" are so called zero observations.
        /// </summary>
        public enum OccurrenceStatusEnum
        {
            /// <summary>
            /// inte observerad
            /// </summary>
            [EnumMember(Value = "inte observerad")]
            InteObserverad = 0,
            /// <summary>
            /// observerad
            /// </summary>
            [EnumMember(Value = "observerad")]
            Observerad = 1
        }

        /// <summary>
        /// States whether a specific taxon was observed or not. Observations with \&quot;Förekomst\&quot; &#x3D; \&quot;inte observerad\&quot; are so called zero observations.
        /// </summary>
        [Required]
        [DataMember(Name="occurrenceStatus")]
        public OccurrenceStatusEnum? OccurrenceStatus { get; set; }

        /// <summary>
        /// States how the quantity of the observed species (or other taxonomic rank) was counted or measured.
        /// </summary>
        public enum QuantityVariableEnum
        {
            /// <summary>
            ///  Antal individer
            /// </summary>
            [EnumMember(Value = "antal individer")]
            AntalIndivider = 0,
            /// <summary>
            ///  Antal fruktkroppar
            /// </summary>
            [EnumMember(Value = "antal fruktkroppar")]
            AntalFruktkroppar = 1,
            /// <summary>
            ///  Antal kapslar
            /// </summary>
            [EnumMember(Value = "antal kapslar")]
            AntalKapslar = 2,
            /// <summary>
            ///  Antal plantor/tuvor
            /// </summary>
            [EnumMember(Value = "antal plantor/tuvor")]
            AntalPlantorTuvor = 3,
            /// <summary>
            ///  Antal stjälkar/strån/skott
            /// </summary>
            [EnumMember(Value = "antal stjälkar/strån/skott")]
            AntalStjälkarStrånSkott = 4,
            /// <summary>
            ///  Antal äggklumpar
            /// </summary>
            [EnumMember(Value = "antal äggklumpar")]
            AntalÄggklumpar = 5,
            /// <summary>
            ///  Täckningsgrad
            /// </summary>
            [EnumMember(Value = "täckningsgrad")]
            Täckningsgrad = 6,
            /// <summary>
            ///  Yttäckning
            /// </summary>
            [EnumMember(Value = "yttäckning")]
            Yttäckning = 7
        }

        /// <summary>
        /// States how the quantity of the observed species (or other taxonomic rank) was counted or measured.
        /// </summary>
        [DataMember(Name="quantityVariable")]
        public QuantityVariableEnum? QuantityVariable { get; set; }

        /// <summary>
        /// Amount of organisms of a certain unit (given in the attribute unit).
        /// </summary>
        [DataMember(Name="quantity")]
        public decimal? Quantity { get; set; }

        /// <summary>
        /// Unit for a certain amount of organisms (given in the attribute quantity).
        /// </summary>
        public enum UnitEnum
        {
            /// <summary>
            /// %
            /// </summary>
            [EnumMember(Value = "%")]
            Percent = 0,
            /// <summary>
            /// cm²
            /// </summary>
            [EnumMember(Value = "cm²")]
            Cm2 = 1,
            /// <summary>
            /// cm³
            /// </summary>
            [EnumMember(Value = "cm³")]
            Cm3 = 2,
            /// <summary>
            /// dm²
            /// </summary>
            [EnumMember(Value = "dm²")]
            Dm2 = 3,
            /// <summary>
            /// kompassgrader
            /// </summary>
            [EnumMember(Value = "kompassgrader")]
            Kompassgrader = 4,
            /// <summary>
            /// m/s
            /// </summary>
            [EnumMember(Value = "m/s")]
            Ms = 5,
            /// <summary>
            /// m²
            /// </summary>
            [EnumMember(Value = "m²")]
            M2 = 6,
            /// <summary>
            /// styck
            /// </summary>
            [EnumMember(Value = "styck")]
            Styck = 7,
            /// <summary>
            /// °C
            /// </summary>
            [EnumMember(Value = "°C")]
            GraderCelsius = 8
        }

        /// <summary>
        /// Unit for a certain amount of organisms (given in the attribute quantity).
        /// </summary>
        [DataMember(Name="unit")]
        public UnitEnum? Unit { get; set; }

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
        public enum IdentificationVerificationStatusEnum
        {
            /// <summary>
            /// värdelista saknas
            /// </summary>
            [EnumMember(Value = "värdelista saknas")]
            VärdelistaSaknas = 0
        }

        /// <summary>
        /// States the quality of a species observation, i.e. whether its verified by an expert or similar. Quality categories are chosen from a codelist.
        /// </summary>
        [DataMember(Name="identificationVerificationStatus")]
        public IdentificationVerificationStatusEnum? IdentificationVerificationStatus { get; set; }

        /// <summary>
        /// Attached information about the species observation, e.g. media files like images, sound recordings etc. Can also be collected physical samples that can be referenced.
        /// </summary>
        [DataMember(Name="associatedMedia")]
        public List<AssociatedMedia> AssociatedMedia { get; set; }

        /// <summary>
        /// A unique identity of the event where the observation were made.
        /// </summary>
        [DataMember(Name="event")]
        public string Event { get; set; }
    }
}
