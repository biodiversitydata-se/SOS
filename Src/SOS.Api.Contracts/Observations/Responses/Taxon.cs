namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// This class contains taxon information about a species observation.
/// </summary>
public class Taxon
{

    /// <summary>
    /// The full name, with authorship and date information
    /// <br/>if known, of the currently valid (zoological) or
    /// <br/>accepted (botanical) taxon.
    /// </summary>
    public string? AcceptedNameUsage { get; set; }

    /// <summary>
    /// An identifier for the name usage (documented meaning of
    /// <br/>the name according to a source) of the currently valid
    /// <br/>(zoological) or accepted (botanical) taxon.
    /// </summary>
    public string? AcceptedNameUsageId { get; set; }

    public TaxonAttributes? Attributes { get; set; }

    /// <summary>
    /// Part of bird directive?
    /// </summary>
    public bool BirdDirective { get; set; }

    /// <summary>
    /// Part of bird directive annex 1?
    /// </summary>
    public bool BirdDirectiveAnnex1 { get; set; }

    /// <summary>
    /// The full scientific name of the class in which
    /// <br/>the taxon is classified.
    /// </summary>
    public string? Class { get; set; }

    /// <summary>
    /// Name used for display
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// The full scientific name of the family in which
    /// <br/>the taxon is classified.
    /// </summary>
    public string? Family { get; set; }

    /// <summary>
    /// The full scientific name of the genus in which
    /// <br/>the taxon is classified.
    /// </summary>
    public string? Genus { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of taxa names
    /// <br/>terminating at the rank immediately superior to the
    /// <br/>taxon referenced in the taxon record.
    /// <br/>Recommended best practice is to order the list
    /// <br/>starting with the highest rank and separating the names
    /// <br/>for each rank with a semi-colon (";").
    /// </summary>
    public string? HigherClassification { get; set; }

    /// <summary>
    /// Object id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the lowest or terminal infraspecific epithet
    /// <br/>of the scientificName, excluding any rank designation.
    /// </summary>
    public string? InfraspecificEpithet { get; set; }

    /// <summary>
    /// The full scientific name of the kingdom in which the
    /// <br/>taxon is classified.
    /// </summary>
    public string? Kingdom { get; set; }

    /// <summary>
    /// The reference to the source in which the specific
    /// <br/>taxon concept circumscription is defined or implied -
    /// <br/>traditionally signified by the Latin "sensu" or "sec."
    /// <br/>(from secundum, meaning "according to").
    /// <br/>For taxa that result from identifications, a reference
    /// <br/>to the keys, monographs, experts and other sources should
    /// <br/>be given.
    /// </summary>
    public string? NameAccordingTo { get; set; }

    /// <summary>
    /// An identifier for the source in which the specific
    /// <br/>taxon concept circumscription is defined or implied.
    /// <br/>See nameAccordingTo.
    /// </summary>
    public string? NameAccordingToId { get; set; }

    /// <summary>
    /// A reference for the publication in which the
    /// <br/>scientificName was originally established under the rules
    /// <br/>of the associated nomenclaturalCode.
    /// </summary>
    public string? NamePublishedIn { get; set; }

    /// <summary>
    /// An identifier for the publication in which the
    /// <br/>scientificName was originally established under the
    /// <br/>rules of the associated nomenclaturalCode.
    /// </summary>
    public string? NamePublishedInId { get; set; }

    /// <summary>
    /// The four-digit year in which the scientificName
    /// <br/>was published.
    /// </summary>
    public string? NamePublishedInYear { get; set; }

    /// <summary>
    /// The nomenclatural code (or codes in the case of an
    /// <br/>ambiregnal name) under which the scientificName is
    /// <br/>constructed.
    /// <br/>Recommended best practice is to use a controlled vocabulary.
    /// </summary>
    public string? NomenclaturalCode { get; set; }

    /// <summary>
    /// The status related to the original publication of the
    /// <br/>name and its conformance to the relevant rules of
    /// <br/>nomenclature. It is based essentially on an algorithm
    /// <br/>according to the business rules of the code.
    /// <br/>It requires no taxonomic opinion.
    /// </summary>
    public string? NomenclaturalStatus { get; set; }

    /// <summary>
    /// The full scientific name of the order in which
    /// <br/>the taxon is classified.
    /// </summary>
    public string? Order { get; set; }

    /// <summary>
    /// The taxon name, with authorship and date information
    /// <br/>if known, as it originally appeared when first established
    /// <br/>under the rules of the associated nomenclaturalCode.
    /// <br/>The basionym (botany) or basonym (bacteriology) of the
    /// <br/>scientificName or the senior/earlier homonym for replaced
    /// <br/>names.
    /// </summary>
    public string? OriginalNameUsage { get; set; }

    /// <summary>
    /// An identifier for the name usage (documented meaning of
    /// <br/>the name according to a source) in which the terminal
    /// <br/>element of the scientificName was originally established
    /// <br/>under the rules of the associated nomenclaturalCode.
    /// </summary>
    public string? OriginalNameUsageId { get; set; }

    /// <summary>
    /// The full name, with authorship and date information
    /// <br/>if known, of the direct, most proximate higher-rank
    /// <br/>parent taxon (in a classification) of the most specific
    /// <br/>element of the scientificName.
    /// </summary>
    public string? ParentNameUsage { get; set; }

    /// <summary>
    /// An identifier for the name usage (documented meaning
    /// <br/>of the name according to a source) of the direct,
    /// <br/>most proximate higher-rank parent taxon
    /// <br/>(in a classification) of the most specific
    /// <br/>element of the scientificName.
    /// </summary>
    public string? ParentNameUsageId { get; set; }

    /// <summary>
    /// The full scientific name of the phylum or division
    /// <br/>in which the taxon is classified.
    /// </summary>
    public string? Phylum { get; set; }

    /// <summary>
    /// The full scientific name, with authorship and date
    /// <br/>information if known. When forming part of an
    /// <br/>Identification, this should be the name in lowest level
    /// <br/>taxonomic rank that can be determined.
    /// <br/>This term should not contain identification qualifications,
    /// <br/>which should instead be supplied in the
    /// <br/>IdentificationQualifier term.
    /// <br/>Currently scientific name without author is provided.
    /// </summary>
    public string? ScientificName { get; set; }

    /// <summary>
    /// The authorship information for the scientificName
    /// <br/>formatted according to the conventions of the applicable
    /// <br/>nomenclaturalCode.
    /// </summary>
    public string? ScientificNameAuthorship { get; set; }

    /// <summary>
    /// An identifier for the nomenclatural (not taxonomic)
    /// <br/>details of a scientific name.
    /// </summary>
    public string? ScientificNameId { get; set; }

    /// <summary>
    /// Secondary parents dyntaxa taxon ids.
    /// </summary>
    public ICollection<int>? SecondaryParentDyntaxaTaxonIds { get; set; }

    /// <summary>
    /// The name of the first or species epithet of
    /// <br/>the scientificName.
    /// </summary>
    public string? SpecificEpithet { get; set; }

    /// <summary>
    /// The full scientific name of the subgenus in which
    /// <br/>the taxon is classified. Values should include the
    /// <br/>genus to avoid homonym confusion.
    /// </summary>
    public string? Subgenus { get; set; }

    /// <summary>
    /// An identifier for the taxonomic concept to which the record
    /// <br/>refers - not for the nomenclatural details of a taxon.
    /// <br/>In SwedishSpeciesObservationSOAPService this property
    /// <br/>has the same value as property TaxonID.
    /// <br/>GUID in Dyntaxa is used as value for this property.
    /// </summary>
    public string? TaxonConceptId { get; set; }

    /// <summary>
    /// An identifier for the set of taxon information
    /// <br/>(data associated with the Taxon class). May be a global
    /// <br/>unique identifier or an identifier specific to the data set.
    /// <br/>In SwedishSpeciesObservationSOAPService this property
    /// <br/>has the same value as property TaxonConceptID.
    /// <br/>GUID in Dyntaxa is used as value for this property.
    /// </summary>
    public string? TaxonId { get; set; }

    /// <summary>
    /// The status of the use of the scientificName as a label
    /// <br/>for a taxon. Requires taxonomic opinion to define the
    /// <br/>scope of a taxon. Rules of priority then are used to
    /// <br/>define the taxonomic status of the nomenclature contained
    /// <br/>in that scope, combined with the experts opinion.
    /// <br/>It must be linked to a specific taxonomic reference that
    /// <br/>defines the concept.
    /// <br/>Recommended best practice is to use a controlled vocabulary.
    /// </summary>
    public string? TaxonomicStatus { get; set; }

    /// <summary>
    /// The taxonomic rank of the most specific name in the
    /// <br/>scientificName. Recommended best practice is to use
    /// <br/>a controlled vocabulary.
    /// </summary>
    public string? TaxonRank { get; set; }

    /// <summary>
    /// Comments or notes about the taxon or name.
    /// </summary>
    public string? TaxonRemarks { get; set; }

    /// <summary>
    /// The taxonomic rank of the most specific name in the
    /// <br/>scientificName as it appears in the original record.
    /// </summary>
    public string? VerbatimTaxonRank { get; set; }

    /// <summary>
    /// A common or vernacular name.
    /// </summary>
    public string? VernacularName { get; set; }

    /// <summary>
    /// Verbatim id
    /// </summary>
    public string? VerbatimId { get; set; }

    /// <summary>
    /// Verbatim name
    /// </summary>
    public string? VerbatimName { get; set; }

}


