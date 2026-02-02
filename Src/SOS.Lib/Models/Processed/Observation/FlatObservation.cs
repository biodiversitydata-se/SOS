using System.Globalization;
using System.Text.RegularExpressions;

namespace SOS.Lib.Models.Processed.Observation;

/// <summary>
/// A flat representation of the Observation class.
/// </summary>
public class FlatObservation
{
    public FlatObservation(Observation observation)
    {
        _observation = observation;
    }

    private Observation _observation;
    private static TimeZoneInfo swedenTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
    public double? LocationDecimalLatitude => _observation?.Location?.DecimalLatitude;
    public double? LocationDecimalLongitude => _observation?.Location?.DecimalLongitude;
    public string OccurrenceId => _observation?.Occurrence?.OccurrenceId;
    
    private static readonly Dictionary<string, Func<FlatObservation, object>> _propertyGetters = new(StringComparer.OrdinalIgnoreCase)
    {
        // Event
        ["event"] = x => "Please specify which event properties you need",
        ["event.plainstarttime"] = x => x._observation?.Event?.PlainStartTime,
        ["event.plainendtime"] = x => x._observation?.Event?.PlainEndTime,
        ["event.discoverymethod"] = x => x._observation?.Event?.DiscoveryMethod?.ToString(),
        ["event.discoverymethod.id"] = x => x._observation?.Event?.DiscoveryMethod?.Id,
        ["event.discoverymethod.value"] = x => x._observation?.Event?.DiscoveryMethod?.Value,
        ["event.startdate"] = x => {
            var date = x._observation?.Event?.StartDate;
            if (date == null) return null;
            if (date.Value.Kind == DateTimeKind.Utc)
                return TimeZoneInfo.ConvertTimeFromUtc(date.Value, swedenTimeZone);
            return date;
        },
        ["event.enddate"] = x => {
            var date = x._observation?.Event?.EndDate;
            if (date == null) return null;
            if (date.Value.Kind == DateTimeKind.Utc)
                return TimeZoneInfo.ConvertTimeFromUtc(date.Value, swedenTimeZone);
            return date;
        },
        ["event.plainstartdate"] = x => x._observation?.Event?.PlainStartDate,
        ["event.plainenddate"] = x => x._observation?.Event?.PlainEndDate,
        ["event.eventid"] = x => x._observation?.Event?.EventId,
        ["event.eventremarks"] = x => x._observation?.Event?.EventRemarks,
        ["event.fieldnotes"] = x => x._observation?.Event?.FieldNotes,
        ["event.fieldnumber"] = x => x._observation?.Event?.FieldNumber,
        ["event.habitat"] = x => x._observation?.Event?.Habitat,
        ["event.parenteventid"] = x => x._observation?.Event?.ParentEventId,
        ["event.samplingeffort"] = x => x._observation?.Event?.SamplingEffort,
        ["event.samplingprotocol"] = x => x._observation?.Event?.SamplingProtocol,
        ["event.samplesizeunit"] = x => x._observation?.Event?.SampleSizeUnit,
        ["event.samplesizevalue"] = x => x._observation?.Event?.SampleSizeValue,
        ["event.verbatimeventdate"] = x => x._observation?.Event?.VerbatimEventDate,
        ["event.media"] = x => x._observation?.Event?.Media == null ? null : string.Join(", ", x._observation?.Event?.Media.Select(m => m.ToString())),
        ["event.measurementorfacts"] = x => x._observation?.Event?.MeasurementOrFacts == null ? null : string.Join(", ", x._observation?.Event?.MeasurementOrFacts.Select(m => m.ToString())),
        ["event.starthistogramweek"] = x => x._observation?.Event?.StartHistogramWeek,
        ["event.endhistogramweek"] = x => x._observation?.Event?.EndHistogramWeek,
        ["event.startyear"] = x => x._observation?.Event?.StartYear,
        ["event.startmonth"] = x => x._observation?.Event?.StartMonth,
        ["event.startday"] = x => x._observation?.Event?.StartDay,
        ["event.endyear"] = x => x._observation?.Event?.EndYear,
        ["event.endmonth"] = x => x._observation?.Event?.EndMonth,
        ["event.endday"] = x => x._observation?.Event?.EndDay,

        // Occurrence
        ["occurrence"] = x => "Please specify which occurrence properties you need",
        ["occurrence.occurrenceid"] = x => x._observation?.Occurrence?.OccurrenceId,
        ["occurrence.activity"] = x => x._observation?.Occurrence?.Activity?.ToString(),
        ["occurrence.activity.id"] = x => x._observation?.Occurrence?.Activity?.Id,
        ["occurrence.activity.value"] = x => x._observation?.Occurrence?.Activity?.Value,
        ["occurrence.associatedmedia"] = x => x._observation?.Occurrence?.AssociatedMedia,
        ["occurrence.associatedoccurrences"] = x => x._observation?.Occurrence?.AssociatedOccurrences,
        ["occurrence.associatedreferences"] = x => x._observation?.Occurrence?.AssociatedReferences,
        ["occurrence.associatedsequences"] = x => x._observation?.Occurrence?.AssociatedSequences,
        ["occurrence.associatedtaxa"] = x => x._observation?.Occurrence?.AssociatedTaxa,
        ["occurrence.behavior"] = x => x._observation?.Occurrence?.Behavior?.ToString(),
        ["occurrence.behavior.id"] = x => x._observation?.Occurrence?.Behavior?.Id,
        ["occurrence.behavior.value"] = x => x._observation?.Occurrence?.Behavior?.Value,
        ["occurrence.biotope"] = x => x._observation?.Occurrence?.Biotope?.ToString(),
        ["occurrence.biotope.id"] = x => x._observation?.Occurrence?.Biotope?.Id,
        ["occurrence.biotope.value"] = x => x._observation?.Occurrence?.Biotope?.Value,
        ["occurrence.biotopedescription"] = x => x._observation?.Occurrence?.BiotopeDescription,
        ["occurrence.birdnestactivityid"] = x => x._observation?.Occurrence?.BirdNestActivityId,
        ["occurrence.catalognumber"] = x => x._observation?.Occurrence?.CatalogNumber,
        ["occurrence.catalogid"] = x => x._observation?.Occurrence?.CatalogId,
        ["occurrence.disposition"] = x => x._observation?.Occurrence?.Disposition,
        ["occurrence.establishmentmeans"] = x => x._observation?.Occurrence?.EstablishmentMeans?.ToString(),
        ["occurrence.establishmentmeans.id"] = x => x._observation?.Occurrence?.EstablishmentMeans?.Id,
        ["occurrence.establishmentmeans.value"] = x => x._observation?.Occurrence?.EstablishmentMeans?.Value,
        ["occurrence.individualcount"] = x => x._observation?.Occurrence?.IndividualCount,
        ["occurrence.isnaturaloccurrence"] = x => x._observation?.Occurrence?.IsNaturalOccurrence,
        ["occurrence.isneverfoundobservation"] = x => x._observation?.Occurrence?.IsNeverFoundObservation,
        ["occurrence.isnotrediscoveredobservation"] = x => x._observation?.Occurrence?.IsNotRediscoveredObservation,
        ["occurrence.ispositiveobservation"] = x => x._observation?.Occurrence?.IsPositiveObservation,
        ["occurrence.lifestage"] = x => x._observation?.Occurrence?.LifeStage?.ToString(),
        ["occurrence.lifestage.id"] = x => x._observation?.Occurrence?.LifeStage?.Id,
        ["occurrence.lifestage.value"] = x => x._observation?.Occurrence?.LifeStage?.Value,
        ["occurrence.media"] = x => x._observation?.Occurrence?.Media == null ? null : string.Join(", ", x._observation?.Occurrence?.Media.Select(m => m.ToString())),
        ["occurrence.occurrenceremarks"] = x => x._observation?.Occurrence?.OccurrenceRemarks,
        ["occurrence.occurrencestatus"] = x => x._observation?.Occurrence?.OccurrenceStatus?.ToString(),
        ["occurrence.occurrencestatus.id"] = x => x._observation?.Occurrence?.OccurrenceStatus?.Id,
        ["occurrence.occurrencestatus.value"] = x => x._observation?.Occurrence?.OccurrenceStatus?.Value,
        ["occurrence.organismquantity"] = x => x._observation?.Occurrence?.OrganismQuantity,
        ["occurrence.organismquantityint"] = x => x._observation?.Occurrence?.OrganismQuantityInt,
        ["occurrence.organismquantityunit"] = x => x._observation?.Occurrence?.OrganismQuantityUnit?.ToString(),
        ["occurrence.organismquantityunit.id"] = x => x._observation?.Occurrence?.OrganismQuantityUnit?.Id,
        ["occurrence.organismquantityunit.value"] = x => x._observation?.Occurrence?.OrganismQuantityUnit?.Value,
        ["occurrence.preparations"] = x => x._observation?.Occurrence?.Preparations,
        ["occurrence.sensitivitycategory"] = x => x._observation?.Occurrence?.SensitivityCategory,
        ["occurrence.recordedby"] = x => x._observation?.Occurrence?.RecordedBy,
        ["occurrence.recordnumber"] = x => x._observation?.Occurrence?.RecordNumber,
        ["occurrence.reportedby"] = x => x._observation?.Occurrence?.ReportedBy,
        ["occurrence.reporteddate"] = x => {
            var date = x._observation?.Occurrence?.ReportedDate;
            if (date == null) return null;
            if (date.Value.Kind == DateTimeKind.Utc)
                return TimeZoneInfo.ConvertTimeFromUtc(date.Value, swedenTimeZone);
            return date;
        },
        ["occurrence.reproductivecondition"] = x => x._observation?.Occurrence?.ReproductiveCondition?.ToString(),
        ["occurrence.reproductivecondition.id"] = x => x._observation?.Occurrence?.ReproductiveCondition?.Id,
        ["occurrence.reproductivecondition.value"] = x => x._observation?.Occurrence?.ReproductiveCondition?.Value,
        ["occurrence.sex"] = x => x._observation?.Occurrence?.Sex?.ToString(),
        ["occurrence.sex.id"] = x => x._observation?.Occurrence?.Sex?.Id,
        ["occurrence.sex.value"] = x => x._observation?.Occurrence?.Sex?.Value,
        ["occurrence.substrate.description"] = x => x._observation?.Occurrence?.Substrate?.Description,
        ["occurrence.substrate.id"] = x => x._observation?.Occurrence?.Substrate?.Id,
        ["occurrence.substrate.name"] = x => x._observation?.Occurrence?.Substrate?.Name?.ToString(),
        ["occurrence.substrate.name.id"] = x => x._observation?.Occurrence?.Substrate?.Name?.Id,
        ["occurrence.substrate.name.value"] = x => x._observation?.Occurrence?.Substrate?.Name?.Value,
        ["occurrence.substrate.quantity"] = x => x._observation?.Occurrence?.Substrate?.Quantity,
        ["occurrence.substrate.speciesdescription"] = x => x._observation?.Occurrence?.Substrate?.SpeciesDescription,
        ["occurrence.substrate.speciesid"] = x => x._observation?.Occurrence?.Substrate?.SpeciesId,
        ["occurrence.substrate.speciesscientificname"] = x => x._observation?.Occurrence?.Substrate?.SpeciesScientificName,
        ["occurrence.substrate.speciesvernacularname"] = x => x._observation?.Occurrence?.Substrate?.SpeciesVernacularName,
        ["occurrence.url"] = x => x._observation?.Occurrence?.Url,
        ["occurrence.length"] = x => x._observation?.Occurrence?.Length,
        ["occurrence.weight"] = x => x._observation?.Occurrence?.Weight,
        ["occurrence.othercatalognumbers"] = x => x._observation?.Occurrence?.OtherCatalogNumbers,

        // Location
        ["location"] = x => "Please specify which location properties you need",
        ["location.decimallatitude"] = x => x._observation?.Location?.DecimalLatitude,
        ["location.decimallongitude"] = x => x._observation?.Location?.DecimalLongitude,
        ["location.sweref99tmx"] = x => x._observation?.Location?.Sweref99TmX,
        ["location.sweref99tmy"] = x => x._observation?.Location?.Sweref99TmY,        
        ["location.geodeticdatum"] = x => x._observation?.Location?.GeodeticDatum,
        ["location.coordinateuncertaintyinmeters"] = x => x._observation?.Location?.CoordinateUncertaintyInMeters,
        ["location.locationid"] = x => x._observation?.Location?.LocationId,
        ["location.locality"] = x => x._observation?.Location?.Locality,
        ["location.locationremarks"] = x => x._observation?.Location?.LocationRemarks,
        ["location.municipality"] = x => x._observation?.Location?.Municipality?.ToString(),
        ["location.municipality.featureid"] = x => x._observation?.Location?.Municipality?.FeatureId,
        ["location.municipality.name"] = x => x._observation?.Location?.Municipality?.Name,
        ["location.county"] = x => x._observation?.Location?.County?.ToString(),
        ["location.county.featureid"] = x => x._observation?.Location?.County?.FeatureId,
        ["location.county.name"] = x => x._observation?.Location?.County?.Name,
        ["location.province"] = x => x._observation?.Location?.Province?.ToString(),
        ["location.province.featureid"] = x => x._observation?.Location?.Province?.FeatureId,
        ["location.province.name"] = x => x._observation?.Location?.Province?.Name,
        ["location.country"] = x => x._observation?.Location?.Country?.ToString(),
        ["location.country.id"] = x => x._observation?.Location?.Country?.Id,
        ["location.country.value"] = x => x._observation?.Location?.Country?.Value,
        ["location.countrycode"] = x => x._observation?.Location?.CountryCode,
        ["location.georeferencedby"] = x => x._observation?.Location?.GeoreferencedBy,
        ["location.georeferenceddate"] = x => x._observation?.Location?.GeoreferencedDate,
        ["location.georeferenceremarks"] = x => x._observation?.Location?.GeoreferenceRemarks,
        ["location.highergeography"] = x => x._observation?.Location?.HigherGeography,
        ["location.island"] = x => x._observation?.Location?.Island,
        ["location.maximumdepthinmeters"] = x => x._observation?.Location?.MaximumDepthInMeters,
        ["location.maximumelevationinmeters"] = x => x._observation?.Location?.MaximumElevationInMeters,
        ["location.minimumdepthinmeters"] = x => x._observation?.Location?.MinimumDepthInMeters,
        ["location.minimumelevationinmeters"] = x => x._observation?.Location?.MinimumElevationInMeters,
        ["location.verbatimcoordinatesystem"] = x => x._observation?.Location?.VerbatimCoordinateSystem,
        ["location.verbatimlatitude"] = x => x._observation?.Location?.VerbatimLatitude,
        ["location.verbatimlongitude"] = x => x._observation?.Location?.VerbatimLongitude,
        ["location.verbatimsrs"] = x => x._observation?.Location?.VerbatimSRS,
        ["location.waterbody"] = x => x._observation?.Location?.WaterBody,
        ["location.coordinateprecision"] = x => x._observation?.Location?.CoordinatePrecision,
        ["location.footprintspatialfit"] = x => x._observation?.Location?.FootprintSpatialFit,
        ["location.footprintsrs"] = x => x._observation?.Location?.FootprintSRS,
        ["location.footprintwkt"] = x => x._observation?.Location?.FootprintWKT,
        ["location.georeferenceprotocol"] = x => x._observation?.Location?.GeoreferenceProtocol,
        ["location.georeferencesources"] = x => x._observation?.Location?.GeoreferenceSources,
        ["location.georeferenceverificationstatus"] = x => x._observation?.Location?.GeoreferenceVerificationStatus,
        ["location.highergeographyid"] = x => x._observation?.Location?.HigherGeographyId,
        ["location.islandgroup"] = x => x._observation?.Location?.IslandGroup,
        ["location.locationaccordingto"] = x => x._observation?.Location?.LocationAccordingTo,
        ["location.maximumdistanceabovesurfaceinmeters"] = x => x._observation?.Location?.MaximumDistanceAboveSurfaceInMeters,
        ["location.minimumdistanceabovesurfaceinmeters"] = x => x._observation?.Location?.MinimumDistanceAboveSurfaceInMeters,
        ["location.pointradiusspatialfit"] = x => x._observation?.Location?.PointRadiusSpatialFit,
        ["location.verbatimcoordinates"] = x => x._observation?.Location?.VerbatimCoordinates,
        ["location.verbatimdepth"] = x => x._observation?.Location?.VerbatimDepth,
        ["location.verbatimelevation"] = x => x._observation?.Location?.VerbatimElevation,
        ["location.verbatimlocality"] = x => x._observation?.Location?.VerbatimLocality,
        ["location.attributes.projectid"] = x => x._observation?.Location?.Attributes?.ProjectId,
        ["location.type"] = x => x._observation?.Location?.Type.ToString(),
        ["location.attributes.externalid"] = x => x._observation?.Location?.Attributes?.ExternalId,
        ["location.parish"] = x => x._observation?.Location?.Parish?.ToString(),
        ["location.parish.featureid"] = x => x._observation?.Location?.Parish?.FeatureId,
        ["location.parish.name"] = x => x._observation?.Location?.Parish?.Name,
        ["location.continent"] = x => x._observation?.Location?.Continent?.ToString(),
        ["location.continent.id"] = x => x._observation?.Location?.Continent?.Id,
        ["location.continent.value"] = x => x._observation?.Location?.Continent?.Value,

        // Taxon
        ["taxon"] = x => "Please specify which taxon properties you need",
        ["taxon.acceptednameusage"] = x => x._observation?.Taxon?.AcceptedNameUsage,
        ["taxon.acceptednameusageid"] = x => x._observation?.Taxon?.AcceptedNameUsageId,
        ["taxon.birddirective"] = x => x._observation?.Taxon?.BirdDirective,
        ["taxon.birddirectiveannex1"] = x => x._observation?.Taxon?.BirdDirectiveAnnex1,
        ["taxon.higherclassification"] = x => x._observation?.Taxon?.HigherClassification,
        ["taxon.id"] = x => x._observation?.Taxon?.Id,
        ["taxon.scientificname"] = x => x._observation?.Taxon?.ScientificName,
        ["taxon.scientificnameauthorship"] = x => x._observation?.Taxon?.ScientificNameAuthorship,
        ["taxon.vernacularname"] = x => x._observation?.Taxon?.VernacularName,
        ["taxon.kingdom"] = x => x._observation?.Taxon?.Kingdom,
        ["taxon.phylum"] = x => x._observation?.Taxon?.Phylum,
        ["taxon.class"] = x => x._observation?.Taxon?.Class,
        ["taxon.order"] = x => x._observation?.Taxon?.Order,
        ["taxon.family"] = x => x._observation?.Taxon?.Family,
        ["taxon.genus"] = x => x._observation?.Taxon?.Genus,
        ["taxon.infraspecificepithet"] = x => x._observation?.Taxon?.InfraspecificEpithet,
        ["taxon.nameaccordingto"] = x => x._observation?.Taxon?.NameAccordingTo,
        ["taxon.nameaccordingtoid"] = x => x._observation?.Taxon?.NameAccordingToId,
        ["taxon.namepublishedin"] = x => x._observation?.Taxon?.NamePublishedIn,
        ["taxon.namepublishedinid"] = x => x._observation?.Taxon?.NamePublishedInId,
        ["taxon.namepublishedinyear"] = x => x._observation?.Taxon?.NamePublishedInYear,
        ["taxon.nomenclaturalcode"] = x => x._observation?.Taxon?.NomenclaturalCode,
        ["taxon.nomenclaturalstatus"] = x => x._observation?.Taxon?.NomenclaturalStatus,
        ["taxon.originalnameusage"] = x => x._observation?.Taxon?.OriginalNameUsage,
        ["taxon.originalnameusageid"] = x => x._observation?.Taxon?.OriginalNameUsageId,
        ["taxon.parentnameusage"] = x => x._observation?.Taxon?.ParentNameUsage,
        ["taxon.parentnameusageid"] = x => x._observation?.Taxon?.ParentNameUsageId,
        ["taxon.scientificnameid"] = x => x._observation?.Taxon?.ScientificNameId,
        ["taxon.secondaryparentdyntaxataxonids"] = x => x._observation?.Taxon?.SecondaryParentDyntaxaTaxonIds == null ? null : string.Join(", ", x._observation?.Taxon?.SecondaryParentDyntaxaTaxonIds),
        ["taxon.specificepithet"] = x => x._observation?.Taxon?.SpecificEpithet,
        ["taxon.subgenus"] = x => x._observation?.Taxon?.Subgenus,
        ["taxon.taxonconceptid"] = x => x._observation?.Taxon?.TaxonConceptId,
        ["taxon.taxonid"] = x => x._observation?.Taxon?.TaxonId,
        ["taxon.taxonomicstatus"] = x => x._observation?.Taxon?.TaxonomicStatus,
        ["taxon.taxonrank"] = x => x._observation?.Taxon?.TaxonRank,
        ["taxon.taxonremarks"] = x => x._observation?.Taxon?.TaxonRemarks,
        ["taxon.verbatimtaxonrank"] = x => x._observation?.Taxon?.VerbatimTaxonRank,

        // Taxon.Attributes
        ["taxon.attributes"] = x => "Please specify which taxon.attributes properties you need",
        ["taxon.attributes.actionplan"] = x => x._observation?.Taxon?.Attributes?.ActionPlan,
        ["taxon.attributes.disturbanceradius"] = x => x._observation?.Taxon?.Attributes?.DisturbanceRadius,
        ["taxon.attributes.dyntaxataxonid"] = x => x._observation?.Taxon?.Attributes?.DyntaxaTaxonId,
        ["taxon.attributes.natura2000habitatsdirectivearticle2"] = x => x._observation?.Taxon?.Attributes?.Natura2000HabitatsDirectiveArticle2,
        ["taxon.attributes.natura2000habitatsdirectivearticle4"] = x => x._observation?.Taxon?.Attributes?.Natura2000HabitatsDirectiveArticle4,
        ["taxon.attributes.natura2000habitatsdirectivearticle5"] = x => x._observation?.Taxon?.Attributes?.Natura2000HabitatsDirectiveArticle5,
        ["taxon.attributes.organismgroup"] = x => x._observation?.Taxon?.Attributes?.OrganismGroup,
        ["taxon.attributes.parentdyntaxataxonid"] = x => x._observation?.Taxon?.Attributes?.ParentDyntaxaTaxonId,
        ["taxon.attributes.protectedbylaw"] = x => x._observation?.Taxon?.Attributes?.ProtectedByLaw,
        ["taxon.attributes.sensitivitycategory"] = x => x._observation?.Taxon?.Attributes?.SensitivityCategory?.ToString(),
        ["taxon.attributes.sensitivitycategory.id"] = x => x._observation?.Taxon?.Attributes?.SensitivityCategory?.Id,
        ["taxon.attributes.sensitivitycategory.value"] = x => x._observation?.Taxon?.Attributes?.SensitivityCategory?.Value,
        ["taxon.attributes.redlistcategory"] = x => x._observation?.Taxon?.Attributes?.RedlistCategory,
        ["taxon.attributes.redlistcategoryderived"] = x => x._observation?.Taxon?.Attributes?.RedlistCategoryDerived,
        ["taxon.attributes.sortorder"] = x => x._observation?.Taxon?.Attributes?.SortOrder,
        ["taxon.attributes.swedishhistory"] = x => x._observation?.Taxon?.Attributes?.SwedishHistory,
        ["taxon.attributes.swedishoccurrence"] = x => x._observation?.Taxon?.Attributes?.SwedishOccurrence,
        ["taxon.attributes.synonyms"] = x => x._observation?.Taxon?.Attributes?.Synonyms == null ? null : string.Join(", ", x._observation?.Taxon?.Attributes?.Synonyms),
        ["taxon.attributes.vernacularnames"] = x => x._observation?.Taxon?.Attributes?.VernacularNames == null ? null : string.Join(", ", x._observation?.Taxon?.Attributes?.VernacularNames),
        ["taxon.attributes.taxoncategory"] = x => x._observation?.Taxon?.Attributes?.TaxonCategory?.ToString(),
        ["taxon.attributes.taxoncategory.id"] = x => x._observation?.Taxon?.Attributes?.TaxonCategory?.Id,
        ["taxon.attributes.taxoncategory.value"] = x => x._observation?.Taxon?.Attributes?.TaxonCategory?.Value,
        ["taxon.attributes.isredlisted"] = x => x._observation?.Taxon?.Attributes?.IsRedlisted,
        ["taxon.attributes.isinvasiveaccordingtoeuregulation"] = x => x._observation?.Taxon?.Attributes?.IsInvasiveAccordingToEuRegulation,
        ["taxon.attributes.isinvasiveinsweden"] = x => x._observation?.Taxon?.Attributes?.IsInvasiveInSweden,
        ["taxon.attributes.invasiveriskassessmentcategory"] = x => x._observation?.Taxon?.Attributes?.InvasiveRiskAssessmentCategory,
        ["taxon.attributes.gbiftaxonid"] = x => x._observation?.Taxon?.Attributes?.GbifTaxonId,        
        
        // Record level
        ["modified"] = x => {
            var date = x._observation?.Modified;
            if (date == null) return null;
            if (date.Value.Kind == DateTimeKind.Utc)
                return TimeZoneInfo.ConvertTimeFromUtc(date.Value, swedenTimeZone);
            return date;
        },
        ["sensitive"] = x => x._observation?.Sensitive,
        ["accessrights"] = x => x._observation?.AccessRights?.ToString(),
        ["accessrights.id"] = x => x._observation?.AccessRights?.Id,
        ["accessrights.value"] = x => x._observation?.AccessRights?.Value,
        ["isgeneralized"] = x => x._observation?.IsGeneralized,
        ["basisofrecord"] = x => x._observation?.BasisOfRecord?.ToString(),
        ["basisofrecord.id"] = x => x._observation?.BasisOfRecord?.Id,
        ["basisofrecord.value"] = x => x._observation?.BasisOfRecord?.Value,
        ["collectioncode"] = x => x._observation?.CollectionCode,
        ["collectionid"] = x => x._observation?.CollectionId,
        ["institutioncode"] = x => x._observation?.InstitutionCode?.ToString(),
        ["institutioncode.id"] = x => x._observation?.InstitutionCode?.Id,
        ["institutioncode.value"] = x => x._observation?.InstitutionCode?.Value,
        ["ownerinstitutioncode"] = x => x._observation?.OwnerInstitutionCode,
        ["rightsholder"] = x => x._observation?.RightsHolder,
        ["bibliographiccitation"] = x => x._observation?.BibliographicCitation,
        ["datageneralizations"] = x => x._observation?.DataGeneralizations,
        ["datasetid"] = x => x._observation?.DatasetId,
        ["dynamicproperties"] = x => x._observation?.DynamicProperties,
        ["id"] = x => x._observation?.Id,
        ["informationwithheld"] = x => x._observation?.InformationWithheld,
        ["institutionid"] = x => x._observation?.InstitutionId,
        ["language"] = x => x._observation?.Language,
        ["license"] = x => x._observation?.License,
        ["privatecollection"] = x => x._observation?.PrivateCollection,
        ["publiccollection"] = x => x._observation?.PublicCollection,
        ["references"] = x => x._observation?.References,
        ["speciescollectionlabel"] = x => x._observation?.SpeciesCollectionLabel,
        ["type"] = x => x._observation?.Type?.ToString(),
        ["measurementorfacts"] = x => x._observation?.MeasurementOrFacts == null ? null : string.Join(", ", x._observation.MeasurementOrFacts.Select(m => m.ToString())),
        ["projects"] = x => x._observation?.Projects == null ? null : string.Join(", ", x._observation.Projects.Select(m => m.ToString())),        
        ["projectssummary.project1id"] = x => x._observation?.ProjectsSummary?.Project1Id,
        ["projectssummary.project1name"] = x => x._observation?.ProjectsSummary?.Project1Name,
        ["projectssummary.project1category"] = x => x._observation?.ProjectsSummary?.Project1Category,
        ["projectssummary.project1url"] = x => x._observation?.ProjectsSummary?.Project1Url,
        ["projectssummary.project1values"] = x => x._observation?.ProjectsSummary?.Project1Values,
        ["projectssummary.project2id"] = x => x._observation?.ProjectsSummary?.Project2Id,
        ["projectssummary.project2name"] = x => x._observation?.ProjectsSummary?.Project2Name,
        ["projectssummary.project2category"] = x => x._observation?.ProjectsSummary?.Project2Category,
        ["projectssummary.project2url"] = x => x._observation?.ProjectsSummary?.Project2Url,
        ["projectssummary.project2values"] = x => x._observation?.ProjectsSummary?.Project2Values,
        ["created"] = x => x._observation?.Created,
        ["datastewardship.datasetidentifier"] = x => x._observation?.DataStewardship?.DatasetIdentifier,
        ["datastewardship.datasettitle"] = x => x._observation?.DataStewardship?.DatasetTitle,
        ["geologicalcontext"] = x => x._observation?.GeologicalContext,
        ["artportaleninternal"] = x => x._observation?.ArtportalenInternal,
        ["projects.projectssummary"] = x => x._observation?.ProjectsSummary,
        ["dataquality"] = x => x._observation?.DataQuality,
        ["mongodbid"] = x => x._observation?.MongoDbId,
        ["diffusionstatus"] = x => x._observation?.DiffusionStatus,
        ["dataproviderid"] = x => x._observation?.DataProviderId,
        ["datasetname"] = x => x._observation?.DatasetName,
        ["type"] = x => x._observation?.Type?.ToString(),
        ["type.id"] = x => x._observation?.Type?.Id,
        ["type.value"] = x => x._observation?.Type?.Value,

        // Identification
        ["identification"] = x => "Please specify which identification properties you need",
        ["identification.verified"] = x => x._observation?.Identification?.Verified,
        ["identification.verificationstatus"] = x => x._observation?.Identification?.VerificationStatus?.ToString(),
        ["identification.verificationstatus.id"] = x => x._observation?.Identification?.VerificationStatus?.Id,
        ["identification.verificationstatus.value"] = x => x._observation?.Identification?.VerificationStatus?.Value,
        ["identification.confirmedby"] = x => x._observation?.Identification?.ConfirmedBy,
        ["identification.confirmeddate"] = x => x._observation?.Identification?.ConfirmedDate,
        ["identification.identifiedby"] = x => x._observation?.Identification?.IdentifiedBy,
        ["identification.dateidentified"] = x => x._observation?.Identification?.DateIdentified,
        ["identification.uncertainidentification"] = x => x._observation?.Identification?.UncertainIdentification,
        ["identification.determinationmethod"] = x => x._observation?.Identification?.DeterminationMethod?.ToString(),
        ["identification.determinationmethod.id"] = x => x._observation?.Identification?.DeterminationMethod?.Id,
        ["identification.determinationmethod.value"] = x => x._observation?.Identification?.DeterminationMethod?.Value,
        ["identification.verifiedby"] = x => x._observation?.Identification?.VerifiedBy,
        ["identification.identificationqualifier"] = x => x._observation?.Identification?.IdentificationQualifier,
        ["identification.typestatus"] = x => x._observation?.Identification?.TypeStatus,
        ["identification.identificationid"] = x => x._observation?.Identification?.IdentificationId,
        ["identification.identificationreferences"] = x => x._observation?.Identification?.IdentificationReferences,
        ["identification.identificationremarks"] = x => x._observation?.Identification?.IdentificationRemarks,

        // Artportalen
        ["artportaleninternal.invasivespeciestreatment"] = x => x._observation?.ArtportalenInternal?.InvasiveSpeciesTreatment?.ToString(),
        ["artportaleninternal.invasivespeciestreatment.id"] = x => x._observation?.ArtportalenInternal?.InvasiveSpeciesTreatment?.Id,
        ["artportaleninternal.invasivespeciestreatment.value"] = x => x._observation?.ArtportalenInternal?.InvasiveSpeciesTreatment?.Value,

        // Material sample
        ["materialsample"] = x => x._observation?.MaterialSample?.MaterialSampleId,
        ["materialsample.materialsampleid"] = x => x._observation?.MaterialSample?.MaterialSampleId,

        // Organism
        ["organism"] = x => "Please specify which organism properties you need",
        ["organism.organismid"] = x => x._observation?.Organism?.OrganismId,
        ["organism.organismname"] = x => x._observation?.Organism?.OrganismName,
        ["organism.organismscope"] = x => x._observation?.Organism?.OrganismScope,
        ["organism.associatedorganisms"] = x => x._observation?.Organism?.AssociatedOrganisms,
        ["organism.previousidentifications"] = x => x._observation?.Organism?.PreviousIdentifications,
        ["organism.organismremarks"] = x => x._observation?.Organism?.OrganismRemarks,

        // Geological context
        ["geologicalcontext"] = x => "Please specify which geologicalcontext properties you need",
        ["geologicalcontext.bed"] = x => x._observation?.GeologicalContext?.Bed,
        ["geologicalcontext.earliestageorloweststage"] = x => x._observation?.GeologicalContext?.EarliestAgeOrLowestStage,
        ["geologicalcontext.earliesteonorlowesteonothem"] = x => x._observation?.GeologicalContext?.EarliestEonOrLowestEonothem,
        ["geologicalcontext.earliestepochorlowestseries"] = x => x._observation?.GeologicalContext?.EarliestEpochOrLowestSeries,
        ["geologicalcontext.earliesteraorlowesterathem"] = x => x._observation?.GeologicalContext?.EarliestEraOrLowestErathem,
        ["geologicalcontext.earliestgeochronologicalera"] = x => x._observation?.GeologicalContext?.EarliestGeochronologicalEra,
        ["geologicalcontext.earliestperiodorlowestsystem"] = x => x._observation?.GeologicalContext?.EarliestPeriodOrLowestSystem,
        ["geologicalcontext.formation"] = x => x._observation?.GeologicalContext?.Formation,
        ["geologicalcontext.geologicalcontextid"] = x => x._observation?.GeologicalContext?.GeologicalContextId,
        ["geologicalcontext.group"] = x => x._observation?.GeologicalContext?.Group,
        ["geologicalcontext.highestbiostratigraphiczone"] = x => x._observation?.GeologicalContext?.HighestBiostratigraphicZone,
        ["geologicalcontext.latestageorhigheststage"] = x => x._observation?.GeologicalContext?.LatestAgeOrHighestStage,
        ["geologicalcontext.latesteonorhighesteonothem"] = x => x._observation?.GeologicalContext?.LatestEonOrHighestEonothem,
        ["geologicalcontext.latestepochorhighestseries"] = x => x._observation?.GeologicalContext?.LatestEpochOrHighestSeries,
        ["geologicalcontext.latesteraorhighesterathem"] = x => x._observation?.GeologicalContext?.LatestEraOrHighestErathem,
        ["geologicalcontext.latestgeochronologicalera"] = x => x._observation?.GeologicalContext?.LatestGeochronologicalEra,
        ["geologicalcontext.latestperiodorhighestsystem"] = x => x._observation?.GeologicalContext?.LatestPeriodOrHighestSystem,
        ["geologicalcontext.lithostratigraphicterms"] = x => x._observation?.GeologicalContext?.LithostratigraphicTerms,
        ["geologicalcontext.lowestbiostratigraphiczone"] = x => x._observation?.GeologicalContext?.LowestBiostratigraphicZone,
        ["geologicalcontext.member"] = x => x._observation?.GeologicalContext?.Member,
    };

    public object GetValue(PropertyFieldDescription propertyField)
    {
        if (_propertyGetters.TryGetValue(propertyField.PropertyPath, out var getter))
        {
            return getter(this);
        }
        if (propertyField.IsDynamicCreated)
            return null!;
        throw new ArgumentException($"Field is not mapped: \"{propertyField.PropertyPath}\"");
    }

    public object GetDynamicValue(PropertyFieldDescription propertyField)
    {
        var propertyPath = propertyField.PropertyPath.ToLower();
        var regexp = new Regex("^project-\\d+");
        if ((_observation?.Projects?.Any() ?? false) && regexp.Match(propertyPath).Length != 0)
        {
            var projectId = propertyField.DynamicIds.ElementAt(0);
            var project = _observation.Projects.FirstOrDefault(p => p.Id.Equals(projectId));
            if (project != null)
            {
                regexp = new Regex("^project-\\d+.name$");
                if (regexp.Match(propertyPath).Length != 0)
                {
                    return project.Name;
                }
                regexp = new Regex("^project-\\d+.category");
                if (regexp.Match(propertyPath).Length != 0)
                {
                    return project.Category;
                }
                regexp = new Regex("^project-\\d+.url");
                if (regexp.Match(propertyPath).Length != 0)
                {
                    return project.ProjectURL;
                }
                regexp = new Regex("^project-\\d+.parameter-\\d+$");
                if (regexp.Match(propertyPath).Length != 0)
                {
                    var parameterId = propertyField.DynamicIds.ElementAt(1);
                    var parameter = project.ProjectParameters?.FirstOrDefault(pp => pp.Id.Equals(parameterId));
                    return parameter?.Value;
                }
            }
        }

        return null;
    }    

    public string GetStringValue(PropertyFieldDescription propertyField)
    {
        var value = GetValue(propertyField);

        if (value == null)
            return string.Empty;

        return propertyField.DataTypeEnum switch
        {
            PropertyFieldDataType.Boolean =>
                Convert.ToBoolean(value).ToString(CultureInfo.InvariantCulture),

            PropertyFieldDataType.DateTime =>
                Convert.ToDateTime(value).ToShortDateString(),

            PropertyFieldDataType.Double =>
                Convert.ToDouble(value).ToString(CultureInfo.InvariantCulture),

            PropertyFieldDataType.Int32 =>
                Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture),

            PropertyFieldDataType.Int64 =>
                Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture),

            PropertyFieldDataType.TimeSpan =>
                (value is TimeSpan ts ? ts : TimeSpan.Parse(value.ToString()))
                    .ToString("hh\\:mm"),

            _ => value.ToString()
        };
    }
}