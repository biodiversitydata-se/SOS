using System.ComponentModel.DataAnnotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// Taxonomic information about the observation. States which species (or subspecies, species aggregation, genus, family etc) was observed and identified.
    /// </summary>
    public class Taxon
    {
        /// <summary>
        /// Unique id for species or other given taxon rank according to Dyntaxa.
        /// </summary>
        [Required]
        [RegularExpression("/(urn:lsid:dyntaxa.se:Taxon:\\S+)/")]
        public string TaxonID { get; set; }

        /// <summary>
        /// The Swedish name according to Dyntaxa for the observed organism, at the given taxonomic level.
        /// </summary>
        public string VernacularName { get; set; }

        /// <summary>
        /// The scientific name according to Dyntaxa for the observed organism, at the given taxonomic level.
        /// </summary>
        [Required]
        public string ScientificName { get; set; }

        /// <summary>
        /// The taxonomic level according to Dyntaxa at which the species identification is given.
        /// </summary>
        [Required]
        public string TaxonRank { get; set; }

        /// <summary>
        /// The Swedish name for the observed organism, at the given taxonomic level, as reported by the surveyor. Can be different from now valid taxonomy. (For traceability.)
        /// </summary>
        public string VerbatimName { get; set; }

        /// <summary>
        /// The id for species or other given level of taxonomy as reported by the surveyor. Can be different from now valid taxonomy. (For traceability.)
        /// </summary>
        public string VerbatimTaxonID { get; set; }
    }
}
