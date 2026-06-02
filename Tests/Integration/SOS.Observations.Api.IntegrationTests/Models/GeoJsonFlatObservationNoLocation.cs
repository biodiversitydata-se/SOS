namespace SOS.Observations.Api.IntegrationTests.Models;

public class GeoJsonFlatObservationNoLocation
{
    /// <summary>
    /// Occurrence ID (Observation GUID)
    /// </summary>
    public string? OccurrenceId { get; set; }

    /// <summary>
    /// Basis of record (Dataunderlag)
    /// </summary>
    public string? BasisOfRecord { get; set; }

    /// <summary>
    /// Basis of record ID (Dataunderlag id)
    /// </summary>
    public int? BasisOfRecordId { get; set; }

    /// <summary>
    /// Accepted name usage ID (Godkänd namnanvändande id)
    /// </summary>
    public string? AcceptedNameUsageId { get; set; }

    /// <summary>
    /// Taxon Id (Taxon ID)
    /// </summary>
    public int? DyntaxaTaxonId { get; set; }

    /// <summary>
    /// Scientific name (Vetenskapligt namn)
    /// </summary>
    public string? ScientificName { get; set; }

    /// <summary>
    /// Scientific name authorship (Auktor)
    /// </summary>
    public string? ScientificNameAuthorship { get; set; }

    /// <summary>
    /// Common name (Svenskt namn)
    /// </summary>
    public string? VernacularName { get; set; }

    /// <summary>
    /// Higher classification (Taxonklassificering)
    /// </summary>
    public string? HigherClassification { get; set; }

    /// <summary>
    /// Taxon rank (Taxonkategori)
    /// </summary>
    public string? TaxonRank { get; set; }

    /// <summary>
    /// Kingdom (Rike)
    /// </summary>
    public string? Kingdom { get; set; }

    /// <summary>
    /// Phylum (Fylum)
    /// </summary>
    public string? Phylum { get; set; }

    /// <summary>
    /// Class (Klass)
    /// </summary>
    public string? Class { get; set; }

    /// <summary>
    /// Order (Ordning)
    /// </summary>
    public string? Order { get; set; }

    /// <summary>
    /// Family (Familj)
    /// </summary>
    public string? Family { get; set; }

    /// <summary>
    /// Genus (Släkte)
    /// </summary>
    public string? Genus { get; set; }

    /// <summary>
    /// Nomenclatural status (Nomenklaturisk status)
    /// </summary>
    public string? NomenclaturalStatus { get; set; }

    /// <summary>
    /// Original name usage ID (ursprungligt namnanvändande id)
    /// </summary>
    public string? OriginalNameUsageId { get; set; }

    /// <summary>
    /// Parent name usage ID (Överliggande namnanvändande id)
    /// </summary>
    public string? ParentNameUsageId { get; set; }

    /// <summary>
    /// Secondary Parent taxon ID´s (Sekundära överliggande taxon id:n)
    /// </summary>
    public string? SecondaryParentDyntaxaTaxonIds { get; set; }

    /// <summary>
    /// Taxon GUID (Taxon GUID)
    /// </summary>
    public string? TaxonId { get; set; }

    /// <summary>
    /// Taxonomic status (Taxonomisk status)
    /// </summary>
    public string? TaxonomicStatus { get; set; }

    /// <summary>
    /// Taxon category (Taxonkategori)
    /// </summary>
    public string? TaxonCategory { get; set; }

    /// <summary>
    /// Taxon category ID (Taxonkategori id)
    /// </summary>
    public int? TaxonCategoryId { get; set; }

    /// <summary>
    /// Organism group (Organismgrupp)
    /// </summary>
    public string? OrganismGroup { get; set; }

    /// <summary>
    /// Is redlisted (Rödlistad)
    /// </summary>
    public bool? TaxonIsRedlisted { get; set; }

    /// <summary>
    /// Redlist category (Rödlistekategori)
    /// </summary>
    public string? RedlistCategory { get; set; }

    /// <summary>
    /// Redlist category (derived) (Rödlistekategori (ärvd))
    /// </summary>
    public string? RedlistCategoryDerived { get; set; }

    /// <summary>
    /// Taxon remarks (Taxonkommentar (taxonkonceptetets definition))
    /// </summary>
    public string? TaxonRemarks { get; set; }

    /// <summary>
    /// Taxon sort order (Taxon sorteringsordning)
    /// </summary>
    public int? SortOrder { get; set; }

    /// <summary>
    /// Dyntaxa taxon ID (Dyntaxa taxon id)
    /// </summary>
    public int? DyntaxaTaxonIdAttributes { get; set; }

    /// <summary>
    /// Parent Dyntaxa taxon ID (Överliggande Dyntaxa taxon id)
    /// </summary>
    public int? ParentDyntaxaTaxonId { get; set; }

    /// <summary>
    /// Protected by law (Fridlyst)
    /// </summary>
    public bool? ProtectedByLaw { get; set; }

    /// <summary>
    /// Species sensitivity category (Artens skyddsklass)
    /// </summary>
    public string? TaxonSensitivityCategory { get; set; }

    /// <summary>
    /// Species sensitivity category ID (Artens skyddsklass id)
    /// </summary>
    public int? TaxonSensitivityCategoryId { get; set; }

    /// <summary>
    /// Species disturbance radius (Artens störningsradie)
    /// </summary>
    public int? DisturbanceRadius { get; set; }

    /// <summary>
    /// Species action plan (Åtgärdsprogram)
    /// </summary>
    public string? ActionPlan { get; set; }

    /// <summary>
    /// Birds Directive (Fågeldirektivet)
    /// </summary>
    public bool? BirdDirective { get; set; }

    /// <summary>
    /// Birds Directive Annex 1 (Fågeldirektivet bilaga 1)
    /// </summary>
    public bool? BirdDirectiveAnnex1 { get; set; }

    /// <summary>
    /// Habitats Directive Annex 2 (Habitatdirektivet bilaga 2)
    /// </summary>
    public bool? Natura2000HabitatsDirectiveArticle2 { get; set; }

    /// <summary>
    /// Habitats Directive Annex 4 (Habitatdirektivet bilaga 4)
    /// </summary>
    public bool? Natura2000HabitatsDirectiveArticle4 { get; set; }

    /// <summary>
    /// Habitats Directive Annex 5 (Habitatdirektivet bilaga 5)
    /// </summary>
    public bool? Natura2000HabitatsDirectiveArticle5 { get; set; }

    /// <summary>
    /// Invasive (EU regulation) (Invasiv (EU-förordning))
    /// </summary>
    public bool? TaxonIsInvasiveEuRegulation { get; set; }

    /// <summary>
    /// Invasive (Invasiv)
    /// </summary>
    public bool? TaxonIsInvasiveInSweden { get; set; }

    /// <summary>
    /// Invasive risk category (Sweden) (Invasiv riskkategori (Sverige))
    /// </summary>
    public string? TaxonInvasiveRiskCategory { get; set; }

    /// <summary>
    /// Swedish immigration history (Svensk invandringshistoria)
    /// </summary>
    public string? SwedishHistory { get; set; }

    /// <summary>
    /// Swedish occurence (Svensk förekomst)
    /// </summary>
    public string? SwedishOccurrence { get; set; }

    /// <summary>
    /// Synonyms (Synonymer)
    /// </summary>
    public string? TaxonSynonyms { get; set; }

    /// <summary>
    /// Vernacular names (Trivialnamn)
    /// </summary>
    public string? VernacularNames { get; set; }

    /// <summary>
    /// Event ID (Fältbesök id)
    /// </summary>
    public string? EventId { get; set; }

    /// <summary>
    /// Start date [date time] (Startdatum [datum tid])
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date [date time] (Slutdatum [datum tid])
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Start date (Startdatum)
    /// </summary>
    public string? PlainStartDate { get; set; }

    /// <summary>
    /// End date (Slutdatum)
    /// </summary>
    public string? PlainEndDate { get; set; }

    /// <summary>
    /// Start time (Starttid)
    /// </summary>
    public string? PlainStartTime { get; set; }

    /// <summary>
    /// End time (Sluttid)
    /// </summary>
    public string? PlainEndTime { get; set; }

    /// <summary>
    /// Event remarks (Fältbesökskommentar)
    /// </summary>
    public string? EventRemarks { get; set; }

    /// <summary>
    /// Discovery method (Metod (Artportalen))
    /// </summary>
    public string? DiscoveryMethod { get; set; }

    /// <summary>
    /// Discovery method ID (Metod id (Artportalen))
    /// </summary>
    public int? DiscoveryMethodId { get; set; }

    /// <summary>
    /// Sampling effort (Insamlingsinsats)
    /// </summary>
    public string? SamplingEffort { get; set; }

    /// <summary>
    /// Sampling protocol (Insamlingsmetod)
    /// </summary>
    public string? SamplingProtocol { get; set; }

    /// <summary>
    /// Sample size unit (Stickprovsstorlek enhet)
    /// </summary>
    public string? SampleSizeUnit { get; set; }

    /// <summary>
    /// Sample size value (Stickprovsstorlek värde)
    /// </summary>
    public string? SampleSizeValue { get; set; }

    /// <summary>
    /// Habitat (Habitat)
    /// </summary>
    public string? Habitat { get; set; }

    /// <summary>
    /// Event field notes (Fältbesök fältanteckningar)
    /// </summary>
    public string? FieldNotes { get; set; }

    /// <summary>
    /// Verbatim event date (Ursprunglig tidsangivelse för fältbesöket)
    /// </summary>
    public string? VerbatimEventDate { get; set; }

    /// <summary>
    /// Occurrence status (Observation status)
    /// </summary>
    public string? OccurrenceStatus { get; set; }

    /// <summary>
    /// Occurrence status ID (Observation status id)
    /// </summary>
    public int? OccurrenceStatusId { get; set; }

    /// <summary>
    /// Is positive observation (Påträffad)
    /// </summary>
    public bool? IsPositiveObservation { get; set; }

    /// <summary>
    /// Is never found observation (Aldrig funnen)
    /// </summary>
    public bool? IsNeverFoundObservation { get; set; }

    /// <summary>
    /// Is not rediscovered observation (Ej återfunnen)
    /// </summary>
    public bool? IsNotRediscoveredObservation { get; set; }

    /// <summary>
    /// Individual count (Antal individer)
    /// </summary>
    public string? IndividualCount { get; set; }

    /// <summary>
    /// Organism quantity (Organismkvantitet)
    /// </summary>
    public string? OrganismQuantity { get; set; }

    /// <summary>
    /// Organism quantity unit (Organismkvantitetsenhet)
    /// </summary>
    public string? OrganismQuantityUnit { get; set; }

    /// <summary>
    /// Organism quantity (integer) (Organismkvantitet (heltal))
    /// </summary>
    public int? OrganismQuantityInt { get; set; }

    /// <summary>
    /// Organism quantity unit ID (Organismkvantitetsenhet id)
    /// </summary>
    public int? OrganismQuantityUnitId { get; set; }

    /// <summary>
    /// Life stage/Age class (Stadium/ålder)
    /// </summary>
    public string? LifeStage { get; set; }

    /// <summary>
    /// Life stage/Age class ID (Stadium/ålder id)
    /// </summary>
    public int? LifeStageId { get; set; }

    /// <summary>
    /// Sex (Kön)
    /// </summary>
    public string? Sex { get; set; }

    /// <summary>
    /// Sex ID (Kön id)
    /// </summary>
    public int? SexId { get; set; }

    /// <summary>
    /// Activity (Aktivitet (Artportalen))
    /// </summary>
    public string? Activity { get; set; }

    /// <summary>
    /// Activity ID (Aktivitet (Artportalen) id)
    /// </summary>
    public int? ActivityId { get; set; }

    /// <summary>
    /// Bird breeding criterion ID (Häckningskriterie id)
    /// </summary>
    public int? BirdNestActivityId { get; set; }

    /// <summary>
    /// Behaviour (Beteende)
    /// </summary>
    public string? Behavior { get; set; }

    /// <summary>
    /// Behaviour Id (Beteende id)
    /// </summary>
    public int? BehaviorId { get; set; }

    /// <summary>
    /// Reproductive condition (Reproduktionsstatus)
    /// </summary>
    public string? ReproductiveCondition { get; set; }

    /// <summary>
    /// Reproductive condition Id (Reproduktionsstatus Id)
    /// </summary>
    public int? ReproductiveConditionId { get; set; }

    /// <summary>
    /// Length (mm) (Längd (mm))
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// Weight (g) (Vikt (g))
    /// </summary>
    public int? Weight { get; set; }

    /// <summary>
    /// Occurrence remarks (Kommentarer)
    /// </summary>
    public string? OccurrenceRemarks { get; set; }

    /// <summary>
    /// Recorded by (Observatör)
    /// </summary>
    public string? RecordedBy { get; set; }

    /// <summary>
    /// Reported by (Rapportör)
    /// </summary>
    public string? ReportedBy { get; set; }

    /// <summary>
    /// Reported date (Rapporterad (datum))
    /// </summary>
    public DateTime? ReportedDate { get; set; }

    /// <summary>
    /// Biotope (Biotop)
    /// </summary>
    public string? Biotope { get; set; }

    /// <summary>
    /// Biotope Id (Biotop id)
    /// </summary>
    public int? BiotopeId { get; set; }

    /// <summary>
    /// Biotope description (Biotopbeskrivning)
    /// </summary>
    public string? BiotopeDescription { get; set; }

    /// <summary>
    /// Substrate ID (Substrat id)
    /// </summary>
    public int? SubstrateId { get; set; }

    /// <summary>
    /// Substrate name (Substratnamn)
    /// </summary>
    public string? SubstrateName { get; set; }

    /// <summary>
    /// Substrate name ID (Substratnamn id)
    /// </summary>
    public int? SubstrateNameId { get; set; }

    /// <summary>
    /// Substrate description (Substratbeskrivning)
    /// </summary>
    public string? SubstrateDescription { get; set; }

    /// <summary>
    /// Substrate quantity (Substrat kvantitet)
    /// </summary>
    public int? SubstrateQuantity { get; set; }

    /// <summary>
    /// Substrate species ID (Substratart id)
    /// </summary>
    public int? SubstrateSpeciesId { get; set; }

    /// <summary>
    /// Substrate species scientific name (Substratart vetenskapligt namn)
    /// </summary>
    public string? SubstrateSpeciesScientificName { get; set; }

    /// <summary>
    /// Substrate species vernacular name (Substratart svenskt namn)
    /// </summary>
    public string? SubstrateSpeciesVernacularName { get; set; }

    /// <summary>
    /// Substrate species description (Substratart beskrivning)
    /// </summary>
    public string? SubstrateSpeciesDescription { get; set; }

    /// <summary>
    /// Associated media (Associerad media)
    /// </summary>
    public string? AssociatedMedia { get; set; }

    /// <summary>
    /// Associated references (Associerade referenser)
    /// </summary>
    public string? AssociatedReferences { get; set; }

    /// <summary>
    /// Original data resource record ID (Ursprungkällans observations-id)
    /// </summary>
    public string? CatalogNumber { get; set; }

    /// <summary>
    /// Original data resource record ID (integer) (Ursprungkällans observations-id (heltal))
    /// </summary>
    public int? CatalogId { get; set; }

    /// <summary>
    /// Occurrence URL (Observation URL)
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Record number (Rapportörens observations-id)
    /// </summary>
    public string? RecordNumber { get; set; }

    /// <summary>
    /// Preparations (Preparations/Konserveringsmetoder)
    /// </summary>
    public string? Preparations { get; set; }

    /// <summary>
    /// Occurrence sensitivity category (Observationens skyddsklass)
    /// </summary>
    public int? SensitivityCategory { get; set; }

    /// <summary>
    /// Is natural occurrence (Naturlig förekomst)
    /// </summary>
    public bool? IsNaturalOccurrence { get; set; }

    /// <summary>
    /// Verified (Verifierad)
    /// </summary>
    public bool? Verified { get; set; }

    /// <summary>
    /// Verification status (Verifieringsstatus)
    /// </summary>
    public string? VerificationStatus { get; set; }

    /// <summary>
    /// Verification status ID (Verifieringsstatus id)
    /// </summary>
    public int? VerificationStatusId { get; set; }

    /// <summary>
    /// Uncertain identification (Osäker artbestämning)
    /// </summary>
    public bool? UncertainIdentification { get; set; }

    /// <summary>
    /// Confirmed by (Konfirmatör)
    /// </summary>
    public string? ConfirmedBy { get; set; }

    /// <summary>
    /// Confirmed date (Konfirmationsdatum)
    /// </summary>
    public string? ConfirmedDate { get; set; }

    /// <summary>
    /// Identified by (Determinatör)
    /// </summary>
    public string? IdentifiedBy { get; set; }

    /// <summary>
    /// Date identfied (Determinationsdatum)
    /// </summary>
    public string? DateIdentified { get; set; }

    /// <summary>
    /// Determination method (Bestämningsmetod)
    /// </summary>
    public string? DeterminationMethod { get; set; }

    /// <summary>
    /// Determination method ID (Bestämningsmetod id)
    /// </summary>
    public int? DeterminationMethodId { get; set; }

    /// <summary>
    /// Verified by (Verifierare)
    /// </summary>
    public string? VerifiedBy { get; set; }

    /// <summary>
    /// Identification type status (Typstatus)
    /// </summary>
    public string? TypeStatus { get; set; }

    /// <summary>
    /// Identification qualifier (Bestämningsstatus)
    /// </summary>
    public string? IdentificationQualifier { get; set; }

    /// <summary>
    /// Identification remarks (Artbestämningskommentar)
    /// </summary>
    public string? IdentificationRemarks { get; set; }

    /// <summary>
    /// Invasive species treatment (Invasiva arter åtgärd)
    /// </summary>
    public string? InvasiveSpeciesTreatment { get; set; }

    /// <summary>
    /// Invasive species treatment ID (Invasiva arter åtgärd id)
    /// </summary>
    public int? InvasiveSpeciesTreatmentId { get; set; }

    /// <summary>
    /// Projects (Projekt)
    /// </summary>
    public string? Projects { get; set; }

    /// <summary>
    /// Dynamic properties (Dynamiska egenskaper)
    /// </summary>
    public string? DynamicProperties { get; set; }

    /// <summary>
    /// Dataset name (Dataset namn)
    /// </summary>
    public string? DatasetName { get; set; }

    /// <summary>
    /// Dataset ID (Dataset (GUID))
    /// </summary>
    public string? DatasetId { get; set; }

    /// <summary>
    /// Collection code (Samlingskod)
    /// </summary>
    public string? CollectionCode { get; set; }

    /// <summary>
    /// Collection code ID (Samlings id)
    /// </summary>
    public string? CollectionId { get; set; }

    /// <summary>
    /// Private collection (Privat samling)
    /// </summary>
    public string? PrivateCollection { get; set; }

    /// <summary>
    /// Public collection (Publik samling)
    /// </summary>
    public string? PublicCollection { get; set; }

    /// <summary>
    /// Species collection label (Samlingsnummer)
    /// </summary>
    public string? SpeciesCollectionLabel { get; set; }

    /// <summary>
    /// Institution (Institution)
    /// </summary>
    public string? InstitutionCode { get; set; }

    /// <summary>
    /// Institution code ID (Institution kod id)
    /// </summary>
    public int? InstitutionCodeId { get; set; }

    /// <summary>
    /// Institution ID (Institution id)
    /// </summary>
    public string? InstitutionId { get; set; }

    /// <summary>
    /// Owner institution code (Dataägare institution)
    /// </summary>
    public string? OwnerInstitutionCode { get; set; }

    /// <summary>
    /// Dataset ID (integer) (Dataset id (heltal))
    /// </summary>
    public int? DataProviderId { get; set; }

    /// <summary>
    /// Modified date (Modifierad datum)
    /// </summary>
    public DateTime? Modified { get; set; }

    /// <summary>
    /// Sensitive (Skyddad)
    /// </summary>
    public bool? Sensitive { get; set; }

    /// <summary>
    /// Access rights (Åtkomsträttigheter)
    /// </summary>
    public string? AccessRights { get; set; }

    /// <summary>
    /// Access rights ID (Åtkomsträttigheter id)
    /// </summary>
    public int? AccessRightsId { get; set; }

    /// <summary>
    /// Rights holder (Rättighetsinnehavare)
    /// </summary>
    public string? RightsHolder { get; set; }

    /// <summary>
    /// Generalized (Diffuserad)
    /// </summary>
    public bool? IsGeneralized { get; set; }

    /// <summary>
    /// Data generalizations (Datageneraliseringar)
    /// </summary>
    public string? DataGeneralizations { get; set; }

    /// <summary>
    /// Language (Språk)
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// License (Licens)
    /// </summary>
    public string? License { get; set; }

    /// <summary>
    /// Resource type (Dataresurstyp)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Occurrence media (Observationsmedia)
    /// </summary>
    public string? Media { get; set; }

    /// <summary>
    /// Measurement or facts (Mått eller fakta)
    /// </summary>
    public string? MeasurementOrFacts { get; set; }

    /// <summary>
    /// Event media (Fältbesök media)
    /// </summary>
    public string? EventMedia { get; set; }

    /// <summary>
    /// Event Measurement or facts (Fältbesök Mått eller fakta)
    /// </summary>
    public string? EventMeasurementOrFacts { get; set; }

    /// <summary>
    /// Start date (year) (Startdatum (år))
    /// </summary>
    public int? StartYear { get; set; }

    /// <summary>
    /// Start date (month) (Startdatum (månad))
    /// </summary>
    public int? StartMonth { get; set; }

    /// <summary>
    /// Start date (day) (Startdatum (datum))
    /// </summary>
    public int? StartDay { get; set; }

    /// <summary>
    /// End date (year) (Slutdatum (år))
    /// </summary>
    public int? EndYear { get; set; }

    /// <summary>
    /// End date (month) (Slutdatum (månad))
    /// </summary>
    public int? EndMonth { get; set; }

    /// <summary>
    /// End date (day) (Slutdatum (dag))
    /// </summary>
    public int? EndDay { get; set; }

    /// <summary>
    /// Start date (histogram week) (Startdatum (histogramvecka))
    /// </summary>
    public int? StartHistogramWeek { get; set; }

    /// <summary>
    /// End date (histogram week) (Slutdatum (histogramvecka))
    /// </summary>
    public int? EndHistogramWeek { get; set; }

    /// <summary>
    /// GBIF TaxonID (GBIF TaxonID)
    /// </summary>
    public int? GbifTaxonId { get; set; }

    /// <summary>
    /// Project1 ID (Projekt1 ID)
    /// </summary>
    public int? Project1Id { get; set; }

    /// <summary>
    /// Project1 Name (Projekt1 Namn)
    /// </summary>
    public string? Project1Name { get; set; }

    /// <summary>
    /// Project1 Category (Projekt1 Kategori)
    /// </summary>
    public string? Project1Category { get; set; }

    /// <summary>
    /// Project1 URL (Projekt1 URL)
    /// </summary>
    public string? Project1Url { get; set; }

    /// <summary>
    /// Project1 Values (Projekt1 Värden)
    /// </summary>
    public string? Project1Values { get; set; }

    /// <summary>
    /// Project2 ID (Projekt2 ID)
    /// </summary>
    public int? Project2Id { get; set; }

    /// <summary>
    /// Project2 Name (Projekt2 Namn)
    /// </summary>
    public string? Project2Name { get; set; }

    /// <summary>
    /// Project2 Category (Projekt2 Kategori)
    /// </summary>
    public string? Project2Category { get; set; }

    /// <summary>
    /// Project2 URL (Projekt2 URL)
    /// </summary>
    public string? Project2Url { get; set; }

    /// <summary>
    /// Project2 Values (Projekt2 Värden)
    /// </summary>
    public string? Project2Values { get; set; }

    /// <summary>
    /// Dataset Identifier (Dataset Identifierare)
    /// </summary>
    public string? DatasetIdentifier { get; set; }

    /// <summary>
    /// Dataset title (Dataset titel)
    /// </summary>
    public string? DatasetTitle { get; set; }

    /// <summary>
    /// Material sample ID (Fysiskt prov id)
    /// </summary>
    public string? MaterialSampleId { get; set; }

    /// <summary>
    /// Organism ID (Organism id)
    /// </summary>
    public string? OrganismId { get; set; }

    /// <summary>
    /// Organism name (Organismnamn)
    /// </summary>
    public string? OrganismName { get; set; }

    /// <summary>
    /// Organism scope (Organism omfattning)
    /// </summary>
    public string? OrganismScope { get; set; }

    /// <summary>
    /// Associated organisms (Associerade organismer)
    /// </summary>
    public string? AssociatedOrganisms { get; set; }

    /// <summary>
    /// Previous identifications (Tidigare artbestämningar)
    /// </summary>
    public string? PreviousIdentifications { get; set; }

    /// <summary>
    /// Organism remarks (Organismkommentar)
    /// </summary>
    public string? OrganismRemarks { get; set; }    
}