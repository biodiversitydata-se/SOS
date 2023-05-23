using System;
using System.Linq;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// A flat representation of the Observation class.
    /// </summary>
    public class FlatObservation
    {
        private Observation _observation;
        private static TimeZoneInfo swedenTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        public FlatObservation(Observation observation)
        {
            _observation = observation;
        }

        public string EventStartTimeString => _observation?.Event?.PlainStartTime;
        public string EventEndTimeString => _observation?.Event?.PlainEndTime;
        public string OccurrenceId => _observation?.Occurrence?.OccurrenceId;
        public string EventDiscoveryMethod => _observation?.Event?.DiscoveryMethod?.ToString();
        public int? EventDiscoveryMethodId => _observation?.Event?.DiscoveryMethod?.Id;
        public string EventDiscoveryMethodValue => _observation?.Event?.DiscoveryMethod?.Value;
        public DateTime? EventStartDate
        {
            get
            {
                var date = _observation?.Event?.StartDate;
                if (date == null) return null;
                var swedenDate = TimeZoneInfo.ConvertTimeFromUtc(date.Value, swedenTimeZone);
                return swedenDate;
                //return _observation?.Event?.StartDate;
            }
        }
        public DateTime? EventEndDate
        {
            get
            {
                var date = _observation?.Event?.EndDate;
                if (date == null) return null;
                var swedenDate = TimeZoneInfo.ConvertTimeFromUtc(date.Value, swedenTimeZone);
                return swedenDate;
                //return _observation?.Event?.EndDate;
            }
        }

        public string EventPlainStartDate => _observation?.Event?.PlainStartDate;
        public string EventPlainEndDate => _observation?.Event?.PlainEndDate;
        public string EventEventId => _observation?.Event?.EventId;
        public string EventEventRemarks => _observation?.Event?.EventRemarks;
        public string EventFieldNotes => _observation?.Event?.FieldNotes;
        public string EventFieldNumber => _observation?.Event?.FieldNumber;
        public string EventHabitat => _observation?.Event?.Habitat;
        public string EventParentEventId => _observation?.Event?.ParentEventId;
        public string EventSamplingEffort => _observation?.Event?.SamplingEffort;
        public string EventSamplingProtocol => _observation?.Event?.SamplingProtocol;
        public string EventSampleSizeUnit => _observation?.Event?.SampleSizeUnit;
        public string EventSampleSizeValue => _observation?.Event?.SampleSizeValue;
        public string EventVerbatimEventDate => _observation?.Event?.VerbatimEventDate;
        public string EventMedia => _observation?.Event?.Media == null ? null : string.Join(", ", _observation?.Event?.Media.Select(m => m.ToString()));
        public string EventMeasurementOrFacts => _observation?.Event?.MeasurementOrFacts == null ? null : string.Join(", ", _observation?.Event?.MeasurementOrFacts.Select(m => m.ToString()));
        public string GeologicalContextBed => _observation?.GeologicalContext?.Bed;
        public string GeologicalContextEarliestAgeOrLowestStage => _observation?.GeologicalContext?.EarliestAgeOrLowestStage;
        public string GeologicalContextEarliestEonOrLowestEonothem => _observation?.GeologicalContext?.EarliestEonOrLowestEonothem;
        public string GeologicalContextEarliestEpochOrLowestSeries => _observation?.GeologicalContext?.EarliestEpochOrLowestSeries;
        public string GeologicalContextEarliestEraOrLowestErathem => _observation?.GeologicalContext?.EarliestEraOrLowestErathem;
        public string GeologicalContextEarliestGeochronologicalEra => _observation?.GeologicalContext?.EarliestGeochronologicalEra;
        public string GeologicalContextEarliestPeriodOrLowestSystem => _observation?.GeologicalContext?.EarliestPeriodOrLowestSystem;
        public string GeologicalContextFormation => _observation?.GeologicalContext?.Formation;
        public string GeologicalContextGeologicalContextId => _observation?.GeologicalContext?.GeologicalContextId;
        public string GeologicalContextGroup => _observation?.GeologicalContext?.Group;
        public string GeologicalContextHighestBiostratigraphicZone => _observation?.GeologicalContext?.HighestBiostratigraphicZone;
        public string GeologicalContextLatestAgeOrHighestStage => _observation?.GeologicalContext?.LatestAgeOrHighestStage;
        public string GeologicalContextLatestEonOrHighestEonothem => _observation?.GeologicalContext?.LatestEonOrHighestEonothem;
        public string GeologicalContextLatestEpochOrHighestSeries => _observation?.GeologicalContext?.LatestEpochOrHighestSeries;
        public string GeologicalContextLatestEraOrHighestErathem => _observation?.GeologicalContext?.LatestEraOrHighestErathem;
        public string GeologicalContextLatestGeochronologicalEra => _observation?.GeologicalContext?.LatestGeochronologicalEra;
        public string GeologicalContextLatestPeriodOrHighestSystem => _observation?.GeologicalContext?.LatestPeriodOrHighestSystem;
        public string GeologicalContextLithostratigraphicTerms => _observation?.GeologicalContext?.LithostratigraphicTerms;
        public string GeologicalContextLowestBiostratigraphicZone => _observation?.GeologicalContext?.LowestBiostratigraphicZone;
        public string GeologicalContextMember => _observation?.GeologicalContext?.Member;
        public string IdentificationConfirmedBy => _observation?.Identification?.ConfirmedBy;
        public string IdentificationConfirmedDate => _observation?.Identification?.ConfirmedDate;
        public string IdentificationDateIdentified => _observation?.Identification?.DateIdentified;
        public string IdentificationIdentificationId => _observation?.Identification?.IdentificationId;
        public string IdentificationIdentificationQualifier => _observation?.Identification?.IdentificationQualifier;
        public string IdentificationIdentificationReferences => _observation?.Identification?.IdentificationReferences;
        public string IdentificationIdentificationRemarks => _observation?.Identification?.IdentificationRemarks;
        public bool? IdentificationVerified => _observation?.Identification?.Verified;

        /*[Obsolete]
        public bool? IdentificationValidated => IdentificationVerified;
        [Obsolete]
        public string IdentificationValidationStatus => IdentificationVerificationStatus;
        [Obsolete]
        public int? IdentificationValidationStatusId => IdentificationVerificationStatusId;
        [Obsolete]
        public string IdentificationValidationStatusValue => IdentificationVerificationStatusValue;
        */
        public string IdentificationVerificationStatus => _observation?.Identification?.VerificationStatus?.ToString();
        public int? IdentificationVerificationStatusId => _observation?.Identification?.VerificationStatus?.Id;
        public string IdentificationVerificationStatusValue => _observation?.Identification?.VerificationStatus?.Value;
        public string IdentificationIdentifiedBy => _observation?.Identification?.IdentifiedBy;
        public string IdentificationTypeStatus => _observation?.Identification?.TypeStatus;
        public bool? IdentificationUncertainIdentification => _observation?.Identification?.UncertainIdentification;
        public string IdentificationDeterminationMethod => _observation?.Identification?.DeterminationMethod?.ToString();
        public int? IdentificationDeterminationMethodId => _observation?.Identification?.DeterminationMethod?.Id;
        public string IdentificationDeterminationMethodValue => _observation?.Identification?.DeterminationMethod?.Value;
        public string IdentificationVerifiedBy => _observation?.Identification?.VerifiedBy;
        public string LocationContinent => _observation?.Location?.Continent?.ToString();
        public int? LocationContinentId => _observation?.Location?.Continent?.Id;
        public string LocationContinentValue => _observation?.Location?.Continent?.Value;
        public string LocationType => _observation?.Location?.Type.ToString();
        public double? LocationCoordinatePrecision => _observation?.Location?.CoordinatePrecision;
        public double? LocationCoordinateUncertaintyInMeters => _observation?.Location?.CoordinateUncertaintyInMeters;
        public string LocationCountry => _observation?.Location?.Country?.ToString();
        public int? LocationCountryId => _observation?.Location?.Country?.Id;
        public string LocationCountryValue => _observation?.Location?.Country?.Value;
        public string LocationCountryCode => _observation?.Location?.CountryCode;
        public string LocationCountryRegion => _observation?.Location?.CountryRegion?.ToString();
        public string LocationCountryRegionFeatureId => _observation?.Location?.CountryRegion?.FeatureId;
        public string LocationCountryRegionName => _observation?.Location?.CountryRegion?.Name;
        public string LocationCounty => _observation?.Location?.County?.ToString();
        public string LocationCountyFeatureId => _observation?.Location?.County?.FeatureId;
        public string LocationCountyName => _observation?.Location?.County?.Name;
        public string LocationMunicipality => _observation?.Location?.Municipality?.ToString();
        public string LocationMunicipalityFeatureId => _observation?.Location?.Municipality?.FeatureId;
        public string LocationMunicipalityName => _observation?.Location?.Municipality?.Name;
        public string LocationParish => _observation?.Location?.Parish?.ToString();
        public string LocationParishFeatureId => _observation?.Location?.Parish?.FeatureId;
        public string LocationParishName => _observation?.Location?.Parish?.Name;
        public string LocationProvince => _observation?.Location?.Province?.ToString();
        public string LocationProvinceFeatureId => _observation?.Location?.Province?.FeatureId;
        public string LocationProvinceName => _observation?.Location?.Province?.Name;
        public double? LocationDecimalLatitude => _observation?.Location?.DecimalLatitude;
        public double? LocationDecimalLongitude => _observation?.Location?.DecimalLongitude;
        public double? LocationEtrs89X => _observation?.Location?.Etrs89X;
        public double? LocationEtrs89Y => _observation?.Location?.Etrs89Y;
        public double? LocationSweref99TmX => _observation?.Location?.Sweref99TmX;
        public double? LocationSweref99TmY => _observation?.Location?.Sweref99TmY;
        public string LocationFootprintSpatialFit => _observation?.Location?.FootprintSpatialFit;
        public string LocationFootprintSRS => _observation?.Location?.FootprintSRS;
        public string LocationFootprintWKT => _observation?.Location?.FootprintWKT;
        public string LocationGeodeticDatum => _observation?.Location?.GeodeticDatum;
        public string LocationGeoreferencedBy => _observation?.Location?.GeoreferencedBy;
        public string LocationGeoreferencedDate => _observation?.Location?.GeoreferencedDate;
        public string LocationGeoreferenceProtocol => _observation?.Location?.GeoreferenceProtocol;
        public string LocationGeoreferenceRemarks => _observation?.Location?.GeoreferenceRemarks;
        public string LocationGeoreferenceSources => _observation?.Location?.GeoreferenceSources;
        public string LocationGeoreferenceVerificationStatus => _observation?.Location?.GeoreferenceVerificationStatus;
        public string LocationHigherGeography => _observation?.Location?.HigherGeography;
        public string LocationHigherGeographyId => _observation?.Location?.HigherGeographyId;
        public string LocationIsland => _observation?.Location?.Island;
        public string LocationIslandGroup => _observation?.Location?.IslandGroup;
        public string LocationLocality => _observation?.Location?.Locality;
        public string LocationLocationAccordingTo => _observation?.Location?.LocationAccordingTo;
        public string LocationLocationId => _observation?.Location?.LocationId;
        public string LocationLocationRemarks => _observation?.Location?.LocationRemarks;
        public double? LocationMaximumDepthInMeters => _observation?.Location?.MaximumDepthInMeters;
        public double? LocationMaximumDistanceAboveSurfaceInMeters => _observation?.Location?.MaximumDistanceAboveSurfaceInMeters;
        public double? LocationMaximumElevationInMeters => _observation?.Location?.MaximumElevationInMeters;
        public double? LocationMinimumDepthInMeters => _observation?.Location?.MinimumDepthInMeters;
        public double? LocationMinimumDistanceAboveSurfaceInMeters => _observation?.Location?.MinimumDistanceAboveSurfaceInMeters;
        public double? LocationMinimumElevationInMeters => _observation?.Location?.MinimumElevationInMeters;
        public string LocationPointRadiusSpatialFit => _observation?.Location?.PointRadiusSpatialFit;
        public string LocationVerbatimCoordinates => _observation?.Location?.VerbatimCoordinates;
        public string LocationVerbatimCoordinateSystem => _observation?.Location?.VerbatimCoordinateSystem;
        public string LocationVerbatimDepth => _observation?.Location?.VerbatimDepth;
        public string LocationVerbatimElevation => _observation?.Location?.VerbatimElevation;
        public string LocationVerbatimLatitude => _observation?.Location?.VerbatimLatitude;
        public string LocationVerbatimLocality => _observation?.Location?.VerbatimLocality;
        public string LocationVerbatimLongitude => _observation?.Location?.VerbatimLongitude;
        public string LocationVerbatimSRS => _observation?.Location?.VerbatimSRS;
        public string LocationWaterBody => _observation?.Location?.WaterBody;
        public string MaterialSampleMaterialSampleId => _observation?.MaterialSample?.MaterialSampleId;
        public string OccurrenceActivity => _observation?.Occurrence?.Activity?.ToString();
        public int? OccurrenceActivityId => _observation?.Occurrence?.Activity?.Id;
        public string OccurrenceActivityValue => _observation?.Occurrence?.Activity?.Value;
        public string OccurrenceAssociatedMedia => _observation?.Occurrence?.AssociatedMedia;
        public string OccurrenceAssociatedOccurrences => _observation?.Occurrence?.AssociatedOccurrences;
        public string OccurrenceAssociatedReferences => _observation?.Occurrence?.AssociatedReferences;
        public string OccurrenceAssociatedSequences => _observation?.Occurrence?.AssociatedSequences;
        public string OccurrenceAssociatedTaxa => _observation?.Occurrence?.AssociatedTaxa;
        public string OccurrenceBehavior => _observation?.Occurrence?.Behavior?.ToString();
        public int? OccurrenceBehaviorId => _observation?.Occurrence?.Behavior?.Id;
        public string OccurrenceBehaviorValue => _observation?.Occurrence?.Behavior?.Value;
        public string OccurrenceBiotope => _observation?.Occurrence?.Biotope?.ToString();
        public int? OccurrenceBiotopeId => _observation?.Occurrence?.Biotope?.Id;
        public string OccurrenceBiotopeValue => _observation?.Occurrence?.Biotope?.Value;
        public string OccurrenceBiotopeDescription => _observation?.Occurrence?.BiotopeDescription;
        public int? OccurrenceBirdNestActivityId => _observation?.Occurrence?.BirdNestActivityId;
        public string OccurrenceCatalogNumber => _observation?.Occurrence?.CatalogNumber;
        public int? OccurrenceCatalogId => _observation?.Occurrence?.CatalogId;
        public string OccurrenceDisposition => _observation?.Occurrence?.Disposition;
        public string OccurrenceEstablishmentMeans => _observation?.Occurrence?.EstablishmentMeans?.ToString();
        public int? OccurrenceEstablishmentMeansId => _observation?.Occurrence?.EstablishmentMeans?.Id;
        public string OccurrenceEstablishmentMeansValue => _observation?.Occurrence?.EstablishmentMeans?.Value;
        public string OccurrenceIndividualCount => _observation?.Occurrence?.IndividualCount;
      //  public string OccurrenceIndividualId => _observation?.Occurrence?.IndividualId;
        public bool? OccurrenceIsNaturalOccurrence => _observation?.Occurrence?.IsNaturalOccurrence;
        public bool? OccurrenceIsNeverFoundObservation => _observation?.Occurrence?.IsNeverFoundObservation;
        public bool? OccurrenceIsNotRediscoveredObservation => _observation?.Occurrence?.IsNotRediscoveredObservation;
        public bool? OccurrenceIsPositiveObservation => _observation?.Occurrence?.IsPositiveObservation;
        public string OccurrenceLifeStage => _observation?.Occurrence?.LifeStage?.ToString();
        public int? OccurrenceLifeStageId => _observation?.Occurrence?.LifeStage?.Id;
        public string OccurrenceLifeStageValue => _observation?.Occurrence?.LifeStage?.Value;
        public string OccurrenceMedia => _observation?.Occurrence?.Media == null ? null : string.Join(", ", _observation?.Occurrence?.Media.Select(m => m.ToString()));
        public string OccurrenceOccurrenceId => _observation?.Occurrence?.OccurrenceId;
        public string OccurrenceOccurrenceRemarks => _observation?.Occurrence?.OccurrenceRemarks;
        public string OccurrenceOccurrenceStatus => _observation?.Occurrence?.OccurrenceStatus?.ToString();
        public int? OccurrenceOccurrenceStatusId => _observation?.Occurrence?.OccurrenceStatus?.Id;
        public string OccurrenceOccurrenceStatusValue => _observation?.Occurrence?.OccurrenceStatus?.Value;
        public string OccurrenceOtherCatalogNumbers => _observation?.Occurrence?.OtherCatalogNumbers;
        public string OccurrenceOrganismQuantity => _observation?.Occurrence?.OrganismQuantity;
        public int? OccurrenceOrganismQuantityInt => _observation?.Occurrence?.OrganismQuantityInt;
        public string OccurrenceOrganismQuantityUnit => _observation?.Occurrence?.OrganismQuantityUnit?.ToString();
        public int? OccurrenceOrganismQuantityUnitId => _observation?.Occurrence?.OrganismQuantityUnit?.Id;
        public string OccurrenceOrganismQuantityUnitValue => _observation?.Occurrence?.OrganismQuantityUnit?.Value;
        public string OccurrencePreparations => _observation?.Occurrence?.Preparations;
       
        //[Obsolete]
        //public int? OccurrenceProtectionLevel => OccurrenceSensitivityCategory;
       
        public int? OccurrenceSensitivityCategory => _observation?.Occurrence?.SensitivityCategory;
        public string OccurrenceRecordedBy => _observation?.Occurrence?.RecordedBy;
        public string OccurrenceRecordNumber => _observation?.Occurrence?.RecordNumber;
        public string OccurrenceReportedBy => _observation?.Occurrence?.ReportedBy;
        public DateTime? OccurrenceReportedDate
        {
            get
            {
                var date = _observation?.Occurrence?.ReportedDate;
                if (date == null) return null;
                var swedenDate = TimeZoneInfo.ConvertTimeFromUtc(date.Value, swedenTimeZone);
                return swedenDate;
                //return _observation?.Occurrence?.ReportedDate;
            }
        }
        public string OccurrenceReproductiveCondition => _observation?.Occurrence?.ReproductiveCondition?.ToString();
        public int? OccurrenceReproductiveConditionId => _observation?.Occurrence?.ReproductiveCondition?.Id;
        public string OccurrenceReproductiveConditionValue => _observation?.Occurrence?.ReproductiveCondition?.Value;
        public string OccurrenceSex => _observation?.Occurrence?.Sex?.ToString();
        public int? OccurrenceSexId => _observation?.Occurrence?.Sex?.Id;
        public string OccurrenceSexValue => _observation?.Occurrence?.Sex?.Value;
        public string OccurrenceSubstrateDescription => _observation?.Occurrence?.Substrate?.Description;
        public int? OccurrenceSubstrateId => _observation?.Occurrence?.Substrate?.Id;
        public string OccurrenceSubstrateName => _observation?.Occurrence?.Substrate?.Name?.ToString();
        public int? OccurrenceSubstrateNameId => _observation?.Occurrence?.Substrate?.Name?.Id;
        public string OccurrenceSubstrateNameValue => _observation?.Occurrence?.Substrate?.Name?.Value;
        public int? OccurrenceSubstrateQuantity => _observation?.Occurrence?.Substrate?.Quantity;
        public string OccurrenceSubstrateSpeciesDescription => _observation?.Occurrence?.Substrate?.SpeciesDescription;
        public int? OccurrenceSubstrateSpeciesId => _observation?.Occurrence?.Substrate?.SpeciesId;
        public string OccurrenceSubstrateSpeciesScientificName => _observation?.Occurrence?.Substrate?.SpeciesScientificName;
        public string OccurrenceSubstrateSpeciesVernacularName => _observation?.Occurrence?.Substrate?.SpeciesVernacularName;
        public string OccurrenceUrl => _observation?.Occurrence?.Url;
        public int? OccurrenceLength => _observation?.Occurrence?.Length;
        public int? OccurrenceWeight => _observation?.Occurrence?.Weight;
        public string OrganismOrganismId => _observation?.Organism?.OrganismId;
        public string OrganismOrganismName => _observation?.Organism?.OrganismName;
        public string OrganismOrganismScope => _observation?.Organism?.OrganismScope;
        public string OrganismAssociatedOrganisms => _observation?.Organism?.AssociatedOrganisms;
        public string OrganismPreviousIdentifications => _observation?.Organism?.PreviousIdentifications;
        public string OrganismOrganismRemarks => _observation?.Organism?.OrganismRemarks;
        public string TaxonAcceptedNameUsage => _observation?.Taxon?.AcceptedNameUsage;
        public string TaxonAcceptedNameUsageId => _observation?.Taxon?.AcceptedNameUsageId;
        public string TaxonAttributesActionPlan => _observation?.Taxon?.Attributes?.ActionPlan;
        public int? TaxonAttributesDisturbanceRadius => _observation?.Taxon?.Attributes?.DisturbanceRadius;
        public int? TaxonAttributesDyntaxaTaxonId => _observation?.Taxon?.Attributes?.DyntaxaTaxonId;
        public bool? TaxonAttributesNatura2000HabitatsDirectiveArticle2 => _observation?.Taxon?.Attributes?.Natura2000HabitatsDirectiveArticle2;
        public bool? TaxonAttributesNatura2000HabitatsDirectiveArticle4 => _observation?.Taxon?.Attributes?.Natura2000HabitatsDirectiveArticle4;
        public bool? TaxonAttributesNatura2000HabitatsDirectiveArticle5 => _observation?.Taxon?.Attributes?.Natura2000HabitatsDirectiveArticle5;
        public string TaxonAttributesOrganismGroup => _observation?.Taxon?.Attributes?.OrganismGroup;
        public int? TaxonAttributesParentDyntaxaTaxonId => _observation?.Taxon?.Attributes?.ParentDyntaxaTaxonId;
        public bool? TaxonAttributesProtectedByLaw => _observation?.Taxon?.Attributes?.ProtectedByLaw;
       
       /* [Obsolete]
        public string TaxonAttributesProtectionLevel => TaxonAttributesSensitivityCategory;
        [Obsolete]
        public int? TaxonAttributesProtectionLevelId => TaxonAttributesSensitivityCategoryId;
        [Obsolete]
        public string TaxonAttributesProtectionLevelValue => TaxonAttributesSensitivityCategoryValue;
        */
        public string TaxonAttributesSensitivityCategory => _observation?.Taxon?.Attributes?.SensitivityCategory?.ToString();
        public int? TaxonAttributesSensitivityCategoryId => _observation?.Taxon?.Attributes?.SensitivityCategory?.Id;
        public string TaxonAttributesSensitivityCategoryValue => _observation?.Taxon?.Attributes?.SensitivityCategory?.Value;
        public string TaxonAttributesTaxonCategory => _observation?.Taxon?.Attributes?.TaxonCategory?.ToString();
        public int? TaxonAttributesTaxonCategoryId => _observation?.Taxon?.Attributes?.TaxonCategory?.Id;
        public string TaxonAttributesTaxonCategoryValue => _observation?.Taxon?.Attributes?.TaxonCategory?.Value;
        public bool? TaxonAttributesIsRedlisted => _observation?.Taxon?.Attributes?.IsRedlisted;
        public bool? TaxonAttributesIsInvasiveAccordingToEuRegulation => _observation?.Taxon?.Attributes?.IsInvasiveAccordingToEuRegulation;
        public bool? TaxonAttributesIsInvasiveInSweden => _observation?.Taxon?.Attributes?.IsInvasiveInSweden;
        public string TaxonAttributesRiskAssessmentCategory => _observation?.Taxon?.Attributes?.InvasiveRiskAssessmentCategory;
        public string TaxonAttributesRedlistCategory => _observation?.Taxon?.Attributes?.RedlistCategory;
        public int? TaxonAttributesSortOrder => _observation?.Taxon?.Attributes?.SortOrder;
        public string TaxonAttributesSwedishHistory => _observation?.Taxon?.Attributes?.SwedishHistory;
        public string TaxonAttributesSwedishOccurrence => _observation?.Taxon?.Attributes?.SwedishOccurrence;
        public string TaxonAttributesSynonyms => _observation?.Taxon?.Attributes?.Synonyms == null ? null : string.Join(", ", _observation?.Taxon?.Attributes?.Synonyms);
        public string TaxonAttributesVernacularNames => _observation?.Taxon?.Attributes?.VernacularNames == null ? null : string.Join(", ", _observation?.Taxon?.Attributes?.VernacularNames);
        public bool? TaxonBirdDirective => _observation?.Taxon?.BirdDirective;
        public string TaxonClass => _observation?.Taxon?.Class;
        public string TaxonFamily => _observation?.Taxon?.Family;
        public string TaxonGenus => _observation?.Taxon?.Genus;
        public string TaxonHigherClassification => _observation?.Taxon?.HigherClassification;
        public int? TaxonId => _observation?.Taxon?.Id;
        public string TaxonInfraspecificEpithet => _observation?.Taxon?.InfraspecificEpithet;
        public string TaxonKingdom => _observation?.Taxon?.Kingdom;
        public string TaxonNameAccordingTo => _observation?.Taxon?.NameAccordingTo;
        public string TaxonNameAccordingToId => _observation?.Taxon?.NameAccordingToId;
        public string TaxonNamePublishedIn => _observation?.Taxon?.NamePublishedIn;
        public string TaxonNamePublishedInId => _observation?.Taxon?.NamePublishedInId;
        public string TaxonNamePublishedInYear => _observation?.Taxon?.NamePublishedInYear;
        public string TaxonNomenclaturalCode => _observation?.Taxon?.NomenclaturalCode;
        public string TaxonNomenclaturalStatus => _observation?.Taxon?.NomenclaturalStatus;
        public string TaxonOrder => _observation?.Taxon?.Order;
        public string TaxonOriginalNameUsage => _observation?.Taxon?.OriginalNameUsage;
        public string TaxonOriginalNameUsageId => _observation?.Taxon?.OriginalNameUsageId;
        public string TaxonParentNameUsage => _observation?.Taxon?.ParentNameUsage;
        public string TaxonParentNameUsageId => _observation?.Taxon?.ParentNameUsageId;
        public string TaxonPhylum => _observation?.Taxon?.Phylum;
        public string TaxonScientificName => _observation?.Taxon?.ScientificName;
        public string TaxonScientificNameAuthorship => _observation?.Taxon?.ScientificNameAuthorship;
        public string TaxonScientificNameId => _observation?.Taxon?.ScientificNameId;
        //public string TaxonSecondaryParentDyntaxaTaxonIds => _observation?.Taxon?.SecondaryParentDyntaxaTaxonIds;
        public string TaxonSpecificEpithet => _observation?.Taxon?.SpecificEpithet;
        public string TaxonSubgenus => _observation?.Taxon?.Subgenus;
        public string TaxonTaxonConceptId => _observation?.Taxon?.TaxonConceptId;
        public string TaxonTaxonId => _observation?.Taxon?.TaxonId;
        public string TaxonTaxonomicStatus => _observation?.Taxon?.TaxonomicStatus;
        public string TaxonTaxonRank => _observation?.Taxon?.TaxonRank;
        public string TaxonTaxonRemarks => _observation?.Taxon?.TaxonRemarks;
        public string TaxonVerbatimTaxonRank => _observation?.Taxon?.VerbatimTaxonRank;
        public string TaxonVernacularName => _observation?.Taxon?.VernacularName;
        public string AccessRights => _observation?.AccessRights?.ToString();
        public int? AccessRightsId => _observation?.AccessRights?.Id;
        public string AccessRightsValue => _observation?.AccessRights?.Value;
        public string BasisOfRecord => _observation?.BasisOfRecord?.ToString();
        public int? BasisOfRecordId => _observation?.BasisOfRecord?.Id;
        public string BasisOfRecordValue => _observation?.BasisOfRecord?.Value;
        public string BibliographicCitation => _observation?.BibliographicCitation;
        public string CollectionCode => _observation?.CollectionCode;
        public string CollectionId => _observation?.CollectionId;
        public string DataGeneralizations => _observation?.DataGeneralizations;
        public int? DataProviderId => _observation?.DataProviderId;
        public string DatasetId => _observation?.DatasetId;
        public string DatasetName => _observation?.DatasetName;
        public string DynamicProperties => _observation?.DynamicProperties;
        public string Id => _observation?.Id;
        public string InformationWithheld => _observation?.InformationWithheld;
        public string InstitutionId => _observation?.InstitutionId;
        public string InstitutionCode => _observation?.InstitutionCode?.ToString();
        public int? InstitutionCodeId => _observation?.InstitutionCode?.Id;
        public string InstitutionCodeValue => _observation?.InstitutionCode?.Value;
        public string Language => _observation?.Language;
        public string License => _observation?.License;
        public DateTime? Modified
        {
            get
            {
                var date = _observation?.Modified;
                if (date == null) return null;
                var swedenDate = TimeZoneInfo.ConvertTimeFromUtc(date.Value, swedenTimeZone);
                return swedenDate;
                //return _observation?.Modified;
            }
        }
        public string OwnerInstitutionCode => _observation?.OwnerInstitutionCode;
        public string PrivateCollection => _observation?.PrivateCollection;

      //  [Obsolete]
      //  public bool? Protected => Sensitive;

        public bool? Sensitive => _observation?.Sensitive;
        public string PublicCollection => _observation?.PublicCollection;
        public string References => _observation?.References;
        public string RightsHolder => _observation?.RightsHolder;
        public string SpeciesCollectionLabel => _observation?.SpeciesCollectionLabel;
        public string Type => _observation?.Type?.ToString();
        public int? TypeId => _observation?.Type?.Id;
        public string TypeValue => _observation?.Type?.Value;
        public string MeasurementOrFacts => _observation?.MeasurementOrFacts == null ? null : string.Join(", ", _observation.MeasurementOrFacts.Select(m => m.ToString()));
        public string Projects => _observation?.Projects == null ? null : string.Join(", ", _observation.Projects.Select(m => m.ToString()));

        public string TaxonSecondaryParentDyntaxaTaxonIds => _observation?.Taxon?.SecondaryParentDyntaxaTaxonIds == null
            ? null
            : string.Join(", ", _observation?.Taxon?.SecondaryParentDyntaxaTaxonIds);
        public object GetValue(PropertyFieldDescription propertyField)
        {
            return propertyField.PropertyPath.ToLower() switch
            {    
                "occurrence.occurrenceid" => OccurrenceOccurrenceId,
                "dataproviderid" => DataProviderId,
                "datasetname" => DatasetName,
                "modified" => Modified,
                "sensitive" => Sensitive,
                "accessrights" => AccessRights,
                "accessrights.id" => AccessRightsId,
                "accessrights.value" => AccessRightsValue,
                "basisofrecord" => BasisOfRecord,
                "basisofrecord.id" => BasisOfRecordId,
                "basisofrecord.value" => BasisOfRecordValue,
                "collectioncode" => CollectionCode,
                "collectionid" => CollectionId,
                "institutioncode" => InstitutionCode,
                "institutioncode.id" => InstitutionCodeId,
                "institutioncode.value" => InstitutionCodeValue,
                "ownerinstitutioncode" => OwnerInstitutionCode,
                "rightsholder" => RightsHolder,
                "event" => "Please specify which event properties you need",
                "event.startdate" => EventStartDate,
                "event.enddate" => EventEndDate,
                "event.plainstartdate" => EventPlainStartDate,
                "event.plainenddate" => EventPlainEndDate,
                "event.plainstarttime" => EventStartTimeString,
                "event.plainendtime" => EventEndTimeString,
                "event.eventid" => EventEventId,
                "event.eventremarks" => EventEventRemarks,
                "event.habitat" => EventHabitat,
                "event.discoverymethod" => EventDiscoveryMethod,
                "event.discoverymethod.id" => EventDiscoveryMethodId,
                "event.discoverymethod.value" => EventDiscoveryMethodValue,
                "event.samplingeffort" => EventSamplingEffort,
                "event.samplingprotocol" => EventSamplingProtocol,
                "event.samplesizeunit" => EventSampleSizeUnit,
                "event.samplesizevalue" => EventSampleSizeValue,
                "event.media" => EventMedia,
                "event.measurementorfacts" => EventMeasurementOrFacts,
                "event.fieldnotes" => EventFieldNotes,
                "event.fieldnumber" => EventFieldNumber,
                "event.parenteventid" => EventParentEventId,
                "event.verbatimeventdate" => EventVerbatimEventDate,
                "identification" => "Please specify which identification properties you need",
                "identification.verified" => IdentificationVerified,
                "identification.verificationstatus" => IdentificationVerificationStatus,
                "identification.verificationstatus.id" => IdentificationVerificationStatusId,
                "identification.verificationstatus.value" => IdentificationVerificationStatusValue,
                "identification.confirmedby" => IdentificationConfirmedBy,
                "identification.confirmeddate" => IdentificationConfirmedDate,
                "identification.identifiedby" => IdentificationIdentifiedBy,
                "identification.dateidentified" => IdentificationDateIdentified,
                "identification.uncertainidentification" => IdentificationUncertainIdentification,
                "identification.determinationmethod" => IdentificationDeterminationMethod,
                "identification.determinationmethod.id" => IdentificationDeterminationMethodId,
                "identification.determinationmethod.value" => IdentificationDeterminationMethodValue,
                "identification.verifiedby" => IdentificationVerifiedBy,
                "identification.identificationqualifier" => IdentificationIdentificationQualifier,
                "identification.typestatus" => IdentificationTypeStatus,
                "identification.identificationid" => IdentificationIdentificationId,
                "identification.identificationreferences" => IdentificationIdentificationReferences,
                "identification.identificationremarks" => IdentificationIdentificationRemarks,
                "location" => "Please specify which location properties you need",
                "location.decimallatitude" => LocationDecimalLatitude,
                "location.decimallongitude" => LocationDecimalLongitude,
                "location.sweref99tmx" => LocationSweref99TmX,
                "location.sweref99tmy" => LocationSweref99TmY,
                "location.geodeticdatum" => LocationGeodeticDatum,
                "location.coordinateuncertaintyinmeters" => LocationCoordinateUncertaintyInMeters,
                "location.locationid" => LocationLocationId,
                "location.locality" => LocationLocality,
                "location.locationremarks" => LocationLocationRemarks,
                "location.municipality" => LocationMunicipality,
                "location.municipality.featureid" => LocationMunicipalityFeatureId,
                "location.municipality.name" => LocationMunicipalityName,
                "location.county" => LocationCounty,
                "location.county.featureid" => LocationCountyFeatureId,
                "location.county.name" => LocationCountyName,
                "location.parish" => LocationParish,
                "location.parish.featureid" => LocationParishFeatureId,
                "location.parish.name" => LocationParishName,
                "location.province" => LocationProvince,
                "location.province.featureid" => LocationProvinceFeatureId,
                "location.province.name" => LocationProvinceName,
                "location.continent" => LocationContinent,
                "location.continent.id" => LocationContinentId,
                "location.continent.value" => LocationContinentValue,
                "location.country" => LocationCountry,
                "location.country.id" => LocationCountryId,
                "location.country.value" => LocationCountryValue,
                "location.countrycode" => LocationCountryCode,
                "location.georeferencedby" => LocationGeoreferencedBy,
                "location.georeferenceddate" => LocationGeoreferencedDate,
                "location.georeferenceremarks" => LocationGeoreferenceRemarks,
                "location.highergeography" => LocationHigherGeography,
                "location.island" => LocationIsland,
                "location.maximumdepthinmeters" => LocationMaximumDepthInMeters,
                "location.maximumelevationinmeters" => LocationMaximumElevationInMeters,
                "location.minimumdepthinmeters" => LocationMinimumDepthInMeters,
                "location.minimumelevationinmeters" => LocationMinimumElevationInMeters,
                "location.verbatimcoordinatesystem" => LocationVerbatimCoordinateSystem,
                "location.verbatimlatitude" => LocationVerbatimLatitude,
                "location.verbatimlongitude" => LocationVerbatimLongitude,
                "location.verbatimsrs" => LocationVerbatimSRS,
                "location.waterbody" => LocationWaterBody,
                "location.coordinateprecision" => LocationCoordinatePrecision,
                "location.footprintspatialfit" => LocationFootprintSpatialFit,
                "location.footprintsrs" => LocationFootprintSRS,
                "location.footprintwkt" => LocationFootprintWKT,
                "location.georeferenceprotocol" => LocationGeoreferenceProtocol,
                "location.georeferencesources" => LocationGeoreferenceSources,
                "location.georeferenceverificationstatus" => LocationGeoreferenceVerificationStatus,
                "location.highergeographyid" => LocationHigherGeographyId,
                "location.islandgroup" => LocationIslandGroup,
                "location.locationaccordingto" => LocationLocationAccordingTo,
                "location.maximumdistanceabovesurfaceinmeters" => LocationMaximumDistanceAboveSurfaceInMeters,
                "location.minimumdistanceabovesurfaceinmeters" => LocationMinimumDistanceAboveSurfaceInMeters,
                "location.pointradiusdpatialfit" => LocationPointRadiusSpatialFit,
                "location.verbatimcoordinates" => LocationVerbatimCoordinates,
                "location.verbatimdepth" => LocationVerbatimDepth,
                "location.verbatimelevation" => LocationVerbatimElevation,
                "location.verbatimlocality" => LocationVerbatimLocality,
                "location.type" => LocationType,
                "occurrence" => "Please specify which occurrence properties you need",
                "occurrence.recordedby" => OccurrenceRecordedBy,
                "occurrence.reportedby" => OccurrenceReportedBy,
                "occurrence.occurrencestatus" => OccurrenceOccurrenceStatus,
                "occurrence.occurrencestatus.id" => OccurrenceOccurrenceStatusId,
                "occurrence.occurrencestatus.value" => OccurrenceOccurrenceStatusValue,
                "occurrence.individualcount" => OccurrenceIndividualCount,
                "occurrence.organismquantity" => OccurrenceOrganismQuantity,
                "occurrence.organismquantityint" => OccurrenceOrganismQuantityInt,
                "occurrence.organismquantityunit" => OccurrenceOrganismQuantityUnit,
                "occurrence.organismquantityunit.id" => OccurrenceOrganismQuantityUnitId,
                "occurrence.organismquantityunit.value" => OccurrenceOrganismQuantityUnitValue,
                "occurrence.isnaturaloccurrence" => OccurrenceIsNaturalOccurrence,
                "occurrence.isneverfoundobservation" => OccurrenceIsNeverFoundObservation,
                "occurrence.isnotrediscoveredobservation" => OccurrenceIsNotRediscoveredObservation,
                "occurrence.ispositiveobservation" => OccurrenceIsPositiveObservation,
                "occurrence.occurrenceremarks" => OccurrenceOccurrenceRemarks,
                "occurrence.sensitivitycategory" => OccurrenceSensitivityCategory,
                "occurrence.activity" => OccurrenceActivity,
                "occurrence.activity.id" => OccurrenceActivityId,
                "occurrence.activity.value" => OccurrenceActivityValue,
                "occurrence.behavior" => OccurrenceBehavior,
                "occurrence.behavior.id" => OccurrenceBehaviorId,
                "occurrence.behavior.value" => OccurrenceBehaviorValue,
                "occurrence.biotope" => OccurrenceBiotope,
                "occurrence.biotope.id" => OccurrenceBiotopeId,
                "occurrence.biotope.value" => OccurrenceBiotopeValue,
                "occurrence.biotopedescription" => OccurrenceBiotopeDescription,
                "occurrence.associatedmedia" => OccurrenceAssociatedMedia,
                "occurrence.lifestage" => OccurrenceLifeStage,
                "occurrence.lifestage.id" => OccurrenceLifeStageId,
                "occurrence.lifestage.value" => OccurrenceLifeStageValue,
                "occurrence.OccurrenceStatus" => OccurrenceOccurrenceStatus,
                "occurrence.reproductivecondition" => OccurrenceReproductiveCondition,
                "occurrence.reproductivecondition.id" => OccurrenceReproductiveConditionId,
                "occurrence.reproductivecondition.value" => OccurrenceReproductiveConditionValue,
                "occurrence.sex" => OccurrenceSex, 
                "occurrence.sex.id" => OccurrenceSexId,
                "occurrence.sex.value" => OccurrenceSexValue,
                "occurrence.url" => OccurrenceUrl,
                "occurrence.length" => OccurrenceLength,
                "occurrence.weight" => OccurrenceWeight,
                "occurrence.substrate.description" => OccurrenceSubstrateDescription,
                "occurrence.substrate.id" => OccurrenceSubstrateId,
                "occurrence.substrate.name" => OccurrenceSubstrateName,
                "occurrence.substrate.name.id" => OccurrenceSubstrateNameId,
                "occurrence.substrate.name.value" => OccurrenceSubstrateNameValue,
                "occurrence.substrate.quantity" => OccurrenceSubstrateQuantity,
                "occurrence.substrate.speciesdescription" => OccurrenceSubstrateSpeciesDescription,
                "occurrence.substrate.speciesid" => OccurrenceSubstrateSpeciesId,
                "occurrence.substrate.speciesscientificname" => OccurrenceSubstrateSpeciesScientificName,
                "occurrence.substrate.speciesvernacularname" => OccurrenceSubstrateSpeciesVernacularName,
                "occurrence.birdnestactivityid" => OccurrenceBirdNestActivityId,
                "occurrence.catalognumber" => OccurrenceCatalogNumber,
                "occurrence.catalogid" => OccurrenceCatalogId,
                "occurrence.associatedreferences" => OccurrenceAssociatedReferences,
                "occurrence.media" => OccurrenceMedia,
                "occurrence.preparations" => OccurrencePreparations,
                "occurrence.recordnumber" => OccurrenceRecordNumber,
                "occurrence.reporteddate" => OccurrenceReportedDate,
                "occurrence.associatedoccurrences" => OccurrenceAssociatedOccurrences,
                "occurrence.associatedsequences" => OccurrenceAssociatedSequences,
                "occurrence.associatedtaxa" => OccurrenceAssociatedTaxa,
                "occurrence.disposition" => OccurrenceDisposition,
                "occurrence.establishmentmeans" => OccurrenceEstablishmentMeans,
                "occurrence.establishmentmeans.id" => OccurrenceEstablishmentMeansId,
                "occurrence.establishmentmeans.value" => OccurrenceEstablishmentMeansValue,
                "occurrence.othercatalognumbers" => OccurrenceOtherCatalogNumbers,
                "taxon" => "Please specify which taxon properties you need",
                "taxon.acceptednameusage" => TaxonAcceptedNameUsage,
                "taxon.acceptednameusageid" => TaxonAcceptedNameUsageId,
                "taxon.birddirective" => TaxonBirdDirective,
                "taxon.higherclassification" => TaxonHigherClassification,
                "taxon.id" => TaxonId,
                "taxon.scientificname" => TaxonScientificName,
                "taxon.scientificnameauthorship" => TaxonScientificNameAuthorship,
                "taxon.vernacularname" => TaxonVernacularName,
                "taxon.kingdom" => TaxonKingdom,
                "taxon.phylum" => TaxonPhylum,
                "taxon.class" => TaxonClass,
                "taxon.order" => TaxonOrder,
                "taxon.family" => TaxonFamily,
                "taxon.genus" => TaxonGenus,
                "taxon.infraspecificepithet" => TaxonInfraspecificEpithet,
                "taxon.nameaccordingto" => TaxonNameAccordingTo,
                "taxon.nameaccordingtoid" => TaxonNameAccordingToId,
                "taxon.namepublishedin" => TaxonNamePublishedIn,
                "taxon.namepublishedinid" => TaxonNamePublishedInId,
                "taxon.namepublishedinyear" => TaxonNamePublishedInYear,
                "taxon.nomenclaturalcode" => TaxonNomenclaturalCode,
                "taxon.nomenclaturalstatus" => TaxonNomenclaturalStatus,
                "taxon.originalnameusage" => TaxonOriginalNameUsage,
                "taxon.originalnameusageid" => TaxonOriginalNameUsageId,
                "taxon.parentnameusage" => TaxonParentNameUsage,
                "taxon.parentnameusageid" => TaxonParentNameUsageId,
                "taxon.scientificnameid" => TaxonScientificNameId,
                "taxon.secondaryparentdyntaxataxonids" => TaxonSecondaryParentDyntaxaTaxonIds,
                "taxon.specificepithet" => TaxonSpecificEpithet,
                "taxon.subgenus" => TaxonSubgenus,
                "taxon.taxonconceptid" => TaxonTaxonConceptId,
                "taxon.taxonid" => TaxonTaxonId,
                "taxon.taxonomicstatus" => TaxonTaxonomicStatus,
                "taxon.taxonrank" => TaxonTaxonRank,
                "taxon.taxonremarks" => TaxonTaxonRemarks,
                "taxon.verbatimtaxonrank" => TaxonVerbatimTaxonRank,
                "taxon.attributes" => "Please specify which taxon.attributes properties you need",
                "taxon.attributes.actionplan" => TaxonAttributesActionPlan,
                "taxon.attributes.disturbanceradius" => TaxonAttributesDisturbanceRadius,
                "taxon.attributes.dyntaxataxonid" => TaxonAttributesDyntaxaTaxonId,
                "taxon.attributes.natura2000habitatsdirectivearticle2" => TaxonAttributesNatura2000HabitatsDirectiveArticle2,
                "taxon.attributes.natura2000habitatsdirectivearticle4" => TaxonAttributesNatura2000HabitatsDirectiveArticle4,
                "taxon.attributes.natura2000habitatsdirectivearticle5" => TaxonAttributesNatura2000HabitatsDirectiveArticle5,
                "taxon.attributes.organismgroup" => TaxonAttributesOrganismGroup,
                "taxon.attributes.parentdyntaxataxonid" => TaxonAttributesParentDyntaxaTaxonId,
                "taxon.attributes.protectedbylaw" => TaxonAttributesProtectedByLaw,
                "taxon.attributes.sensitivitycategory" => TaxonAttributesSensitivityCategory,
                "taxon.attributes.sensitivitycategory.id" => TaxonAttributesSensitivityCategoryId,
                "taxon.attributes.sensitivitycategory.value" => TaxonAttributesSensitivityCategoryValue,
                "taxon.attributes.redlistcategory" => TaxonAttributesRedlistCategory,
                "taxon.attributes.sortorder" => TaxonAttributesSortOrder,
                "taxon.attributes.swedishhistory" => TaxonAttributesSwedishHistory,
                "taxon.attributes.swedishoccurrence" => TaxonAttributesSwedishOccurrence,
                "taxon.attributes.synonyms" => TaxonAttributesSynonyms,
                "taxon.attributes.vernacularnames" => TaxonAttributesVernacularNames,
                "taxon.attributes.taxoncategory" => TaxonAttributesTaxonCategory,
                "taxon.attributes.taxoncategory.id" => TaxonAttributesTaxonCategoryId,
                "taxon.attributes.taxoncategory.value" => TaxonAttributesTaxonCategoryValue,
                "taxon.attributes.isredlisted" => TaxonAttributesIsRedlisted,
                "taxon.attributes.isinvasiveaccordingtoeuregulation" => TaxonAttributesIsInvasiveAccordingToEuRegulation,
                "taxon.attributes.isinvasiveinsweden" => TaxonAttributesIsInvasiveInSweden,
                "taxon.attributes.invasiveriskassessmentcategory" => TaxonAttributesRiskAssessmentCategory,
                "datasetid" => DatasetId,
                "dynamicproperties" => DynamicProperties,
                "institutionid" => InstitutionId,
                "id" => Id,
                "language" => Language,
                "license" => License,
                "privatecollection" => PrivateCollection,
                "publiccollection" => PublicCollection,
                "speciescollectionlabel" => SpeciesCollectionLabel,
                "bibliographiccitation" => BibliographicCitation,
                "datageneralizations" => DataGeneralizations,
                "informationwithheld" => InformationWithheld,
                "references" => References,
                "type" => Type,
                "type.id" => TypeId,
                "type.value" => TypeValue,
                "measurementorfacts" => MeasurementOrFacts,
                "projects" => Projects,
                "geologicalcontext" => "Please specify which geologicalcontext properties you need",
                "geologicalcontext.bed" => GeologicalContextBed,
                "geologicalcontext.earliestageorloweststage" => GeologicalContextEarliestAgeOrLowestStage,
                "geologicalcontext.earliesteonorlowesteonothem" => GeologicalContextEarliestEonOrLowestEonothem,
                "geologicalcontext.earliestepochorlowestseries" => GeologicalContextEarliestEpochOrLowestSeries,
                "geologicalcontext.earliesteraorlowesterathem" => GeologicalContextEarliestEraOrLowestErathem,
                "geologicalcontext.earliestgeochronologicalera" => GeologicalContextEarliestGeochronologicalEra,
                "geologicalcontext.earliestperiodorlowestsystem" => GeologicalContextEarliestPeriodOrLowestSystem,
                "geologicalcontext.formation" => GeologicalContextFormation,
                "geologicalcontext.geologicalcontextid" => GeologicalContextGeologicalContextId,
                "geologicalcontext.group" => GeologicalContextGroup,
                "geologicalcontext.highestbiostratigraphiczone" => GeologicalContextHighestBiostratigraphicZone,
                "geologicalcontext.latestageorhigheststage" => GeologicalContextLatestAgeOrHighestStage,
                "geologicalcontext.latesteonorhighesteonothem" => GeologicalContextLatestEonOrHighestEonothem,
                "geologicalcontext.latestepochorhighestseries" => GeologicalContextLatestEpochOrHighestSeries,
                "geologicalcontext.latesteraorhighesterathem" => GeologicalContextLatestEraOrHighestErathem,
                "geologicalcontext.latestgeochronologicalera" => GeologicalContextLatestGeochronologicalEra,
                "geologicalcontext.latestperiodorhighestsystem" => GeologicalContextLatestPeriodOrHighestSystem,
                "geologicalcontext.lithostratigraphicterms" => GeologicalContextLithostratigraphicTerms,
                "geologicalcontext.lowestbiostratigraphiczone" => GeologicalContextLowestBiostratigraphicZone,
                "geologicalcontext.member" => GeologicalContextMember,
                "materialsample" => MaterialSampleMaterialSampleId,
                "materialsample.materialsampleid" => MaterialSampleMaterialSampleId,
                "organism" => "Please specify which organism properties you need",
                "organism.organismid" => OrganismOrganismId,
                "organism.organismname" => OrganismOrganismName,
                "organism.organismscope" => OrganismOrganismScope,
                "organism.associatedorganisms" => OrganismAssociatedOrganisms,
                "organism.previousidentifications" => OrganismPreviousIdentifications,
                "organism.organismremarks" => OrganismOrganismRemarks,
                _ => throw new ArgumentException($"Field is not mapped: \"{propertyField.PropertyPath}\"")
            };
        }
    }
}