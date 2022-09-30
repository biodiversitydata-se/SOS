using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// Taxonomic information about the observation. States which species (or subspecies, species aggregation, genus, family etc) was observed and identified.
    /// </summary>
    [DataContract]
    public class TaxonModel
    { 
        /// <summary>
        /// Unique id for species or other given taxon rank according to Dyntaxa.
        /// </summary>
        [Required]
        [RegularExpression("/(urn:lsid:dyntaxa.se:Taxon:\\S+)/")]
        [DataMember(Name="taxonID")]
        public string TaxonID { get; set; }

        /// <summary>
        /// The Swedish name according to Dyntaxa for the observed organism, at the given taxonomic level.
        /// </summary>
        [DataMember(Name="vernacularName")]
        public string VernacularName { get; set; }

        /// <summary>
        /// The scientific name according to Dyntaxa for the observed organism, at the given taxonomic level.
        /// </summary>
        [Required]
        [DataMember(Name="scientificName")]
        public string ScientificName { get; set; }

        /// <summary>
        /// The taxonomic level according to Dyntaxa at which the species identification is given.
        /// </summary>
        [Required]
        [DataMember(Name="taxonRank")]
        public string TaxonRank { get; set; }

        /// <summary>
        /// The Swedish name for the observed organism, at the given taxonomic level, as reported by the surveyor. Can be different from now valid taxonomy. (For traceability.)
        /// </summary>
        [DataMember(Name="verbatimName")]
        public string VerbatimName { get; set; }

        /// <summary>
        /// The id for species or other given level of taxonomy as reported by the surveyor. Can be different from now valid taxonomy. (For traceability.)
        /// </summary>
        [DataMember(Name="verbatimTaxonID")]
        public string VerbatimTaxonID { get; set; }
    }
}
