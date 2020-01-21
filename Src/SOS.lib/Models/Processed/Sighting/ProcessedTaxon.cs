using System.Collections.Generic;
using Newtonsoft.Json;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Lib.Models.Processed.Sighting
{
    /// <summary>
    /// This class contains taxon information about a species
    /// observation in Darwin Core 1.5 compatible format.
    /// Further information about the properties can
    /// be found at http://rs.tdwg.org/dwc/terms/
    /// </summary>
    public class ProcessedTaxon: IEntity<int>, IBasicTaxon
    {
        /// <summary>
        /// Dyntaxa taxon id.
        /// </summary>
        public int DyntaxaTaxonId { get; set; }

        /// <summary>
        /// Main parent Dyntaxa taxon id.
        /// </summary>
        public int? ParentDyntaxaTaxonId { get; set; }

        /// <summary>
        /// Secondary parents dyntaxa taxon ids.
        /// </summary>
        public IEnumerable<int> SecondaryParentDyntaxaTaxonIds { get; set; }

        /// <summary>
        /// Vernacular names.
        /// </summary>
        public IEnumerable<TaxonVernacularName> VernacularNames { get; set; }

        /// <summary>
        /// Darwin Core term name: acceptedNameUsage.
        /// The full name, with authorship and date information
        /// if known, of the currently valid (zoological) or
        /// accepted (botanical) taxon.
        /// This property is currently not used.
        /// </summary>
        public string AcceptedNameUsage { get; set; }

        /// <summary>
        /// Darwin Core term name: acceptedNameUsageID.
        /// An identifier for the name usage (documented meaning of
        /// the name according to a source) of the currently valid
        /// (zoological) or accepted (botanical) taxon.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string AcceptedNameUsageID { get; set; }

        /// <summary>
        /// Action plan
        /// </summary>
        public string ActionPlan { get; set; }

        /// <summary>
        /// Part of bird directive?
        /// </summary>
        public bool? BirdDirective { get; set; }

        /// <summary>
        /// Darwin Core term name: class.
        /// The full scientific name of the class in which
        /// the taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// Radius of disturbance
        /// </summary>
        public int? DisturbanceRadius { get; set; }

        /// <summary>
        /// Darwin Core term name: family.
        /// The full scientific name of the family in which
        /// the taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Family { get; set; }

        /// <summary>
        /// Darwin Core term name: genus.
        /// The full scientific name of the genus in which
        /// the taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Genus { get; set; }

        /// <summary>
        /// Darwin Core term name: higherClassification.
        /// A list (concatenated and separated) of taxa names
        /// terminating at the rank immediately superior to the
        /// taxon referenced in the taxon record.
        /// Recommended best practice is to order the list
        /// starting with the highest rank and separating the names
        /// for each rank with a semi-colon (";").
        /// This property is currently not used.
        /// </summary>
        public string HigherClassification { get; set; }

        /// <summary>
        /// Object id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Individual id
        /// </summary>
        public string IndividualId { get; set; }

        /// <summary>
        /// Darwin Core term name: infraspecificEpithet.
        /// The name of the lowest or terminal infraspecific epithet
        /// of the scientificName, excluding any rank designation.
        /// This property is currently not used.
        /// </summary>
        public string InfraspecificEpithet { get; set; }

        /// <summary>
        /// Darwin Core term name: kingdom.
        /// The full scientific name of the kingdom in which the
        /// taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Kingdom { get; set; }

        /// <summary>
        /// Darwin Core term name: nameAccordingTo.
        /// The reference to the source in which the specific
        /// taxon concept circumscription is defined or implied -
        /// traditionally signified by the Latin "sensu" or "sec."
        /// (from secundum, meaning "according to").
        /// For taxa that result from identifications, a reference
        /// to the keys, monographs, experts and other sources should
        /// be given.
        /// This property is currently not used.
        /// </summary>
        public string NameAccordingTo { get; set; }

        /// <summary>
        /// Darwin Core term name: nameAccordingToID.
        /// An identifier for the source in which the specific
        /// taxon concept circumscription is defined or implied.
        /// See nameAccordingTo.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string NameAccordingToID { get; set; }

        /// <summary>
        /// Darwin Core term name: namePublishedIn.
        /// A reference for the publication in which the
        /// scientificName was originally established under the rules
        /// of the associated nomenclaturalCode.
        /// This property is currently not used.
        /// </summary>
        public string NamePublishedIn { get; set; }

        /// <summary>
        /// Darwin Core term name: namePublishedInID.
        /// An identifier for the publication in which the
        /// scientificName was originally established under the
        /// rules of the associated nomenclaturalCode.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string NamePublishedInId { get; set; }

        /// <summary>
        /// Darwin Core term name: namePublishedInYear.
        /// The four-digit year in which the scientificName
        /// was published.
        /// This property is currently not used.
        /// </summary>
        public string NamePublishedInYear { get; set; }

        /// <summary>
        /// part of Habitats directive article 2
        /// </summary>
        public bool? Natura2000HabitatsDirectiveArticle2 { get; set; }

        /// <summary>
        /// part of Habitats directive article 2
        /// </summary>
        public bool? Natura2000HabitatsDirectiveArticle4 { get; set; }

        /// <summary>
        /// part of Habitats directive article 2
        /// </summary>
        public bool? Natura2000HabitatsDirectiveArticle5 { get; set; }

        /// <summary>
        /// Darwin Core term name: nomenclaturalCode.
        /// The nomenclatural code (or codes in the case of an
        /// ambiregnal name) under which the scientificName is
        /// constructed.
        /// Recommended best practice is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string NomenclaturalCode { get; set; }

        /// <summary>
        /// Darwin Core term name: nomenclaturalStatus.
        /// The status related to the original publication of the
        /// name and its conformance to the relevant rules of
        /// nomenclature. It is based essentially on an algorithm
        /// according to the business rules of the code.
        /// It requires no taxonomic opinion.
        /// This property is currently not used.
        /// </summary>
        public string NomenclaturalStatus { get; set; }

        /// <summary>
        /// Darwin Core term name: order.
        /// The full scientific name of the order in which
        /// the taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// Organism group
        /// </summary>
        public string OrganismGroup { get; set; }

        /// <summary>
        /// Darwin Core term name: originalNameUsage.
        /// The taxon name, with authorship and date information
        /// if known, as it originally appeared when first established
        /// under the rules of the associated nomenclaturalCode.
        /// The basionym (botany) or basonym (bacteriology) of the
        /// scientificName or the senior/earlier homonym for replaced
        /// names.
        /// This property is currently not used.
        /// </summary>
        public string OriginalNameUsage { get; set; }

        /// <summary>
        /// Darwin Core term name: originalNameUsageID.
        /// An identifier for the name usage (documented meaning of
        /// the name according to a source) in which the terminal
        /// element of the scientificName was originally established
        /// under the rules of the associated nomenclaturalCode.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string OriginalNameUsageId { get; set; }

        /// <summary>
        /// Darwin Core term name: parentNameUsage.
        /// The full name, with authorship and date information
        /// if known, of the direct, most proximate higher-rank
        /// parent taxon (in a classification) of the most specific
        /// element of the scientificName.
        /// This property is currently not used.
        /// </summary>
        public string ParentNameUsage { get; set; }

        /// <summary>
        /// Darwin Core term name: parentNameUsageID.
        /// An identifier for the name usage (documented meaning
        /// of the name according to a source) of the direct,
        /// most proximate higher-rank parent taxon
        /// (in a classification) of the most specific
        /// element of the scientificName.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string ParentNameUsageId { get; set; }

        /// <summary>
        /// Darwin Core term name: phylum.
        /// The full scientific name of the phylum or division
        /// in which the taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Phylum { get; set; }

        /// <summary>
        /// True if taxon is protected by law
        /// </summary>
        public bool? ProtectedByLaw { get; set; }

        /// <summary>
        /// True if taxon is protected by law
        /// </summary>
        public string ProtectionLevel { get; set; }

        /// <summary>
        /// Redlist category
        /// </summary>
        public string RedlistCategory { get; set; }

        /// <summary>
        /// Darwin Core term name: scientificName.
        /// The full scientific name, with authorship and date
        /// information if known. When forming part of an
        /// Identification, this should be the name in lowest level
        /// taxonomic rank that can be determined.
        /// This term should not contain identification qualifications,
        /// which should instead be supplied in the
        /// IdentificationQualifier term.
        /// Currently scientific name without author is provided.
        /// </summary>
        public string ScientificName { get; set; }

        /// <summary>
        /// Darwin Core term name: scientificNameAuthorship.
        /// The authorship information for the scientificName
        /// formatted according to the conventions of the applicable
        /// nomenclaturalCode.
        /// This property is currently not used.
        /// </summary>
        public string ScientificNameAuthorship { get; set; }

        /// <summary>
        /// Darwin Core term name: scientificNameID.
        /// An identifier for the nomenclatural (not taxonomic)
        /// details of a scientific name.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string ScientificNameId { get; set; }

        /// <summary>
        /// Darwin Core term name: specificEpithet.
        /// The name of the first or species epithet of
        /// the scientificName.
        /// This property is currently not used.
        /// </summary>
        public string SpecificEpithet { get; set; }

        /// <summary>
        /// Darwin Core term name: subgenus.
        /// The full scientific name of the subgenus in which
        /// the taxon is classified. Values should include the
        /// genus to avoid homonym confusion.
        /// This property is currently not used.
        /// </summary>
        public string Subgenus { get; set; }

        /// <summary>
        /// Do taxon occur in sweden 
        /// </summary>
        public string SwedishOccurrence { get; set; }

        /// <summary>
        /// Do taxon occur in sweden 
        /// </summary>
        public string SwedishHistory { get; set; }

        /// <summary>
        /// Darwin Core term name: taxonConceptID.
        /// An identifier for the taxonomic concept to which the record
        /// refers - not for the nomenclatural details of a taxon.
        /// In SwedishSpeciesObservationSOAPService this property
        /// has the same value as property TaxonID. 
        /// GUID in Dyntaxa is used as value for this property.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string TaxonConceptId { get; set; }

        /// <summary>
        /// An identifier for the set of taxon information
        /// (data associated with the Taxon class). May be a global
        /// unique identifier or an identifier specific to the data set.
        /// In SwedishSpeciesObservationSOAPService this property
        /// has the same value as property TaxonConceptID. 
        /// GUID in Dyntaxa is used as value for this property.
        /// </summary>
        public string TaxonId { get; set; }

        /// <summary>
        /// Darwin Core term name: taxonomicStatus.
        /// The status of the use of the scientificName as a label
        /// for a taxon. Requires taxonomic opinion to define the
        /// scope of a taxon. Rules of priority then are used to
        /// define the taxonomic status of the nomenclature contained
        /// in that scope, combined with the experts opinion.
        /// It must be linked to a specific taxonomic reference that
        /// defines the concept.
        /// Recommended best practice is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string TaxonomicStatus { get; set; }

        /// <summary>
        /// Darwin Core term name: taxonRank.
        /// The taxonomic rank of the most specific name in the
        /// scientificName. Recommended best practice is to use
        /// a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string TaxonRank { get; set; }

        /// <summary>
        /// Darwin Core term name: taxonRemarks.
        /// Comments or notes about the taxon or name.
        /// This property is currently not used.
        /// </summary>
        public string TaxonRemarks { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimTaxonRank.
        /// The taxonomic rank of the most specific name in the
        /// scientificName as it appears in the original record.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimTaxonRank { get; set; }

        /// <summary>
        /// Darwin Core term name: vernacularName.
        /// A common or vernacular name.
        /// </summary>
        public string VernacularName { get; set; }
    }
}
