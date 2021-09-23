using System;
using System.Globalization;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// A flat representation of the Observation class.
    /// </summary>
    public class FlatObservation
    {
        private Observation _observation;
        public FlatObservation(Observation observation)
        {
            _observation = observation;
        }

        public string EventStartTimeString
        {
            get
            {
                var date = _observation?.Event?.StartDate;
                if (date == null) return null;
                string format = "HH:mm";

                return date.Value.ToString(format, CultureInfo.InvariantCulture);
            }
        }

        public string EventEndTimeString
        {
            get
            {
                var date = _observation?.Event?.EndDate;
                if (date == null) return null;
                string format = "HH:mm";

                return date.Value.ToString(format, CultureInfo.InvariantCulture);
            }
        }
        public string OccurrenceId => _observation?.Occurrence?.OccurrenceId;
        public string EventDiscoveryMethod => _observation?.Event?.DiscoveryMethod?.ToString();
        public int? EventDiscoveryMethodId => _observation?.Event?.DiscoveryMethod?.Id;
        public string EventDiscoveryMethodValue => _observation?.Event?.DiscoveryMethod?.Value;
        public DateTime? EventStartDate => _observation?.Event?.StartDate;
        public DateTime? EventEndDate => _observation?.Event?.EndDate;
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
        public string EventMedia => _observation?.Event?.Media == null ? null : string.Join(", ", _observation?.Event?.Media);
        public string EventMeasurementOrFacts => _observation?.Event?.MeasurementOrFacts == null ? null : string.Join(", ", _observation?.Event?.MeasurementOrFacts);
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
        public bool? IdentificationValidated => _observation?.Identification?.Validated;
        public string IdentificationValidationStatus => _observation?.Identification?.ValidationStatus?.ToString();
        public int? IdentificationValidationStatusId => _observation?.Identification?.ValidationStatus?.Id;
        public string IdentificationValidationStatusValue => _observation?.Identification?.ValidationStatus?.Value;
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
        public double? LocationCoordinatePrecision => _observation?.Location?.CoordinatePrecision;
        public double? LocationCoordinateUncertaintyInMeters => _observation?.Location?.CoordinateUncertaintyInMeters;
        public string LocationCountry => _observation?.Location?.Country?.ToString();
        public int? LocationCountryId => _observation?.Location?.Country?.Id;
        public string LocationCountryValue => _observation?.Location?.Country?.Value;
        public string LocationCountryCode => _observation?.Location?.CountryCode;
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
        public string LocationHigherGeographyID => _observation?.Location?.HigherGeographyID;
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
        public string OccurrenceIndividualID => _observation?.Occurrence?.IndividualID;
        public bool? OccurrenceIsNaturalOccurrence => _observation?.Occurrence?.IsNaturalOccurrence;
        public bool? OccurrenceIsNeverFoundObservation => _observation?.Occurrence?.IsNeverFoundObservation;
        public bool? OccurrenceIsNotRediscoveredObservation => _observation?.Occurrence?.IsNotRediscoveredObservation;
        public bool? OccurrenceIsPositiveObservation => _observation?.Occurrence?.IsPositiveObservation;
        public string OccurrenceLifeStage => _observation?.Occurrence?.LifeStage?.ToString();
        public int? OccurrenceLifeStageId => _observation?.Occurrence?.LifeStage?.Id;
        public string OccurrenceLifeStageValue => _observation?.Occurrence?.LifeStage?.Value;
        public string OccurrenceMedia => _observation?.Occurrence?.Media == null ? null : string.Join(", ", _observation?.Occurrence?.Media);
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
        public int? OccurrenceProtectionLevel => _observation?.Occurrence?.ProtectionLevel;
        public string OccurrenceRecordedBy => _observation?.Occurrence?.RecordedBy;
        public string OccurrenceRecordNumber => _observation?.Occurrence?.RecordNumber;
        public string OccurrenceReportedBy => _observation?.Occurrence?.ReportedBy;
        public DateTime? OccurrenceReportedDate => _observation?.Occurrence?.ReportedDate;
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
        public string TaxonAcceptedNameUsageID => _observation?.Taxon?.AcceptedNameUsageID;
        public string TaxonAttributesActionPlan => _observation?.Taxon?.Attributes?.ActionPlan;
        public int? TaxonAttributesDisturbanceRadius => _observation?.Taxon?.Attributes?.DisturbanceRadius;
        public int? TaxonAttributesDyntaxaTaxonId => _observation?.Taxon?.Attributes?.DyntaxaTaxonId;
        public bool? TaxonAttributesNatura2000HabitatsDirectiveArticle2 => _observation?.Taxon?.Attributes?.Natura2000HabitatsDirectiveArticle2;
        public bool? TaxonAttributesNatura2000HabitatsDirectiveArticle4 => _observation?.Taxon?.Attributes?.Natura2000HabitatsDirectiveArticle4;
        public bool? TaxonAttributesNatura2000HabitatsDirectiveArticle5 => _observation?.Taxon?.Attributes?.Natura2000HabitatsDirectiveArticle5;
        public string TaxonAttributesOrganismGroup => _observation?.Taxon?.Attributes?.OrganismGroup;
        public int? TaxonAttributesParentDyntaxaTaxonId => _observation?.Taxon?.Attributes?.ParentDyntaxaTaxonId;
        public bool? TaxonAttributesProtectedByLaw => _observation?.Taxon?.Attributes?.ProtectedByLaw;
        public string TaxonAttributesProtectionLevel => _observation?.Taxon?.Attributes?.ProtectionLevel?.ToString();
        public int? TaxonAttributesProtectionLevelId => _observation?.Taxon?.Attributes?.ProtectionLevel?.Id;
        public string TaxonAttributesProtectionLevelValue => _observation?.Taxon?.Attributes?.ProtectionLevel?.Value;
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
        public string TaxonNameAccordingToID => _observation?.Taxon?.NameAccordingToID;
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
        public DateTime? Modified => _observation?.Modified;
        public string OwnerInstitutionCode => _observation?.OwnerInstitutionCode;
        public string PrivateCollection => _observation?.PrivateCollection;
        public bool? Protected => _observation?.Protected;
        public string PublicCollection => _observation?.PublicCollection;
        public string References => _observation?.References;
        public string RightsHolder => _observation?.RightsHolder;
        public string SpeciesCollectionLabel => _observation?.SpeciesCollectionLabel;
        public string Type => _observation?.Type?.ToString();
        public int? TypeId => _observation?.Type?.Id;
        public string TypeValue => _observation?.Type?.Value;
        public string MeasurementOrFacts => _observation?.MeasurementOrFacts == null ? null : string.Join(", ", _observation?.MeasurementOrFacts);
        public string Projects => _observation?.Projects == null ? null : string.Join(", ", _observation?.Projects);

        public string TaxonSecondaryParentDyntaxaTaxonIds => _observation?.Taxon?.SecondaryParentDyntaxaTaxonIds == null
            ? null
            : string.Join(", ", _observation?.Taxon?.SecondaryParentDyntaxaTaxonIds);
        public object GetValue(PropertyFieldDescription propertyField)
        {
            switch (propertyField.Name)
            {
                case "Occurrence.OccurrenceId":
                    return OccurrenceOccurrenceId;
                case "DataProviderId":
                    return DataProviderId;
                case "DatasetName":
                    return DatasetName;
                case "Modified":
                    return Modified;
                case "Protected":
                    return Protected;
                case "AccessRights":
                    return AccessRights;
                case "AccessRights.Id":
                    return AccessRightsId;
                case "AccessRights.Value":
                    return AccessRightsValue;
                case "BasisOfRecord":
                    return BasisOfRecord;
                case "BasisOfRecord.Id":
                    return BasisOfRecordId;
                case "BasisOfRecord.Value":
                    return BasisOfRecordValue;
                case "CollectionCode":
                    return CollectionCode;
                case "CollectionId":
                    return CollectionId;
                case "InstitutionCode":
                    return InstitutionCode;
                case "InstitutionCode.Id":
                    return InstitutionCodeId;
                case "InstitutionCode.Value":
                    return InstitutionCodeValue;
                case "OwnerInstitutionCode":
                    return OwnerInstitutionCode;
                case "RightsHolder":
                    return RightsHolder;
                case "Event.StartDate":
                    return EventStartDate;
                case "Event.EndDate":
                    return EventEndDate;
                case "Event.StartTime":
                    return EventStartTimeString;
                case "Event.EndTime":
                    return EventEndTimeString;
                case "Event.EventId":
                    return EventEventId;
                case "Event.EventRemarks":
                    return EventEventRemarks;
                case "Event.Habitat":
                    return EventHabitat;
                case "Event.DiscoveryMethod":
                    return EventDiscoveryMethod;
                case "Event.DiscoveryMethod.Id":
                    return EventDiscoveryMethodId;
                case "Event.DiscoveryMethod.Value":
                    return EventDiscoveryMethodValue;
                case "Event.SamplingEffort":
                    return EventSamplingEffort;
                case "Event.SamplingProtocol":
                    return EventSamplingProtocol;
                case "Event.SampleSizeUnit":
                    return EventSampleSizeUnit;
                case "Event.SampleSizeValue":
                    return EventSampleSizeValue;
                case "Event.Media":
                    return EventMedia;
                case "Event.MeasurementOrFacts":
                    return EventMeasurementOrFacts;
                case "Event.FieldNotes":
                    return EventFieldNotes;
                case "Event.FieldNumber":
                    return EventFieldNumber;
                case "Event.ParentEventId":
                    return EventParentEventId;
                case "Event.VerbatimEventDate":
                    return EventVerbatimEventDate;
                case "Identification.Validated":
                    return IdentificationValidated;
                case "Identification.ValidationStatus":
                    return IdentificationValidationStatus;
                case "Identification.ValidationStatus.Id":
                    return IdentificationValidationStatusId;
                case "Identification.ValidationStatus.Value":
                    return IdentificationValidationStatusValue;
                case "Identification.ConfirmedBy":
                    return IdentificationConfirmedBy;
                case "Identification.ConfirmedDate":
                    return IdentificationConfirmedDate;
                case "Identification.IdentifiedBy":
                    return IdentificationIdentifiedBy;
                case "Identification.DateIdentified":
                    return IdentificationDateIdentified;
                case "Identification.UncertainIdentification":
                    return IdentificationUncertainIdentification;
                case "Identification.DeterminationMethod":
                    return IdentificationDeterminationMethod;
                case "Identification.DeterminationMethod.Id":
                    return IdentificationDeterminationMethodId;
                case "Identification.DeterminationMethod.Value":
                    return IdentificationDeterminationMethodValue;
                case "Identification.VerifiedBy":
                    return IdentificationVerifiedBy;
                case "Identification.IdentificationQualifier":
                    return IdentificationIdentificationQualifier;
                case "Identification.TypeStatus":
                    return IdentificationTypeStatus;
                case "Identification.IdentificationId":
                    return IdentificationIdentificationId;
                case "Identification.IdentificationReferences":
                    return IdentificationIdentificationReferences;
                case "Identification.IdentificationRemarks":
                    return IdentificationIdentificationRemarks;
                case "Location.DecimalLatitude":
                    return LocationDecimalLatitude;
                case "Location.DecimalLongitude":
                    return LocationDecimalLongitude;
                case "Location.GeodeticDatum":
                    return LocationGeodeticDatum;
                case "Location.CoordinateUncertaintyInMeters":
                    return LocationCoordinateUncertaintyInMeters;
                case "Location.LocationId":
                    return LocationLocationId;
                case "Location.Locality":
                    return LocationLocality;
                case "Location.LocationRemarks":
                    return LocationLocationRemarks;
                case "Location.Municipality":
                    return LocationMunicipality;
                case "Location.Municipality.FeatureId":
                    return LocationMunicipalityFeatureId;
                case "Location.Municipality.Name":
                    return LocationMunicipalityName;
                case "Location.County":
                    return LocationCounty;
                case "Location.County.FeatureId":
                    return LocationCountyFeatureId;
                case "Location.County.Name":
                    return LocationCountyName;
                case "Location.Parish":
                    return LocationParish;
                case "Location.Parish.FeatureId":
                    return LocationParishFeatureId;
                case "Location.Parish.Name":
                    return LocationParishName;
                case "Location.Province":
                    return LocationProvince;
                case "Location.Province.FeatureId":
                    return LocationProvinceFeatureId;
                case "Location.Province.Name":
                    return LocationProvinceName;
                case "Location.Continent":
                    return LocationContinent;
                case "Location.Continent.Id":
                    return LocationContinentId;
                case "Location.Continent.Value":
                    return LocationContinentValue;
                case "Location.Country":
                    return LocationCountry;
                case "Location.Country.Id":
                    return LocationCountryId;
                case "Location.Country.Value":
                    return LocationCountryValue;
                case "Location.CountryCode":
                    return LocationCountryCode;
                case "Location.GeoreferencedBy":
                    return LocationGeoreferencedBy;
                case "Location.GeoreferencedDate":
                    return LocationGeoreferencedDate;
                case "Location.GeoreferenceRemarks":
                    return LocationGeoreferenceRemarks;
                case "Location.HigherGeography":
                    return LocationHigherGeography;
                case "Location.Island":
                    return LocationIsland;
                case "Location.MaximumDepthInMeters":
                    return LocationMaximumDepthInMeters;
                case "Location.MaximumElevationInMeters":
                    return LocationMaximumElevationInMeters;
                case "Location.MinimumDepthInMeters":
                    return LocationMinimumDepthInMeters;
                case "Location.MinimumElevationInMeters":
                    return LocationMinimumElevationInMeters;
                case "Location.VerbatimCoordinateSystem":
                    return LocationVerbatimCoordinateSystem;
                case "Location.VerbatimLatitude":
                    return LocationVerbatimLatitude;
                case "Location.VerbatimLongitude":
                    return LocationVerbatimLongitude;
                case "Location.VerbatimSRS":
                    return LocationVerbatimSRS;
                case "Location.WaterBody":
                    return LocationWaterBody;
                case "Location.CoordinatePrecision":
                    return LocationCoordinatePrecision;
                case "Location.FootprintSpatialFit":
                    return LocationFootprintSpatialFit;
                case "Location.FootprintSRS":
                    return LocationFootprintSRS;
                case "Location.FootprintWKT":
                    return LocationFootprintWKT;
                case "Location.GeoreferenceProtocol":
                    return LocationGeoreferenceProtocol;
                case "Location.GeoreferenceSources":
                    return LocationGeoreferenceSources;
                case "Location.GeoreferenceVerificationStatus":
                    return LocationGeoreferenceVerificationStatus;
                case "Location.HigherGeographyID":
                    return LocationHigherGeographyID;
                case "Location.IslandGroup":
                    return LocationIslandGroup;
                case "Location.LocationAccordingTo":
                    return LocationLocationAccordingTo;
                case "Location.MaximumDistanceAboveSurfaceInMeters":
                    return LocationMaximumDistanceAboveSurfaceInMeters;
                case "Location.MinimumDistanceAboveSurfaceInMeters":
                    return LocationMinimumDistanceAboveSurfaceInMeters;
                case "Location.PointRadiusSpatialFit":
                    return LocationPointRadiusSpatialFit;
                case "Location.VerbatimCoordinates":
                    return LocationVerbatimCoordinates;
                case "Location.VerbatimDepth":
                    return LocationVerbatimDepth;
                case "Location.VerbatimElevation":
                    return LocationVerbatimElevation;
                case "Location.VerbatimLocality":
                    return LocationVerbatimLocality;
                case "Occurrence.RecordedBy":
                    return OccurrenceRecordedBy;
                case "Occurrence.ReportedBy":
                    return OccurrenceReportedBy;
                case "Occurrence.OccurrenceStatus":
                    return OccurrenceOccurrenceStatus;
                case "Occurrence.OccurrenceStatus.Id":
                    return OccurrenceOccurrenceStatusId;
                case "Occurrence.OccurrenceStatus.Value":
                    return OccurrenceOccurrenceStatusValue;
                case "Occurrence.IndividualCount":
                    return OccurrenceIndividualCount;
                case "Occurrence.OrganismQuantity":
                    return OccurrenceOrganismQuantity;
                case "Occurrence.OrganismQuantityInt":
                    return OccurrenceOrganismQuantityInt;
                case "Occurrence.OrganismQuantityUnit":
                    return OccurrenceOrganismQuantityUnit;
                case "Occurrence.OrganismQuantityUnit.Id":
                    return OccurrenceOrganismQuantityUnitId;
                case "Occurrence.OrganismQuantityUnit.Value":
                    return OccurrenceOrganismQuantityUnitValue;
                case "Occurrence.IsNaturalOccurrence":
                    return OccurrenceIsNaturalOccurrence;
                case "Occurrence.IsNeverFoundObservation":
                    return OccurrenceIsNeverFoundObservation;
                case "Occurrence.IsNotRediscoveredObservation":
                    return OccurrenceIsNotRediscoveredObservation;
                case "Occurrence.IsPositiveObservation":
                    return OccurrenceIsPositiveObservation;
                case "Occurrence.OccurrenceRemarks":
                    return OccurrenceOccurrenceRemarks;
                case "Occurrence.ProtectionLevel":
                    return OccurrenceProtectionLevel;
                case "Occurrence.Activity":
                    return OccurrenceActivity;
                case "Occurrence.Activity.Id":
                    return OccurrenceActivityId;
                case "Occurrence.Activity.Value":
                    return OccurrenceActivityValue;
                case "Occurrence.Behavior":
                    return OccurrenceBehavior;
                case "Occurrence.Behavior.Id":
                    return OccurrenceBehaviorId;
                case "Occurrence.Behavior.Value":
                    return OccurrenceBehaviorValue;
                case "Occurrence.Biotope":
                    return OccurrenceBiotope;
                case "Occurrence.Biotope.Id":
                    return OccurrenceBiotopeId;
                case "Occurrence.Biotope.Value":
                    return OccurrenceBiotopeValue;
                case "Occurrence.BiotopeDescription":
                    return OccurrenceBiotopeDescription;
                case "Occurrence.AssociatedMedia":
                    return OccurrenceAssociatedMedia;
                case "Occurrence.LifeStage":
                    return OccurrenceLifeStage;
                case "Occurrence.LifeStage.Id":
                    return OccurrenceLifeStageId;
                case "Occurrence.LifeStage.Value":
                    return OccurrenceLifeStageValue;
                case "Occurrence.ReproductiveCondition":
                    return OccurrenceReproductiveCondition;
                case "Occurrence.ReproductiveCondition.Id":
                    return OccurrenceReproductiveConditionId;
                case "Occurrence.ReproductiveCondition.Value":
                    return OccurrenceReproductiveConditionValue;
                case "Occurrence.Sex":
                    return OccurrenceSex;
                case "Occurrence.Sex.Id":
                    return OccurrenceSexId;
                case "Occurrence.Sex.Value":
                    return OccurrenceSexValue;
                case "Occurrence.Url":
                    return OccurrenceUrl;
                case "Occurrence.Length":
                    return OccurrenceLength;
                case "Occurrence.Weight":
                    return OccurrenceWeight;
                case "Occurrence.Substrate.Description":
                    return OccurrenceSubstrateDescription;
                case "Occurrence.Substrate.Id":
                    return OccurrenceSubstrateId;
                case "Occurrence.Substrate.Name":
                    return OccurrenceSubstrateName;
                case "Occurrence.Substrate.Name.Id":
                    return OccurrenceSubstrateNameId;
                case "Occurrence.Substrate.Name.Value":
                    return OccurrenceSubstrateNameValue;
                case "Occurrence.Substrate.Quantity":
                    return OccurrenceSubstrateQuantity;
                case "Occurrence.Substrate.SpeciesDescription":
                    return OccurrenceSubstrateSpeciesDescription;
                case "Occurrence.Substrate.SpeciesId":
                    return OccurrenceSubstrateSpeciesId;
                case "Occurrence.Substrate.SpeciesScientificName":
                    return OccurrenceSubstrateSpeciesScientificName;
                case "Occurrence.Substrate.SpeciesVernacularName":
                    return OccurrenceSubstrateSpeciesVernacularName;
                case "Occurrence.BirdNestActivityId":
                    return OccurrenceBirdNestActivityId;
                case "Occurrence.CatalogNumber":
                    return OccurrenceCatalogNumber;
                case "Occurrence.CatalogId":
                    return OccurrenceCatalogId;
                case "Occurrence.AssociatedReferences":
                    return OccurrenceAssociatedReferences;
                case "Occurrence.IndividualID":
                    return OccurrenceIndividualID;
                case "Occurrence.Media":
                    return OccurrenceMedia;
                case "Occurrence.Preparations":
                    return OccurrencePreparations;
                case "Occurrence.RecordNumber":
                    return OccurrenceRecordNumber;
                case "Occurrence.ReportedDate":
                    return OccurrenceReportedDate;
                case "Occurrence.AssociatedOccurrences":
                    return OccurrenceAssociatedOccurrences;
                case "Occurrence.AssociatedSequences":
                    return OccurrenceAssociatedSequences;
                case "Occurrence.AssociatedTaxa":
                    return OccurrenceAssociatedTaxa;
                case "Occurrence.Disposition":
                    return OccurrenceDisposition;
                case "Occurrence.EstablishmentMeans":
                    return OccurrenceEstablishmentMeans;
                case "Occurrence.EstablishmentMeans.Id":
                    return OccurrenceEstablishmentMeansId;
                case "Occurrence.EstablishmentMeans.Value":
                    return OccurrenceEstablishmentMeansValue;
                case "Occurrence.OtherCatalogNumbers":
                    return OccurrenceOtherCatalogNumbers;
                case "Taxon.AcceptedNameUsage":
                    return TaxonAcceptedNameUsage;
                case "Taxon.AcceptedNameUsageID":
                    return TaxonAcceptedNameUsageID;
                case "Taxon.BirdDirective":
                    return TaxonBirdDirective;
                case "Taxon.HigherClassification":
                    return TaxonHigherClassification;
                case "Taxon.Id":
                    return TaxonId;
                case "Taxon.ScientificName":
                    return TaxonScientificName;
                case "Taxon.ScientificNameAuthorship":
                    return TaxonScientificNameAuthorship;
                case "Taxon.VernacularName":
                    return TaxonVernacularName;
                case "Taxon.Kingdom":
                    return TaxonKingdom;
                case "Taxon.Phylum":
                    return TaxonPhylum;
                case "Taxon.Class":
                    return TaxonClass;
                case "Taxon.Order":
                    return TaxonOrder;
                case "Taxon.Family":
                    return TaxonFamily;
                case "Taxon.Genus":
                    return TaxonGenus;
                case "Taxon.InfraspecificEpithet":
                    return TaxonInfraspecificEpithet;
                case "Taxon.NameAccordingTo":
                    return TaxonNameAccordingTo;
                case "Taxon.NameAccordingToID":
                    return TaxonNameAccordingToID;
                case "Taxon.NamePublishedIn":
                    return TaxonNamePublishedIn;
                case "Taxon.NamePublishedInId":
                    return TaxonNamePublishedInId;
                case "Taxon.NamePublishedInYear":
                    return TaxonNamePublishedInYear;
                case "Taxon.NomenclaturalCode":
                    return TaxonNomenclaturalCode;
                case "Taxon.NomenclaturalStatus":
                    return TaxonNomenclaturalStatus;
                case "Taxon.OriginalNameUsage":
                    return TaxonOriginalNameUsage;
                case "Taxon.OriginalNameUsageId":
                    return TaxonOriginalNameUsageId;
                case "Taxon.ParentNameUsage":
                    return TaxonParentNameUsage;
                case "Taxon.ParentNameUsageId":
                    return TaxonParentNameUsageId;
                case "Taxon.ScientificNameId":
                    return TaxonScientificNameId;
                case "Taxon.SecondaryParentDyntaxaTaxonIds":
                    return TaxonSecondaryParentDyntaxaTaxonIds;
                case "Taxon.SpecificEpithet":
                    return TaxonSpecificEpithet;
                case "Taxon.Subgenus":
                    return TaxonSubgenus;
                case "Taxon.TaxonConceptId":
                    return TaxonTaxonConceptId;
                case "Taxon.TaxonId":
                    return TaxonTaxonId;
                case "Taxon.TaxonomicStatus":
                    return TaxonTaxonomicStatus;
                case "Taxon.TaxonRank":
                    return TaxonTaxonRank;
                case "Taxon.TaxonRemarks":
                    return TaxonTaxonRemarks;
                case "Taxon.VerbatimTaxonRank":
                    return TaxonVerbatimTaxonRank;
                case "Taxon.Attributes.ActionPlan":
                    return TaxonAttributesActionPlan;
                case "Taxon.Attributes.DisturbanceRadius":
                    return TaxonAttributesDisturbanceRadius;
                case "Taxon.Attributes.DyntaxaTaxonId":
                    return TaxonAttributesDyntaxaTaxonId;
                case "Taxon.Attributes.Natura2000HabitatsDirectiveArticle2":
                    return TaxonAttributesNatura2000HabitatsDirectiveArticle2;
                case "Taxon.Attributes.Natura2000HabitatsDirectiveArticle4":
                    return TaxonAttributesNatura2000HabitatsDirectiveArticle4;
                case "Taxon.Attributes.Natura2000HabitatsDirectiveArticle5":
                    return TaxonAttributesNatura2000HabitatsDirectiveArticle5;
                case "Taxon.Attributes.OrganismGroup":
                    return TaxonAttributesOrganismGroup;
                case "Taxon.Attributes.ParentDyntaxaTaxonId":
                    return TaxonAttributesParentDyntaxaTaxonId;
                case "Taxon.Attributes.ProtectedByLaw":
                    return TaxonAttributesProtectedByLaw;
                case "Taxon.Attributes.ProtectionLevel":
                    return TaxonAttributesProtectionLevel;
                case "Taxon.Attributes.ProtectionLevel.Id":
                    return TaxonAttributesProtectionLevelId;
                case "Taxon.Attributes.ProtectionLevel.Value":
                    return TaxonAttributesProtectionLevelValue;
                case "Taxon.Attributes.RedlistCategory":
                    return TaxonAttributesRedlistCategory;
                case "Taxon.Attributes.SortOrder":
                    return TaxonAttributesSortOrder;
                case "Taxon.Attributes.SwedishHistory":
                    return TaxonAttributesSwedishHistory;
                case "Taxon.Attributes.SwedishOccurrence":
                    return TaxonAttributesSwedishOccurrence;
                case "Taxon.Attributes.Synonyms":
                    return TaxonAttributesSynonyms;
                case "Taxon.Attributes.VernacularNames":
                    return TaxonAttributesVernacularNames;
                case "DatasetId":
                    return DatasetId;
                case "DynamicProperties":
                    return DynamicProperties;
                case "InstitutionId":
                    return InstitutionId;
                case "Id":
                    return Id;
                case "Language":
                    return Language;
                case "License":
                    return License;
                case "PrivateCollection":
                    return PrivateCollection;
                case "PublicCollection":
                    return PublicCollection;
                case "SpeciesCollectionLabel":
                    return SpeciesCollectionLabel;
                case "BibliographicCitation":
                    return BibliographicCitation;
                case "DataGeneralizations":
                    return DataGeneralizations;
                case "InformationWithheld":
                    return InformationWithheld;
                case "References":
                    return References;
                case "Type":
                    return Type;
                case "Type.Id":
                    return TypeId;
                case "Type.Value":
                    return TypeValue;
                case "MeasurementOrFacts":
                    return MeasurementOrFacts;
                case "Projects":
                    return Projects;
                case "GeologicalContext.Bed":
                    return GeologicalContextBed;
                case "GeologicalContext.EarliestAgeOrLowestStage":
                    return GeologicalContextEarliestAgeOrLowestStage;
                case "GeologicalContext.EarliestEonOrLowestEonothem":
                    return GeologicalContextEarliestEonOrLowestEonothem;
                case "GeologicalContext.EarliestEpochOrLowestSeries":
                    return GeologicalContextEarliestEpochOrLowestSeries;
                case "GeologicalContext.EarliestEraOrLowestErathem":
                    return GeologicalContextEarliestEraOrLowestErathem;
                case "GeologicalContext.EarliestGeochronologicalEra":
                    return GeologicalContextEarliestGeochronologicalEra;
                case "GeologicalContext.EarliestPeriodOrLowestSystem":
                    return GeologicalContextEarliestPeriodOrLowestSystem;
                case "GeologicalContext.Formation":
                    return GeologicalContextFormation;
                case "GeologicalContext.GeologicalContextId":
                    return GeologicalContextGeologicalContextId;
                case "GeologicalContext.Group":
                    return GeologicalContextGroup;
                case "GeologicalContext.HighestBiostratigraphicZone":
                    return GeologicalContextHighestBiostratigraphicZone;
                case "GeologicalContext.LatestAgeOrHighestStage":
                    return GeologicalContextLatestAgeOrHighestStage;
                case "GeologicalContext.LatestEonOrHighestEonothem":
                    return GeologicalContextLatestEonOrHighestEonothem;
                case "GeologicalContext.LatestEpochOrHighestSeries":
                    return GeologicalContextLatestEpochOrHighestSeries;
                case "GeologicalContext.LatestEraOrHighestErathem":
                    return GeologicalContextLatestEraOrHighestErathem;
                case "GeologicalContext.LatestGeochronologicalEra":
                    return GeologicalContextLatestGeochronologicalEra;
                case "GeologicalContext.LatestPeriodOrHighestSystem":
                    return GeologicalContextLatestPeriodOrHighestSystem;
                case "GeologicalContext.LithostratigraphicTerms":
                    return GeologicalContextLithostratigraphicTerms;
                case "GeologicalContext.LowestBiostratigraphicZone":
                    return GeologicalContextLowestBiostratigraphicZone;
                case "GeologicalContext.Member":
                    return GeologicalContextMember;
                case "MaterialSample.MaterialSampleId":
                    return MaterialSampleMaterialSampleId;
                case "Organism.OrganismId":
                    return OrganismOrganismId;
                case "Organism.OrganismName":
                    return OrganismOrganismName;
                case "Organism.OrganismScope":
                    return OrganismOrganismScope;
                case "Organism.AssociatedOrganisms":
                    return OrganismAssociatedOrganisms;
                case "Organism.PreviousIdentifications":
                    return OrganismPreviousIdentifications;
                case "Organism.OrganismRemarks":
                    return OrganismOrganismRemarks;

                default:
                    throw new ArgumentException($"Field is not mapped: \"{propertyField.Name}\"");
            }
        }
    }
}