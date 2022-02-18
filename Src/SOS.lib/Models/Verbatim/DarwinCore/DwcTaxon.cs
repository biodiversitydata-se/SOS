
namespace SOS.Lib.Models.Verbatim.DarwinCore
{
    public class DwcTaxon
    {
        /// <summary>
        ///     Darwin Core term name: kingdom.
        ///     The full scientific name of the kingdom in which the
        ///     taxon is classified.
        /// </summary>
        public string Kingdom { get; set; }

        /// <summary>
        ///     Darwin Core term name: scientificName.
        ///     The full scientific name, with authorship and date
        ///     information if known. When forming part of an
        ///     Identification, this should be the name in lowest level
        ///     taxonomic rank that can be determined.
        ///     This term should not contain identification qualifications,
        ///     which should instead be supplied in the
        ///     IdentificationQualifier term.
        ///     Currently scientific name without author is provided.
        /// </summary>
        public string ScientificName { get; set; }

        /// <summary>
        ///     Darwin Core term name: taxonID.
        ///     An identifier for the set of taxon information
        ///     (data associated with the Taxon class). May be a global
        ///     unique identifier or an identifier specific to the data set.
        ///     In SwedishSpeciesObservationSOAPService this property
        ///     has the same value as property TaxonConceptID.
        ///     GUID in Dyntaxa is used as value for this property.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string TaxonID { get; set; }

        /// <summary>
        ///     Darwin Core term name: taxonRank.
        ///     The taxonomic rank of the most specific name in the
        ///     scientificName. Recommended best practice is to use
        ///     a controlled vocabulary.
        /// </summary>
        public string TaxonRank { get; set; }
    }
}