using SOS.Api.Contracts.Observations.Responses;

namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>Identifiers of SOS controlled vocabularies.</summary>
public enum VocabularyIdDto
{

    /// <summary>Sex of the observed organism.</summary>
    Sex = 0,

    /// <summary>Activity of the observed organism.</summary>
    Activity = 1,

    /// <summary>Life stage of the observed organism.</summary>
    LifeStage = 6,

    /// <summary>Biotope in which the organism was observed.</summary>
    Biotope = 7,

    /// <summary>Substrate on which the organism was observed.</summary>
    Substrate = 8,

    /// <summary>Verification status of an observation.</summary>
    VerificationStatus = 9,

    /// <summary>Institution associated with an observation.</summary>
    Institution = 10,

    /// <summary>Unit used to report organism quantity.</summary>
    Unit = 11,

    /// <summary>The nature or source of an observation record.</summary>
    BasisOfRecord = 12,

    /// <summary>Continent in which an observation was made.</summary>
    Continent = 13,

    /// <summary>How the organism became established at the location.</summary>
    EstablishmentMeans = 14,

    /// <summary>Presence or absence of the organism.</summary>
    OccurrenceStatus = 15,

    /// <summary>Access rights associated with an observation.</summary>
    AccessRights = 16,

    /// <summary>Country in which an observation was made.</summary>
    Country = 17,

    /// <summary>The nature or genre of the resource.</summary>
    Type = 18,

    /// <summary>Type of geographic area.</summary>
    AreaType = 19,

    /// <summary>Method used to discover the organism.</summary>
    DiscoveryMethod = 20,

    /// <summary>Method used to identify the organism.</summary>
    DeterminationMethod = 21,

    /// <summary>Reproductive condition of the organism.</summary>
    ReproductiveCondition = 22,

    /// <summary>Behavior of the observed organism.</summary>
    Behavior = 23,

    /// <summary>Sensitivity category governing disclosure of an observation.</summary>
    SensitivityCategory = 24,

    /// <summary>Bird-nesting activity.</summary>
    BirdNestActivity = 25,

    /// <summary>Taxonomic category.</summary>
    TaxonCategory = 28,

    /// <summary>Treatment applied to an invasive species.</summary>
    InvasiveSpeciesTreatment = 29,

}


