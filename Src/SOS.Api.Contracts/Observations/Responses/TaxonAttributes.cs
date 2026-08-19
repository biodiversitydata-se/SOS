using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Taxon attributes.
/// </summary>
public class TaxonAttributes
{

    /// <summary>
    /// Indicates whether the species is the subject
    /// <br/>of an action plan ('åtgärdsprogram' in swedish).
    /// </summary>
    public string? ActionPlan { get; set; }

    /// <summary>
    /// Disturbance radius.
    /// </summary>
    public int? DisturbanceRadius { get; set; }

    /// <summary>
    /// Taxon id value in Dyntaxa.
    /// </summary>
    public int DyntaxaTaxonId { get; set; }

    /// <summary>
    /// Id for taxon in GBIF
    /// </summary>
    public int? GbifTaxonId { get; set; }

    /// <summary>
    /// First organism label supplied by the source system.
    /// </summary>
    public string? OrganismLabel1 { get; set; }

    /// <summary>
    /// Second organism label supplied by the source system.
    /// </summary>
    public string? OrganismLabel2 { get; set; }

    /// <summary>
    /// True if invasive in sweden according to EU Regulation 1143/2014.
    /// </summary>
    public bool IsInvasiveAccordingToEuRegulation { get; set; }

    /// <summary>
    /// True if invasive in sweden.
    /// </summary>
    public bool IsInvasiveInSweden { get; set; }

    /// <summary>
    /// Invasive risk assessment category.
    /// </summary>
    public string? InvasiveRiskAssessmentCategory { get; set; }

    /// <summary>
    /// True if derived redlist category is one of CR, EN, VU, NT.
    /// </summary>
    public bool IsRedlisted { get; set; }

    /// <summary>
    /// Natura 2000, Habitats directive article 2.
    /// </summary>
    public bool Natura2000HabitatsDirectiveArticle2 { get; set; }

    /// <summary>
    /// Natura 2000, Habitats directive article 4.
    /// </summary>
    public bool Natura2000HabitatsDirectiveArticle4 { get; set; }

    /// <summary>
    /// Natura 2000, Habitats directive article 5.
    /// </summary>
    public bool Natura2000HabitatsDirectiveArticle5 { get; set; }

    /// <summary>
    /// Common name of the organism group that observed species
    /// <br/>belongs to. Classification of species groups is the same as
    /// <br/>used in latest 'Red List of Swedish Species'.
    /// </summary>
    public string? OrganismGroup { get; set; }

    /// <summary>
    /// Parent Dyntaxa TaxonId.
    /// </summary>
    public int? ParentDyntaxaTaxonId { get; set; }

    /// <summary>
    /// Indicates whether the species 
    /// <br/>is protected by the law in Sweden.
    /// </summary>
    public bool ProtectedByLaw { get; set; }
    

    /// <summary>
    /// Redlist category for redlisted species. Possible redlist values
    /// <br/>are DD (Data Deficient), EX (Extinct),
    /// <br/>RE (Regionally Extinct), CR (Critically Endangered),
    /// <br/>EN (Endangered), VU (Vulnerable), NT (Near Threatened).
    /// <br/>Not redlisted species has no value in this property.
    /// </summary>
    public string? RedlistCategory { get; set; }

    public IEnumerable<TaxonScientificName>? ScientificNames { get; set; }

    /// <summary>
    /// Derivied red list category from parent taxa
    /// </summary>
    public string? RedlistCategoryDerived { get; set; }

    public VocabularyValue? SensitivityCategory { get; set; }

    /// <summary>
    /// Systematic sort order.
    /// </summary>
    public int SortOrder { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SpeciesGroup SpeciesGroup { get; set; }

    /// <summary>
    /// Information about the species occurrence in Sweden.
    /// <br/>For example information about if the species reproduce
    /// <br/>in sweden.
    /// </summary>
    public string? SwedishOccurrence { get; set; }

    /// <summary>
    /// This property contains information about the species
    /// <br/>immigration history.
    /// </summary>
    public string? SwedishHistory { get; set; }

    /// <summary>
    /// Scientific synonym names.
    /// </summary>
    public ICollection<TaxonSynonymName>? Synonyms { get; set; }

    public VocabularyValue? TaxonCategory { get; set; }

    /// <summary>
    /// Vernacular names.
    /// </summary>
    public ICollection<TaxonVernacularName>? VernacularNames { get; set; }

}


