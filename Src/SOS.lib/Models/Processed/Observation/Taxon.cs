using System.Collections.Generic;
using Nest;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     This class contains taxon information about a species observation.
    /// </summary>
    public class Taxon : IEntity<int>, IBasicTaxon
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Taxon()
        {
            Attributes = new TaxonAttributes();
        }

        /// <summary>        
        ///     The full name, with authorship and date information
        ///     if known, of the currently valid (zoological) or
        ///     accepted (botanical) taxon.
        /// </summary>
        public string AcceptedNameUsage { get; set; }

        /// <summary>        
        ///     An identifier for the name usage (documented meaning of
        ///     the name according to a source) of the currently valid
        ///     (zoological) or accepted (botanical) taxon.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string AcceptedNameUsageID { get; set; }

        /// <summary>
        /// Taxon attributes.
        /// </summary>
        public TaxonAttributes Attributes { get; set; }

        /// <summary>
        ///     Part of bird directive?
        /// </summary>
        public bool? BirdDirective { get; set; }

        /// <summary>        
        ///     The full scientific name of the class in which
        ///     the taxon is classified.
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        ///     The full scientific name of the family in which
        ///     the taxon is classified.
        /// </summary>
        public string Family { get; set; }

        /// <summary>
        ///     The full scientific name of the genus in which
        ///     the taxon is classified.
        /// </summary>
        public string Genus { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of taxa names
        ///     terminating at the rank immediately superior to the
        ///     taxon referenced in the taxon record.
        ///     Recommended best practice is to order the list
        ///     starting with the highest rank and separating the names
        ///     for each rank with a semi-colon (";").
        /// </summary>
        public string HigherClassification { get; set; }

        /// <summary>
        ///     Object id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     The name of the lowest or terminal infraspecific epithet
        ///     of the scientificName, excluding any rank designation.
        /// </summary>
        public string InfraspecificEpithet { get; set; }

        /// <summary>
        ///     The full scientific name of the kingdom in which the
        ///     taxon is classified.
        /// </summary>
        public string Kingdom { get; set; }

        /// <summary>
        ///     The reference to the source in which the specific
        ///     taxon concept circumscription is defined or implied -
        ///     traditionally signified by the Latin "sensu" or "sec."
        ///     (from secundum, meaning "according to").
        ///     For taxa that result from identifications, a reference
        ///     to the keys, monographs, experts and other sources should
        ///     be given.
        /// </summary>
        public string NameAccordingTo { get; set; }

        /// <summary>
        ///     An identifier for the source in which the specific
        ///     taxon concept circumscription is defined or implied.
        ///     See nameAccordingTo.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string NameAccordingToID { get; set; }

        /// <summary>
        ///     A reference for the publication in which the
        ///     scientificName was originally established under the rules
        ///     of the associated nomenclaturalCode.
        /// </summary>
        public string NamePublishedIn { get; set; }

        /// <summary>
        ///     An identifier for the publication in which the
        ///     scientificName was originally established under the
        ///     rules of the associated nomenclaturalCode.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string NamePublishedInId { get; set; }

        /// <summary>
        ///     The four-digit year in which the scientificName
        ///     was published.
        /// </summary>
        public string NamePublishedInYear { get; set; }

        /// <summary>
        ///     The nomenclatural code (or codes in the case of an
        ///     ambiregnal name) under which the scientificName is
        ///     constructed.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string NomenclaturalCode { get; set; }

        /// <summary>
        ///     The status related to the original publication of the
        ///     name and its conformance to the relevant rules of
        ///     nomenclature. It is based essentially on an algorithm
        ///     according to the business rules of the code.
        ///     It requires no taxonomic opinion.
        /// </summary>
        public string NomenclaturalStatus { get; set; }

        /// <summary>
        ///     The full scientific name of the order in which
        ///     the taxon is classified.
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        ///     The taxon name, with authorship and date information
        ///     if known, as it originally appeared when first established
        ///     under the rules of the associated nomenclaturalCode.
        ///     The basionym (botany) or basonym (bacteriology) of the
        ///     scientificName or the senior/earlier homonym for replaced
        ///     names.
        /// </summary>
        public string OriginalNameUsage { get; set; }

        /// <summary>
        ///     An identifier for the name usage (documented meaning of
        ///     the name according to a source) in which the terminal
        ///     element of the scientificName was originally established
        ///     under the rules of the associated nomenclaturalCode.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string OriginalNameUsageId { get; set; }

        /// <summary>
        ///     The full name, with authorship and date information
        ///     if known, of the direct, most proximate higher-rank
        ///     parent taxon (in a classification) of the most specific
        ///     element of the scientificName.
        /// </summary>
        public string ParentNameUsage { get; set; }

        /// <summary>
        ///     An identifier for the name usage (documented meaning
        ///     of the name according to a source) of the direct,
        ///     most proximate higher-rank parent taxon
        ///     (in a classification) of the most specific
        ///     element of the scientificName.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string ParentNameUsageId { get; set; }

        /// <summary>
        ///     The full scientific name of the phylum or division
        ///     in which the taxon is classified.
        /// </summary>
        public string Phylum { get; set; }

        /// <summary>
        ///     The full scientific name, with authorship and date
        ///     information if known. When forming part of an
        ///     Identification, this should be the name in lowest level
        ///     taxonomic rank that can be determined.
        ///     This term should not contain identification qualifications,
        ///     which should instead be supplied in the
        ///     IdentificationQualifier term.
        ///     Currently scientific name without author is provided.
        /// </summary>
        [Keyword]
        public string ScientificName { get; set; }

        /// <summary>
        ///     The authorship information for the scientificName
        ///     formatted according to the conventions of the applicable
        ///     nomenclaturalCode.
        /// </summary>
        public string ScientificNameAuthorship { get; set; }

        /// <summary>
        ///     An identifier for the nomenclatural (not taxonomic)
        ///     details of a scientific name.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string ScientificNameId { get; set; }

        /// <summary>
        ///     Secondary parents dyntaxa taxon ids.
        /// </summary>
        public IEnumerable<int> SecondaryParentDyntaxaTaxonIds { get; set; }

        /// <summary>
        ///     The name of the first or species epithet of
        ///     the scientificName.
        /// </summary>
        public string SpecificEpithet { get; set; }

        /// <summary>
        ///     The full scientific name of the subgenus in which
        ///     the taxon is classified. Values should include the
        ///     genus to avoid homonym confusion.
        /// </summary>
        public string Subgenus { get; set; }

        /// <summary>
        ///     An identifier for the taxonomic concept to which the record
        ///     refers - not for the nomenclatural details of a taxon.
        ///     In SwedishSpeciesObservationSOAPService this property
        ///     has the same value as property TaxonID.
        ///     GUID in Dyntaxa is used as value for this property.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string TaxonConceptId { get; set; }

        /// <summary>
        ///     An identifier for the set of taxon information
        ///     (data associated with the Taxon class). May be a global
        ///     unique identifier or an identifier specific to the data set.
        ///     In SwedishSpeciesObservationSOAPService this property
        ///     has the same value as property TaxonConceptID.
        ///     GUID in Dyntaxa is used as value for this property.
        /// </summary>
        public string TaxonId { get; set; }

        /// <summary>
        ///     The status of the use of the scientificName as a label
        ///     for a taxon. Requires taxonomic opinion to define the
        ///     scope of a taxon. Rules of priority then are used to
        ///     define the taxonomic status of the nomenclature contained
        ///     in that scope, combined with the experts opinion.
        ///     It must be linked to a specific taxonomic reference that
        ///     defines the concept.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string TaxonomicStatus { get; set; }

        /// <summary>
        ///     The taxonomic rank of the most specific name in the
        ///     scientificName. Recommended best practice is to use
        ///     a controlled vocabulary.
        /// </summary>
        public string TaxonRank { get; set; }

        /// <summary>
        ///     Comments or notes about the taxon or name.
        /// </summary>
        public string TaxonRemarks { get; set; }

        /// <summary>
        ///     The taxonomic rank of the most specific name in the
        ///     scientificName as it appears in the original record.
        /// </summary>
        public string VerbatimTaxonRank { get; set; }

        /// <summary>
        ///     A common or vernacular name.
        /// </summary>
        public string VernacularName { get; set; }
    }
}