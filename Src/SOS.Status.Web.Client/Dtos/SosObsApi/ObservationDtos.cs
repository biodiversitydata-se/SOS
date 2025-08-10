namespace SOS.Status.Web.Client.Dtos.SosObsApi;

/// <summary>
/// Type of aggregation
/// </summary>
public enum AggregationType
{

    SightingsPerWeek = 0,

    SightingsPerYear = 1,

    QuantityPerWeek = 2,

    QuantityPerYear = 3,

    SpeciesSightingsList = 4,

    SpeciesSightingsListTaxonCount = 5,

    SightingsPerWeek48 = 6,

}

/// <summary>
/// API information.
/// </summary>
public class ApiInformation
{

    /// <summary>
    /// Name of the API.
    /// </summary>
    public string ApiName { get; set; }

    /// <summary>
    /// API version with MAJOR, MINOR and PATCH version.
    /// </summary>
    public string ApiVersion { get; set; }

    /// <summary>
    /// The date when this API version was published.
    /// </summary>
    public System.DateTimeOffset ApiReleased { get; set; }

    /// <summary>
    /// A link to the current API documentation.
    /// </summary>
    public System.Uri ApiDocumentation { get; set; }

    /// <summary>
    /// A link to the API changelog.
    /// </summary>
    public System.Uri ApiChangelog { get; set; }

    /// <summary>
    /// The state or status of the API according to lifecycle management. For example. alpha, beta, active, deprecated, retired or decommissioned.
    /// </summary>
    public string ApiStatus { get; set; }

}

/// <summary>
/// Area (region) information.
/// </summary>
public class Area
{

    /// <summary>
    /// FeatureId for the area.
    /// </summary>
    public string FeatureId { get; set; }

    /// <summary>
    /// Name of the area.
    /// </summary>
    public string Name { get; set; }

}


public class AreaBaseDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public AreaTypeDto AreaType { get; set; }

    public LatLonBoundingBox BoundingBox { get; set; }

    /// <summary>
    /// Feature id
    /// </summary>
    public string FeatureId { get; set; }

    /// <summary>
    /// Name of area
    /// </summary>
    public string Name { get; set; }

}

/// <summary>
/// Result returned by paged query
/// </summary>
public class AreaBaseDtoPagedResult
{

    /// <summary>
    /// Paged records
    /// </summary>
    public System.Collections.Generic.ICollection<AreaBaseDto> Records { get; set; }

    /// <summary>
    /// Ignores the specified number of items and returns a sequence starting at the item after the last skipped item (if
    /// <br/>any)
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Returns a sequence containing up to the specified number of items. Anything after the count is ignored
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Total number of records matching the query
    /// </summary>
    public long TotalCount { get; set; }

}

/// <summary>
/// Export format for area.
/// </summary>
public enum AreaExportFormatDto
{

    Json = 0,

    GeoJson = 1,

    Wkt = 2,

}

/// <summary>
/// Area filter.
/// </summary>
public class AreaFilterDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public AreaTypeDto AreaType { get; set; }

    /// <summary>
    /// Feature
    /// </summary>
    public string FeatureId { get; set; }

}

/// <summary>
/// Area type dto
/// </summary>
public enum AreaTypeDto
{

    Municipality = 0,

    Community = 1,

    Sea = 2,

    CountryRegion = 3,

    NatureType = 4,

    Province = 5,

    Ramsar = 6,

    BirdValidationArea = 7,

    Parish = 8,

    Spa = 9,

    County = 10,

    ProtectedNature = 11,

    SwedishForestAgencyDistricts = 12,

    Sci = 13,

    WaterArea = 14,

    Atlas5x5 = 15,

    Atlas10x10 = 16,

    SfvDistricts = 17,

    Campus = 18,

}

/// <summary>
/// Observation information specific for Artportalen.
/// </summary>
public class ArtportalenInternal
{

    /// <summary>
    /// Associated media file
    /// </summary>
    public string AssociatedMedia { get; set; }

    /// <summary>
    /// Bird validation areas.
    /// </summary>
    public System.Collections.Generic.ICollection<string> BirdValidationAreaIds { get; set; }

    /// <summary>
    /// Id of checklist
    /// </summary>
    public int? ChecklistId { get; set; }

    /// <summary>
    /// Year of confirmation.
    /// </summary>
    public int? ConfirmationYear { get; set; }

    /// <summary>
    /// Data source id
    /// </summary>
    public int? DatasourceId { get; set; }

    /// <summary>
    /// Year of determination.
    /// </summary>
    public int? DeterminationYear { get; set; }

    /// <summary>
    /// Id &gt; 0 = diffused
    /// </summary>
    public int DiffusionId { get; set; }

    /// <summary>
    /// Event month range start date =&gt; end date
    /// </summary>
    public System.Collections.Generic.ICollection<int> EventMonths { get; set; }

    /// <summary>
    /// Field diary group id
    /// </summary>
    public int? FieldDiaryGroupId { get; set; }

    /// <summary>
    /// Has Triggered Verification Rules
    /// </summary>
    public bool HasTriggeredVerificationRules { get; set; }

    /// <summary>
    /// Has any Triggered Verification Rule with Warning
    /// </summary>
    public bool HasAnyTriggeredVerificationRuleWithWarning { get; set; }

    /// <summary>
    /// HasUserComments
    /// </summary>
    public bool HasUserComments { get; set; }

    /// <summary>
    /// Media files
    /// </summary>
    public System.Collections.Generic.ICollection<Multimedia> Media { get; set; }

    /// <summary>
    /// Note of Interest.
    /// </summary>
    public bool NoteOfInterest { get; set; }

    /// <summary>
    /// Sighting Id.
    /// </summary>
    public int SightingId { get; set; }

    /// <summary>
    /// Id of SightingSpeciesCollectionItem in Artportalen.
    /// </summary>
    public int? SightingSpeciesCollectionItemId { get; set; }

    /// <summary>
    /// Sighting type.
    /// </summary>
    public int SightingTypeId { get; set; }

    /// <summary>
    /// Sighting type search group id.
    /// </summary>
    public int SightingTypeSearchGroupId { get; set; }

    /// <summary>
    /// Ids of Species Facts connected to Taxon
    /// </summary>
    public System.Collections.Generic.ICollection<int> SpeciesFactsIds { get; set; }

    /// <summary>
    /// Id of publishing types.
    /// </summary>
    public System.Collections.Generic.ICollection<int> SightingPublishTypeIds { get; set; }

    /// <summary>
    /// Internal field used for searches by Artportalen, contains extra user information.
    /// </summary>
    public System.Collections.Generic.ICollection<UserInternal> OccurrenceRecordedByInternal { get; set; }

    /// <summary>
    /// Info about users verifying the observation
    /// </summary>
    public System.Collections.Generic.ICollection<UserInternal> OccurrenceVerifiedByInternal { get; set; }

    /// <summary>
    /// The original presentation name for ParishRegion from data provider.
    /// </summary>
    public string LocationPresentationNameParishRegion { get; set; }

    /// <summary>
    /// Name of parent location, if any.
    /// </summary>
    public string ParentLocality { get; set; }

    /// <summary>
    /// User id of the person that reported the species observation.
    /// </summary>
    public int? ReportedByUserId { get; set; }

    /// <summary>
    /// True if sighting was incremental harvested.
    /// </summary>
    public bool IncrementalHarvested { get; set; }

    /// <summary>
    /// Second hand information flag
    /// </summary>
    public bool SecondHandInformation { get; set; }

    /// <summary>
    /// Sighting barcode url
    /// </summary>
    public string SightingBarcodeURL { get; set; }

    /// <summary>
    /// Sighting summary
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// Triggered observation rule activity id
    /// </summary>
    public int? TriggeredObservationRuleActivityRuleId { get; set; }

    /// <summary>
    /// Triggered observation rule frequency id
    /// </summary>
    public int? TriggeredObservationRuleFrequencyId { get; set; }

    /// <summary>
    /// Triggered observation rule period id
    /// </summary>
    public int? TriggeredObservationRulePeriodRuleId { get; set; }

    /// <summary>
    /// Triggered observation rule promt rule id
    /// </summary>
    public int? TriggeredObservationRulePromptRuleId { get; set; }

    /// <summary>
    /// Triggered observation rule promts
    /// </summary>
    public bool? TriggeredObservationRulePrompts { get; set; }

    /// <summary>
    /// Triggered observation rule regional sighting state
    /// </summary>
    public int? TriggeredObservationRuleRegionalSightingState { get; set; }

    /// <summary>
    /// Triggered observation rule reproduction id
    /// </summary>
    public int? TriggeredObservationRuleReproductionId { get; set; }

    /// <summary>
    /// Triggered observation rule status rule
    /// </summary>
    public int? TriggeredObservationRuleStatusRuleId { get; set; }

}

/// <summary>
/// States the cloud condtions during the survey event.
/// </summary>
public enum Cloudiness
{

    PartlyClear3To5Av8 = 0,

    Clear0Of8 = 1,

    Cloudy6To7Of8 = 2,

    Overcast8Of8 = 3,

    AlmostClear1To2Av8 = 4,

    EverChanging0Till8Av8 = 5,

}

/// <summary>
/// States the wind direction during the survey event as a compass direction.
/// </summary>
public enum CompassDirection
{

    North = 0,

    Northeast = 1,

    Northwest = 2,

    East = 3,

    South = 4,

    Southeast = 5,

    Southwest = 6,

    West = 7,

}

/// <summary>
/// The category of information pertaining to the existence of an Organism (sensu http://rs.tdwg.org/dwc/terms/Organism) at a particular place at a particular time.
/// </summary>
public class DarwinCoreOccurrenceDto
{

    /// <summary>
    /// An identifier for the  set; of information associated with an Event (something that occurs at a place and time). May be a global unique identifier or an identifier specific to the data  set;.
    /// </summary>
    public string EventID { get; set; }

    /// <summary>
    /// An identifier for the broader Event that groups this and potentially other Events.
    /// </summary>
    public string ParentEventID { get; set; }

    /// <summary>
    /// An identifier given to the event in the field. Often serves as a link between field notes and the Event.
    /// </summary>
    public string FieldNumber { get; set; }

    /// <summary>
    /// The date-time or interval during which an Event occurred. For occurrences, this is the date-time when the event was recorded. Not suitable for a time in a geological context.
    /// </summary>
    public string EventDate { get; set; }

    /// <summary>
    /// The time or interval during which an Event occurred.
    /// </summary>
    public string EventTime { get; set; }

    /// <summary>
    /// The earliest integer day of the year on which the Event occurred (1 for January 1, 365 for December 31, except in a leap year, in which case it is 366).
    /// </summary>
    public int? StartDayOfYear { get; set; }

    /// <summary>
    /// The latest integer day of the year on which the Event occurred (1 for January 1, 365 for December 31, except in a leap year, in which case it is 366).
    /// </summary>
    public int? EndDayOfYear { get; set; }

    /// <summary>
    /// The four-digit year in which the Event occurred, according to the Common Era Calendar.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// The integer month in which the Event occurred.
    /// </summary>
    public int? Month { get; set; }

    /// <summary>
    /// he integer day of the month on which the Event occurred.
    /// </summary>
    public int? Day { get; set; }

    /// <summary>
    /// The verbatim original representation of the date and time information for an Event.
    /// </summary>
    public string VerbatimEventDate { get; set; }

    /// <summary>
    /// A category or description of the habitat in which the Event occurred.
    /// </summary>
    public string Habitat { get; set; }

    /// <summary>
    /// The names of, references to, or descriptions of the methods or protocols used during an Event.
    /// </summary>
    public string SamplingProtocol { get; set; }

    /// <summary>
    /// A numeric value for a measurement of the size (time duration, length, area, or volume) of a sample in a sampling event.
    /// </summary>
    public string SampleSizeValue { get; set; }

    /// <summary>
    /// The unit of measurement of the size (time duration, length, area, or volume) of a sample in a sampling event.
    /// </summary>
    public string SampleSizeUnit { get; set; }

    /// <summary>
    /// The amount of effort expended during an Event.
    /// </summary>
    public string SamplingEffort { get; set; }

    /// <summary>
    /// One of a) an indicator of the existence of, b) a reference to (publication, URI), or c) the text of notes taken in the field about the Event.
    /// </summary>
    public string FieldNotes { get; set; }

    /// <summary>
    /// Comments or notes about the Event.
    /// </summary>
    public string EventRemarks { get; set; }

    /// <summary>
    /// An identifier for the set of information associated with a GeologicalContext (the location within a geological context, such as stratigraphy). 
    /// <br/>May be a global unique identifier or an identifier specific to the data set.
    /// </summary>
    public string GeologicalContextID { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic eon or lowest chrono-stratigraphic eonothem or the informal name ("Precambrian") 
    /// <br/>attributable to the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string EarliestEonOrLowestEonothem { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic eon or highest chrono-stratigraphic eonothem or the informal name ("Precambrian") 
    /// <br/>attributable to the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string LatestEonOrHighestEonothem { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic era or lowest chronostratigraphic erathem attributable to the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string EarliestEraOrLowestErathem { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic era or highest chronostratigraphic erathem attributable to the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string LatestEraOrHighestErathem { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic period or lowest chronostratigraphic system attributable to the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string EarliestPeriodOrLowestSystem { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic period or highest chronostratigraphic system attributable to the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string LatestPeriodOrHighestSystem { get; set; }

    /// <summary>
    /// he full name of the earliest possible geochronologic epoch or lowest chronostratigraphic series attributable to the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string EarliestEpochOrLowestSeries { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic epoch or highest chronostratigraphic series attributable to the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string LatestEpochOrHighestSeries { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic age or lowest chronostratigraphic stage attributable to the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string EarliestAgeOrLowestStage { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic age or highest chronostratigraphic stage attributable to the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string LatestAgeOrHighestStage { get; set; }

    /// <summary>
    /// The full name of the lowest possible geological biostratigraphic zone of the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string LowestBiostratigraphicZone { get; set; }

    /// <summary>
    /// The full name of the highest possible geological biostratigraphic zone of the stratigraphic horizon from which the cataloged item was collected.
    /// </summary>
    public string HighestBiostratigraphicZone { get; set; }

    /// <summary>
    /// The combination of all litho-stratigraphic names for the rock from which the cataloged item was collected.
    /// </summary>
    public string LithostratigraphicTerms { get; set; }

    /// <summary>
    /// The full name of the lithostratigraphic group from which the cataloged item was collected.
    /// </summary>
    public string Group { get; set; }

    /// <summary>
    /// The full name of the lithostratigraphic formation from which the cataloged item was collected.
    /// </summary>
    public string Formation { get; set; }

    /// <summary>
    /// The full name of the lithostratigraphic member from which the cataloged item was collected.
    /// </summary>
    public string Member { get; set; }

    /// <summary>
    /// The full name of the lithostratigraphic bed from which the cataloged item was collected.
    /// </summary>
    public string Bed { get; set; }

    /// <summary>
    /// An identifier for the Identification (the body of information associated with the assignment of a scientific name). 
    /// <br/>May be a global unique identifier or an identifier specific to the data set.
    /// </summary>
    public string IdentificationID { get; set; }

    /// <summary>
    /// A string representing the taxonomic identification as it appeared in the original record.
    /// </summary>
    public string VerbatimIdentification { get; set; }

    /// <summary>
    /// A brief phrase or a standard term ("cf.", "aff.") to express the determiner's doubts about the Identification.
    /// </summary>
    public string IdentificationQualifier { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of nomenclatural types (type status, typified scientific name, publication) applied to the subject.
    /// </summary>
    public string TypeStatus { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of names of people, groups, or organizations who assigned the Taxon to the subject.
    /// </summary>
    public string IdentifiedBy { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of the globally unique identifier for the person, people, groups, or organizations responsible for assigning the Taxon to the subject.
    /// </summary>
    public string IdentifiedByID { get; set; }

    /// <summary>
    /// The date on which the subject was determined as representing the Taxon.
    /// </summary>
    public string DateIdentified { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of references (publication, global unique identifier, URI) used in the Identification.
    /// </summary>
    public string IdentificationReferences { get; set; }

    /// <summary>
    /// A categorical indicator of the extent to which the taxonomic identification has been verified to be correct.
    /// </summary>
    public string IdentificationVerificationStatus { get; set; }

    /// <summary>
    /// Comments or notes about the Identification.
    /// </summary>
    public string IdentificationRemarks { get; set; }

    /// <summary>
    /// An identifier for the set of location information (data associated with dcterms:Location). May be a global unique identifier or an identifier specific to the data set.
    /// </summary>
    public string LocationID { get; set; }

    /// <summary>
    /// An identifier for the geographic region within which the Location occurred.
    /// </summary>
    public string HigherGeographyID { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of geographic names less specific than the information captured in the locality term.
    /// </summary>
    public string HigherGeography { get; set; }

    /// <summary>
    /// The name of the continent in which the Location occurs.
    /// </summary>
    public string Continent { get; set; }

    /// <summary>
    /// The name of the water body in which the Location occurs.
    /// </summary>
    public string WaterBody { get; set; }

    /// <summary>
    /// The name of the island on or near which the Location occurs.
    /// </summary>
    public string IslandGroup { get; set; }

    /// <summary>
    /// The name of the country or major administrative unit in which the Location occurs.
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// The standard code for the country in which the Location occurs.
    /// </summary>
    public string CountryCode { get; set; }

    /// <summary>
    /// Name of country region
    /// </summary>
    public string CountryRegion { get; set; }

    /// <summary>
    /// The name of the next smaller administrative region than country (state, province, canton, department, region, etc.) in which the Location occurs.
    /// </summary>
    public string StateProvince { get; set; }

    /// <summary>
    /// The full, unabbreviated name of the next smaller administrative region than stateProvince (county, shire, department, etc.) in which the Location occurs.
    /// </summary>
    public string County { get; set; }

    /// <summary>
    /// The full, unabbreviated name of the next smaller administrative region than county (city, municipality, etc.) in which the Location occurs. 
    /// <br/>Do not use this term for a nearby named place that does not contain the actual location.
    /// </summary>
    public string Municipality { get; set; }

    /// <summary>
    /// The specific description of the place.
    /// </summary>
    public string Locality { get; set; }

    /// <summary>
    /// The original textual description of the place.
    /// </summary>
    public string VerbatimLocality { get; set; }

    /// <summary>
    /// The lower limit of the range of elevation (altitude, usually above sea level), in meters.
    /// </summary>
    public double? MinimumElevationInMeters { get; set; }

    /// <summary>
    /// The upper limit of the range of elevation (altitude, usually above sea level), in meters.
    /// </summary>
    public double? MaximumElevationInMeters { get; set; }

    /// <summary>
    /// The original description of the elevation (altitude, usually above sea level) of the Location.
    /// </summary>
    public string VerbatimElevation { get; set; }

    /// <summary>
    /// The vertical datum used as the reference upon which the values in the elevation terms are based.
    /// </summary>
    public string VerticalDatum { get; set; }

    /// <summary>
    /// The lesser depth of a range of depth below the local surface, in meters.
    /// </summary>
    public double? MinimumDepthInMeters { get; set; }

    /// <summary>
    /// The greater depth of a range of depth below the local surface, in meters.
    /// </summary>
    public double? MaximumDepthInMeters { get; set; }

    /// <summary>
    /// The original description of the depth below the local surface.
    /// </summary>
    public string VerbatimDepth { get; set; }

    /// <summary>
    /// The lesser distance in a range of distance from a reference surface in the vertical direction, in meters. 
    /// <br/>Use positive values for locations above the surface, negative values for locations below. 
    /// <br/>If depth measures are given, the reference surface is the location given by the depth, otherwise the reference surface is the location given by the elevation.
    /// </summary>
    public double? MinimumDistanceAboveSurfaceInMeters { get; set; }

    /// <summary>
    /// The greater distance in a range of distance from a reference surface in the vertical direction, in meters. 
    /// <br/>Use positive values for locations above the surface, negative values for locations below. 
    /// <br/>If depth measures are given, the reference surface is the location given by the depth, otherwise the reference surface is the location given by the elevation.
    /// </summary>
    public double? MaximumDistanceAboveSurfaceInMeters { get; set; }

    /// <summary>
    /// Information about the source of this Location information. Could be a publication (gazetteer), institution, or team of individuals.
    /// </summary>
    public string LocationAccordingTo { get; set; }

    /// <summary>
    /// Comments or notes about the Location.
    /// </summary>
    public string LocationRemarks { get; set; }

    /// <summary>
    /// The geographic latitude (in decimal degrees, using the spatial reference system given in geodeticDatum) of the geographic center of a Location. 
    /// <br/>Positive values are north of the Equator, negative values are south of it. Legal values lie between -90 and 90, inclusive.
    /// </summary>
    public double? DecimalLatitude { get; set; }

    /// <summary>
    /// The geographic longitude (in decimal degrees, using the spatial reference system given in geodeticDatum) of the geographic center of a Location. 
    /// <br/>Positive values are east of the Greenwich Meridian, negative values are west of it. Legal values lie between -180 and 180, inclusive.
    /// </summary>
    public double? DecimalLongitude { get; set; }

    /// <summary>
    /// The ellipsoid, geodetic datum, or spatial reference system (SRS) upon which the geographic coordinates given in decimalLatitude and decimalLongitude as based.
    /// </summary>
    public string GeodeticDatum { get; set; }

    /// <summary>
    /// The horizontal distance (in meters) from the given decimalLatitude and decimalLongitude describing the smallest circle containing the whole of the Location. 
    /// <br/>Leave the value empty if the uncertainty is unknown, cannot be estimated, or is not applicable (because there are no coordinates). Zero is not a valid value for this term.
    /// </summary>
    public int? CoordinateUncertaintyInMeters { get; set; }

    /// <summary>
    /// A decimal representation of the precision of the coordinates given in the decimalLatitude and decimalLongitude.
    /// </summary>
    public double? CoordinatePrecision { get; set; }

    /// <summary>
    /// The ratio of the area of the point-radius (decimalLatitude, decimalLongitude, coordinateUncertaintyInMeters) to the area of the true (original, 
    /// <br/>or most specific) spatial representation of the Location. Legal values are 0, greater than or equal to 1, or undefined. A value of 1 is an exact match or 100% overlap. 
    /// <br/>A value of 0 should be used if the given point-radius does not completely contain the original representation. 
    /// <br/>The pointRadiusSpatialFit is undefined (and should be left empty) if the original representation is a point without uncertainty and the given georeference is not that same point (without uncertainty). 
    /// <br/>If both the original and the given georeference are the same point, the pointRadiusSpatialFit is 1.
    /// </summary>
    public string PointRadiusSpatialFit { get; set; }

    /// <summary>
    /// The verbatim original spatial coordinates of the Location. The coordinate ellipsoid, geodeticDatum, or full Spatial Reference System (SRS) 
    /// <br/>for these coordinates should be stored in verbatimSRS and the coordinate system should be stored in verbatimCoordinateSystem.
    /// </summary>
    public string VerbatimCoordinates { get; set; }

    /// <summary>
    /// The verbatim original latitude of the Location. The coordinate ellipsoid, geodeticDatum, or full Spatial Reference System (SRS) 
    /// <br/>for these coordinates should be stored in verbatimSRS and the coordinate system should be stored in verbatimCoordinateSystem.
    /// </summary>
    public string VerbatimLatitude { get; set; }

    /// <summary>
    /// The verbatim original longitude of the Location. The coordinate ellipsoid, geodeticDatum, or full Spatial Reference System (SRS) 
    /// <br/>for these coordinates should be stored in verbatimSRS and the coordinate system should be stored in verbatimCoordinateSystem.
    /// </summary>
    public string VerbatimLongitude { get; set; }

    /// <summary>
    /// The coordinate format for the verbatimLatitude and verbatimLongitude or the verbatimCoordinates of the Location.
    /// </summary>
    public string VerbatimCoordinateSystem { get; set; }

    /// <summary>
    /// The ellipsoid, geodetic datum, or spatial reference system (SRS) upon which coordinates given in verbatimLatitude and verbatimLongitude, or verbatimCoordinates are based.
    /// </summary>
    public string VerbatimSRS { get; set; }

    /// <summary>
    /// A Well-Known Text (WKT) representation of the shape (footprint, geometry) that defines the Location. 
    /// <br/>A Location may have both a point-radius representation (see decimalLatitude) and a footprint representation, and they may differ from each other.
    /// </summary>
    public string FootprintWKT { get; set; }

    /// <summary>
    /// The ellipsoid, geodetic datum, or spatial reference system (SRS) upon which the geometry given in footprintWKT is based.
    /// </summary>
    public string FootprintSRS { get; set; }

    /// <summary>
    /// he ratio of the area of the footprint (footprintWKT) to the area of the true (original, or most specific) spatial representation of the Location. 
    /// <br/>Legal values are 0, greater than or equal to 1, or undefined. A value of 1 is an exact match or 100% overlap. A value of 0 should be used if the given footprint does not 
    /// <br/>completely contain the original representation. The footprintSpatialFit is undefined (and should be left empty) if the original representation is a point without uncertainty 
    /// <br/>and the given georeference is not that same point (without uncertainty). If both the original and the given georeference are the same point, the footprintSpatialFit is 1.
    /// </summary>
    public string FootprintSpatialFit { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of names of people, groups, or organizations who determined the georeference (spatial representation) for the Location.
    /// </summary>
    public string GeoreferencedBy { get; set; }

    /// <summary>
    /// The date on which the Location was georeferenced.
    /// </summary>
    public string GeoreferencedDate { get; set; }

    /// <summary>
    /// A description or reference to the methods used to determine the spatial footprint, coordinates, and uncertainties.
    /// </summary>
    public string GeoreferenceProtocol { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of maps, gazetteers, or other resources used to georeference the Location, described specifically enough to allow anyone in the future to use the same resources.
    /// </summary>
    public string GeoreferenceSources { get; set; }

    /// <summary>
    /// Notes or comments about the spatial description determination, explaining assumptions made in addition or opposition to the those formalized in the method referred to in georeferenceProtocol.
    /// </summary>
    public string GeoreferenceRemarks { get; set; }

    /// <summary>
    /// An identifier for the MaterialSample (as opposed to a particular digital record of the material sample). In the absence of a persistent global unique identifier, 
    /// <br/>construct one from a combination of identifiers in the record that will most closely make the materialSampleID globally unique.
    /// </summary>
    public string MaterialSampleID { get; set; }

    /// <summary>
    /// An identifier for the Occurrence (as opposed to a particular digital record of the occurrence). In the absence of a persistent global unique identifier, 
    /// <br/>construct one from a combination of identifiers in the record that will most closely make the occurrenceID globally unique.
    /// </summary>
    public string OccurrenceID { get; set; }

    /// <summary>
    /// An identifier (preferably unique) for the record within the data set or collection.
    /// </summary>
    public string CatalogNumber { get; set; }

    /// <summary>
    /// An identifier given to the Occurrence at the time it was recorded. Often serves as a link between field notes and an Occurrence record, such as a specimen collector's number.
    /// </summary>
    public string RecordNumber { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of names of people, groups, or organizations responsible for recording the original Occurrence. 
    /// <br/>The primary collector or observer, especially one who applies a personal identifier (recordNumber), should be listed first.
    /// </summary>
    public string RecordedBy { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of the globally unique identifier for the person, people, groups, or organizations responsible for recording the original Occurrence.
    /// </summary>
    public string RecordedByID { get; set; }

    /// <summary>
    /// The number of individuals present at the time of the Occurrence.
    /// </summary>
    public string IndividualCount { get; set; }

    /// <summary>
    /// A number or enumeration value for the quantity of organisms.
    /// </summary>
    public string OrganismQuantity { get; set; }

    /// <summary>
    /// The type of quantification system used for the quantity of organisms.
    /// </summary>
    public string OrganismQuantityType { get; set; }

    /// <summary>
    /// The sex of the biological individual(s) represented in the Occurrence.
    /// </summary>
    public string Sex { get; set; }

    /// <summary>
    /// The age class or life stage of the Organism(s) at the time the Occurrence was recorded.
    /// </summary>
    public string LifeStage { get; set; }

    /// <summary>
    /// The reproductive condition of the biological individual(s) represented in the Occurrence.
    /// </summary>
    public string ReproductiveCondition { get; set; }

    /// <summary>
    /// The behavior shown by the subject at the time the Occurrence was recorded.
    /// </summary>
    public string Behavior { get; set; }

    /// <summary>
    /// Statement about whether an organism or organisms have been introduced to a given place and time through the direct or indirect activity of modern humans.
    /// </summary>
    public string EstablishmentMeans { get; set; }

    /// <summary>
    /// The degree to which an Organism survives, reproduces, and expands its range at the given place and time.
    /// </summary>
    public string DegreeOfEstablishment { get; set; }

    /// <summary>
    /// The process by which an Organism came to be in a given place at a given time.
    /// </summary>
    public string Pathway { get; set; }

    /// <summary>
    /// A categorical description of the extent to which the georeference has been verified to represent the best possible spatial description for the Location of the Occurrence.
    /// </summary>
    public string GeoreferenceVerificationStatus { get; set; }

    /// <summary>
    /// A statement about the presence or absence of a Taxon at a Location.
    /// </summary>
    public string OccurrenceStatus { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of preparations and preservation methods for a specimen.
    /// </summary>
    public string Preparations { get; set; }

    /// <summary>
    /// The current state of a specimen with respect to the collection identified in collectionCode or collectionID.
    /// </summary>
    public string Disposition { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers (publication, global unique identifier, URI) of media associated with the Occurrence.
    /// </summary>
    public string AssociatedMedia { get; set; }

    /// <summary>
    /// Media associated with the observation
    /// </summary>
    public System.Collections.Generic.ICollection<Multimedia> Media { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers of other Occurrence records and their associations to this Occurrence.
    /// </summary>
    public string AssociatedOccurrences { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers (publication, bibliographic reference, global unique identifier, URI) of literature associated with the Occurrence.
    /// </summary>
    public string AssociatedReferences { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers (publication, global unique identifier, URI) of genetic sequence information associated with the Occurrence.
    /// </summary>
    public string AssociatedSequences { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers or names of taxa and the associations of this Occurrence to each of them.
    /// </summary>
    public string AssociatedTaxa { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of previous or alternate fully qualified catalog numbers or other human-used identifiers for the same Occurrence, 
    /// <br/>whether in the current or any other data set or collection.
    /// </summary>
    public string OtherCatalogNumbers { get; set; }

    /// <summary>
    /// Comments or notes about the Occurrence.
    /// </summary>
    public string OccurrenceRemarks { get; set; }

    /// <summary>
    /// An identifier for the Organism instance (as opposed to a particular digital record of the Organism). May be a globally unique identifier or an identifier specific to the data set.
    /// </summary>
    public string OrganismID { get; set; }

    /// <summary>
    /// A textual name or label assigned to an Organism instance.
    /// </summary>
    public string OrganismName { get; set; }

    /// <summary>
    /// A description of the kind of Organism instance. Can be used to indicate whether the Organism instance represents a discrete organism or if it represents a particular type of aggregation.
    /// </summary>
    public string OrganismScope { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers of other Organisms and the associations of this Organism to each of them.
    /// </summary>
    public string AssociatedOrganisms { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of previous assignments of names to the Organism.
    /// </summary>
    public string PreviousIdentifications { get; set; }

    /// <summary>
    /// Comments or notes about the Organism instance.
    /// </summary>
    public string OrganismRemarks { get; set; }

    /// <summary>
    /// The nature or genre of the resource.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// The most recent date-time on which the resource was changed.
    /// </summary>
    public System.DateTimeOffset? Modified { get; set; }

    /// <summary>
    /// A language of the resource.
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// A legal document giving official permission to do something with the resource.
    /// </summary>
    public string License { get; set; }

    /// <summary>
    /// A person or organization owning or managing rights over the resource.
    /// </summary>
    public string RightsHolder { get; set; }

    /// <summary>
    /// Information about who can access the resource or an indication of its security status.
    /// </summary>
    public string AccessRights { get; set; }

    /// <summary>
    /// A bibliographic reference for the resource as a statement indicating how this record should be cited (attributed) when used.
    /// </summary>
    public string BibliographicCitation { get; set; }

    /// <summary>
    /// A related resource that is referenced, cited, or otherwise pointed to by the described resource.
    /// </summary>
    public string References { get; set; }

    /// <summary>
    /// An identifier for the institution having custody of the object(s) or information referred to in the record.
    /// </summary>
    public string InstitutionID { get; set; }

    /// <summary>
    /// An identifier for the collection or dataset from which the record was derived.
    /// </summary>
    public string CollectionID { get; set; }

    /// <summary>
    /// An identifier for the set of data. May be a global unique identifier or an identifier specific to a collection or institution.
    /// </summary>
    public string DatasetID { get; set; }

    /// <summary>
    /// The name (or acronym) in use by the institution having custody of the object(s) or information referred to in the record.
    /// </summary>
    public string InstitutionCode { get; set; }

    /// <summary>
    /// The name, acronym, coden, or initialism identifying the collection or data set from which the record was derived.
    /// </summary>
    public string CollectionCode { get; set; }

    /// <summary>
    /// The name identifying the data set from which the record was derived.
    /// </summary>
    public string DatasetName { get; set; }

    /// <summary>
    /// The name (or acronym) in use by the institution having ownership of the object(s) or information referred to in the record.
    /// </summary>
    public string OwnerInstitutionCode { get; set; }

    /// <summary>
    /// The specific nature of the data record.
    /// </summary>
    public string BasisOfRecord { get; set; }

    /// <summary>
    /// Additional information that exists, but that has not been shared in the given record.
    /// </summary>
    public string InformationWithheld { get; set; }

    /// <summary>
    /// Actions taken to make the shared data less specific or complete than in its original form. Suggests that alternative data of higher quality may be available on request.
    /// </summary>
    public string DataGeneralizations { get; set; }

    /// <summary>
    /// A list of additional measurements, facts, characteristics, or assertions about the record. Meant to provide a mechanism for structured content.
    /// </summary>
    public string DynamicProperties { get; set; }

    /// <summary>
    /// An identifier for the set of taxon information (data associated with the Taxon class). May be a global unique identifier or an identifier specific to the data set.
    /// </summary>
    public string TaxonID { get; set; }

    /// <summary>
    /// An identifier for the nomenclatural (not taxonomic) details of a scientific name.
    /// </summary>
    public string ScientificNameID { get; set; }

    /// <summary>
    /// An identifier for the name usage (documented meaning of the name according to a source) of the currently valid (zoological) or accepted (botanical) taxon.
    /// </summary>
    public string AcceptedNameUsageID { get; set; }

    /// <summary>
    /// An identifier for the name usage (documented meaning of the name according to a source) of the direct, 
    /// <br/>most proximate higher-rank parent taxon (in a classification) of the most specific element of the scientificName.
    /// </summary>
    public string ParentNameUsageID { get; set; }

    /// <summary>
    /// An identifier for the name usage (documented meaning of the name according to a source) in which the terminal element of the scientificName 
    /// <br/>was originally established under the rules of the associated nomenclaturalCode.
    /// </summary>
    public string OriginalNameUsageID { get; set; }

    /// <summary>
    /// An identifier for the source in which the specific taxon concept circumscription is defined or implied.
    /// </summary>
    public string NameAccordingToID { get; set; }

    /// <summary>
    /// An identifier for the publication in which the scientificName was originally established under the rules of the associated nomenclaturalCode.
    /// </summary>
    public string NamePublishedInID { get; set; }

    /// <summary>
    /// An identifier for the taxonomic concept to which the record refers - not for the nomenclatural details of a taxon.
    /// </summary>
    public string TaxonConceptID { get; set; }

    /// <summary>
    /// The full scientific name, with authorship and date information if known. When forming part of an Identification, this should be the name in lowest level taxonomic rank that can be determined. 
    /// <br/>This term should not contain identification qualifications, which should instead be supplied in the IdentificationQualifier term.
    /// </summary>
    public string ScientificName { get; set; }

    /// <summary>
    /// The full name, with authorship and date information if known, of the currently valid (zoological) or accepted (botanical) taxon.
    /// </summary>
    public string AcceptedNameUsage { get; set; }

    /// <summary>
    /// The full name, with authorship and date information if known, of the direct, most proximate higher-rank parent taxon (in a classification) of the most specific element of the scientificName.
    /// </summary>
    public string ParentNameUsage { get; set; }

    /// <summary>
    /// The taxon name, with authorship and date information if known, as it originally appeared when first established under the rules of the associated nomenclaturalCode. 
    /// <br/>The basionym (botany) or basonym (bacteriology) of the scientificName or the senior/earlier homonym for replaced names.
    /// </summary>
    public string OriginalNameUsage { get; set; }

    /// <summary>
    /// The reference to the source in which the specific taxon concept circumscription is defined or implied - traditionally signified by the Latin "sensu" or "sec." (from secundum, meaning "according to"). 
    /// <br/>For taxa that result from identifications, a reference to the keys, monographs, experts and other sources should be given.
    /// </summary>
    public string NameAccordingTo { get; set; }

    /// <summary>
    /// A reference for the publication in which the scientificName was originally established under the rules of the associated nomenclaturalCode.
    /// </summary>
    public string NamePublishedIn { get; set; }

    /// <summary>
    /// The four-digit year in which the scientificName was published.
    /// </summary>
    public string NamePublishedInYear { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of taxa names terminating at the rank immediately superior to the taxon referenced in the taxon record.
    /// </summary>
    public string HigherClassification { get; set; }

    /// <summary>
    /// The full scientific name of the kingdom in which the taxon is classified.
    /// </summary>
    public string Kingdom { get; set; }

    /// <summary>
    /// The full scientific name of the phylum or division in which the taxon is classified.
    /// </summary>
    public string Phylum { get; set; }

    /// <summary>
    /// The full scientific name of the class in which the taxon is classified.
    /// </summary>
    public string Class { get; set; }

    /// <summary>
    /// The full scientific name of the order in which the taxon is classified.
    /// </summary>
    public string Order { get; set; }

    /// <summary>
    /// The full scientific name of the family in which the taxon is classified.
    /// </summary>
    public string Family { get; set; }

    /// <summary>
    /// The full scientific name of the subfamily in which the taxon is classified.
    /// </summary>
    public string Subfamily { get; set; }

    /// <summary>
    /// The full scientific name of the genus in which the taxon is classified.
    /// </summary>
    public string Genus { get; set; }

    /// <summary>
    /// The genus part of the scientificName without authorship.
    /// </summary>
    public string GenericName { get; set; }

    /// <summary>
    /// The full scientific name of the subgenus in which the taxon is classified. Values should include the genus to avoid homonym confusion.
    /// </summary>
    public string Subgenus { get; set; }

    /// <summary>
    /// The infrageneric part of a binomial name at ranks above species but below genus.
    /// </summary>
    public string InfragenericEpithet { get; set; }

    /// <summary>
    /// The name of the first or species epithet of the scientificName.
    /// </summary>
    public string SpecificEpithet { get; set; }

    /// <summary>
    /// The name of the lowest or terminal infraspecific epithet of the scientificName, excluding any rank designation.
    /// </summary>
    public string InfraspecificEpithet { get; set; }

    /// <summary>
    /// Part of the name of a cultivar, cultivar group or grex that follows the scientific name.
    /// </summary>
    public string CultivarEpithet { get; set; }

    /// <summary>
    /// The taxonomic rank of the most specific name in the scientificName.
    /// </summary>
    public string TaxonRank { get; set; }

    /// <summary>
    /// The taxonomic rank of the most specific name in the scientificName as it appears in the original record.
    /// </summary>
    public string VerbatimTaxonRank { get; set; }

    /// <summary>
    /// The authorship information for the scientificName formatted according to the conventions of the applicable nomenclaturalCode.
    /// </summary>
    public string ScientificNameAuthorship { get; set; }

    /// <summary>
    /// A common or vernacular name.
    /// </summary>
    public string VernacularName { get; set; }

    /// <summary>
    /// The nomenclatural code (or codes in the case of an ambiregnal name) under which the scientificName is constructed.
    /// </summary>
    public string NomenclaturalCode { get; set; }

    /// <summary>
    /// The status of the use of the scientificName as a label for a taxon. Requires taxonomic opinion to define the scope of a taxon. 
    /// <br/>Rules of priority then are used to define the taxonomic status of the nomenclature contained in that scope, combined with the experts opinion. 
    /// <br/>It must be linked to a specific taxonomic reference that defines the concept.
    /// </summary>
    public string TaxonomicStatus { get; set; }

    /// <summary>
    /// The status related to the original publication of the name and its conformance to the relevant rules of nomenclature. 
    /// <br/>It is based essentially on an algorithm according to the business rules of the code. It requires no taxonomic opinion.
    /// </summary>
    public string NomenclaturalStatus { get; set; }

    /// <summary>
    /// Comments or notes about the taxon or name.
    /// </summary>
    public string TaxonRemarks { get; set; }

}


public enum DataProviderCategory
{

    DataHostesship = 0,

    NationalInventory = 1,

    RegionalInventory = 2,

    MuseumCollections = 3,

    HerbariaCollections = 4,

    CitizenSciencePlatform = 5,

    Atlas = 6,

    Terrestrial = 7,

    Freshwater = 8,

    Marine = 9,

    Vertebrates = 10,

    Invertebrates = 11,

    Arthropods = 12,

    Microorganisms = 13,

    Plants_Bryophytes_Lichens = 14,

    Fungi = 15,

    Algae = 16,

    DataStewardship = 17,

}

/// <summary>
/// Information about a data provider.
/// </summary>
public class DataProviderDto
{

    /// <summary>
    /// Id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// A unique text identifer for the data provider.
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// The name of the data provider.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the data provider.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The organization name.
    /// </summary>
    public string Organization { get; set; }

    /// <summary>
    /// Paths that can be used to group and visualize a data provider as a tree in a GUI.
    /// </summary>
    public System.Collections.Generic.ICollection<string> Path { get; set; }

    /// <summary>
    /// URL to the data provider source.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Number of public observations.
    /// </summary>
    public int PublicObservations { get; set; }

    /// <summary>
    /// Number of protected observations.
    /// </summary>
    public int ProtectedObservations { get; set; }

    /// <summary>
    /// Latest harvest date.
    /// </summary>
    public System.DateTimeOffset? LatestHarvestDate { get; set; }

    /// <summary>
    /// Latest process date.
    /// </summary>
    public System.DateTimeOffset? LatestProcessDate { get; set; }

    /// <summary>
    /// Latest incremental harvest and process date. Used for data providers supporting incremental harvest.
    /// </summary>
    public System.DateTimeOffset? LatestIncrementalHarvestDate { get; set; }

    /// <summary>
    /// Date time from where next harvest can be run.
    /// </summary>
    public System.DateTimeOffset? NextHarvestFrom { get; set; }

    /// <summary>
    /// Note about harvest
    /// </summary>
    public string HarvestNotes { get; set; }

    /// <summary>
    /// Cron expression used to schedule harvest.
    /// </summary>
    public string HarvestSchedule { get; set; }

    /// <summary>
    /// Decides whether the data provider should be included in search when no data provider filter is set.
    /// </summary>
    public bool IncludeInSearchByDefault { get; set; }

}

/// <summary>
/// Data provider filter.
/// </summary>
public class DataProviderFilterDto
{

    /// <summary>
    /// Data provider id's
    /// </summary>
    public System.Collections.Generic.ICollection<int> Ids { get; set; }

}

/// <summary>
/// Class related to data quality
/// </summary>
public class DataQuality
{

    /// <summary>
    /// Hashed key created from observation date + taxon id + position
    /// </summary>
    public string UniqueKey { get; set; }

}


public class DataStewardshipFilterDto
{

    /// <summary>
    /// Dataset filter
    /// </summary>
    public System.Collections.Generic.ICollection<string> DatasetIdentifiers { get; set; }

}


public class DataStewardshipInfo
{

    /// <summary>
    /// Dataset Identifier
    /// </summary>
    public string DatasetIdentifier { get; set; }

    /// <summary>
    /// Dataset Title
    /// </summary>
    public string DatasetTitle { get; set; }

}


public enum DateFilterComparisonDto
{

    StartDate = 0,

    EndDate = 1,

    BothStartDateAndEndDate = 2,

    StartDateEndDateMonthRange = 3,

}

/// <summary>
/// Date filter.
/// </summary>
public class DateFilterDto
{

    /// <summary>
    /// Observation start date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed
    /// </summary>
    public System.DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// Observation end date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed
    /// </summary>
    public System.DateTimeOffset? EndDate { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public DateFilterTypeDto DateFilterType { get; set; }

    /// <summary>
    /// Predefined time ranges
    /// </summary>
    // TODO(system.text.json): Add string enum item converter
    public System.Collections.Generic.ICollection<TimeRangeDto> TimeRanges { get; set; }

}

/// <summary>
/// OverlappingStartDateAndEndDate, Start or EndDate of the observation must be within the specified interval    
/// <br/>BetweenStartDateAndEndDate, Start and EndDate of the observation must be within the specified interval    
/// <br/>OnlyStartDate, Only StartDate of the observation must be within the specified interval            
/// <br/>OnlyEndDate, Only EndDate of the observation must be within the specified interval
/// </summary>
public enum DateFilterTypeDto
{

    OverlappingStartDateAndEndDate = 0,

    BetweenStartDateAndEndDate = 1,

    OnlyStartDate = 2,

    OnlyEndDate = 3,

}

/// <summary>
/// Observation diffuse status.
/// </summary>
public enum DiffusionStatus
{

    NotDiffused = 0,

    DiffusedBySystem = 1,

    DiffusedByProvider = 2,

}

/// <summary>
/// Diffuse status dto
/// </summary>
public enum DiffusionStatusDto
{

    NotDiffused = 0,

    DiffusedBySystem = 1,

    DiffusedByProvider = 2,

}

/// <summary>
/// Environment information
/// </summary>
public class EnvironmentInformationDto
{

    /// <summary>
    /// Type of environment (Local, Dev, ST, AT, Prod)
    /// </summary>
    public string EnvironmentType { get; set; }

    /// <summary>
    /// Name of the server hosting the application/service
    /// </summary>
    public string HostingServerName { get; set; }

    /// <summary>
    /// Server operating system.
    /// </summary>
    public string OsPlatform { get; set; }

    /// <summary>
    /// .Net version.
    /// </summary>
    public string AspDotnetVersion { get; set; }

    /// <summary>
    /// Current Culture.
    /// </summary>
    public string CurrentCulture { get; set; }

}

/// <summary>
/// Event information about a species observation.
/// </summary>
public class Event
{

    /// <summary>
    /// An identifier for the set of information associated with an Event (something that occurs at a place and time).
    /// </summary>
    public string EventId { get; set; }

    /// <summary>
    /// Start date/time of the event in W. Europe Standard Time.
    /// </summary>
    public System.DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// Start day of year
    /// </summary>
    public int StartDayOfYear { get; set; }

    /// <summary>
    /// Divide year in 48 "weeks"
    /// </summary>
    public int? StartHistogramWeek { get; set; }

    /// <summary>
    /// Start year of the event, Swedish localization
    /// </summary>
    public int? StartYear { get; set; }

    /// <summary>
    /// Start month of the event, Swedish localization
    /// </summary>
    public int? StartMonth { get; set; }

    /// <summary>
    /// Start day of the event, Swedish localization
    /// </summary>
    public int? StartDay { get; set; }

    /// <summary>
    /// End date/time of the event in W. Europe Standard Time.
    /// </summary>
    public System.DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// Start day of year
    /// </summary>
    public int EndDayOfYear { get; set; }

    /// <summary>
    /// Divide year in 48 weeks
    /// </summary>
    public int? EndHistogramWeek { get; set; }

    /// <summary>
    /// End year of the event, Swedish localization
    /// </summary>
    public int? EndYear { get; set; }

    /// <summary>
    /// End month of the event, Swedish localization
    /// </summary>
    public int? EndMonth { get; set; }

    /// <summary>
    /// End day of the event, Swedish localization
    /// </summary>
    public int? EndDay { get; set; }

    /// <summary>
    /// Start date of the event in the format yyyy-MM-dd.
    /// </summary>
    public string PlainStartDate { get; set; }

    /// <summary>
    /// End date of the event in the format yyyy-MM-dd.
    /// </summary>
    public string PlainEndDate { get; set; }

    /// <summary>
    /// Start time of the event in W. Europe Standard Time formatted as hh:mm.
    /// </summary>
    public string PlainStartTime { get; set; }

    /// <summary>
    /// End time of the event in W. Europe Standard Time formatted as hh:mm.
    /// </summary>
    public string PlainEndTime { get; set; }

    public VocabularyValue DiscoveryMethod { get; set; }

    /// <summary>
    /// Comments or notes about the Event.
    /// </summary>
    public string EventRemarks { get; set; }

    /// <summary>
    /// One of a) an indicator of the existence of, b) a
    /// <br/>reference to (publication, URI), or c) the text of
    /// <br/>notes taken in the field about the Event.
    /// </summary>
    public string FieldNotes { get; set; }

    /// <summary>
    /// An identifier given to the event in the field. Often
    /// <br/>serves as a link between field notes and the Event.
    /// </summary>
    public string FieldNumber { get; set; }

    /// <summary>
    /// A category or description of the habitat in which the Event occurred.
    /// </summary>
    public string Habitat { get; set; }

    /// <summary>
    /// Multimedia associated with the event.
    /// </summary>
    public System.Collections.Generic.ICollection<Multimedia> Media { get; set; }

    /// <summary>
    /// Measurement or facts associated with the event.
    /// </summary>
    public System.Collections.Generic.ICollection<ExtendedMeasurementOrFact> MeasurementOrFacts { get; set; }

    /// <summary>
    /// An identifier for the broader Event that groups this and potentially other Events.
    /// </summary>
    public string ParentEventId { get; set; }

    /// <summary>
    /// The amount of effort expended during an Event.
    /// </summary>
    public string SamplingEffort { get; set; }

    /// <summary>
    /// The name of, reference to, or description of the
    /// <br/>method or protocol used during an Event.
    /// </summary>
    public string SamplingProtocol { get; set; }

    /// <summary>
    /// The unit of measurement of the size (time duration, length, area, or volume) of a sample in a sampling event.
    /// <br/>A sampleSizeUnit must have a corresponding sampleSizeValue, e.g., 5 for sampleSizeValue with metre for
    /// <br/>sampleSizeUnit.
    /// </summary>
    public string SampleSizeUnit { get; set; }

    /// <summary>
    /// A numeric value for a measurement of the size (time duration, length, area, or volume) of a sample in a sampling
    /// <br/>event.
    /// </summary>
    public string SampleSizeValue { get; set; }

    /// <summary>
    /// The verbatim original representation of the date and time information for an Event.
    /// <br/>Examples: spring 1910, Marzo 2002, 1999-03-XX, 17IV1934.
    /// </summary>
    public string VerbatimEventDate { get; set; }

    public Weather Weather { get; set; }

}


public class EventFilterDto
{

    /// <summary>
    /// Event id's
    /// </summary>
    public System.Collections.Generic.ICollection<string> Ids { get; set; }

}


public class ExcludeFilterDto
{

    /// <summary>
    /// Exclude observations with listed occurrence id's
    /// </summary>
    public System.Collections.Generic.ICollection<string> OccurrenceIds { get; set; }

}

/// <summary>
/// Supported export formats
/// </summary>
public enum ExportFormat
{

    Csv = 0,

    DwC = 1,

    DwCEvent = 2,

    GeoJson = 3,

    Excel = 4,

}

/// <summary>
/// Keep control of user exports
/// </summary>
public class ExportJobInfoDto
{

    public string Id { get; set; }

    public System.DateTimeOffset CreatedDate { get; set; }

    public System.DateTimeOffset? ExpireDate { get; set; }

    public System.DateTimeOffset? ProcessStartDate { get; set; }

    public System.DateTimeOffset? ProcessEndDate { get; set; }

    public TimeSpan ProcessingTime { get; set; }

    public int? NumberOfObservations { get; set; }

    public string Description { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public ExportFormat Format { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public ExportJobStatus Status { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public OutputFieldSet OutputFieldSet { get; set; }

    public string PickUpUrl { get; set; }

}


public enum ExportJobStatus
{

    Unknown = 0,

    Queued = 1,

    Processing = 2,

    Succeeded = 3,

    Failed = 4,

}


public class ExtendedFilterDto
{

    /// <summary>
    /// Checklist Id
    /// </summary>
    public int? ChecklistId { get; set; }

    /// <summary>
    /// Field diary group Id's
    /// </summary>
    public System.Collections.Generic.ICollection<int> FieldDiaryGroupIds { get; set; }

    /// <summary>
    /// Reported by Artportalen user id.
    /// </summary>
    public int? ReportedByUserId { get; set; }

    /// <summary>
    /// Observed by Artportalen user id.
    /// </summary>
    public int? ObservedByUserId { get; set; }

    /// <summary>
    /// Reported by user service user id.
    /// </summary>
    public int? ReportedByUserServiceUserId { get; set; }

    /// <summary>
    /// Observed by user service user id.
    /// </summary>
    public int? ObservedByUserServiceUserId { get; set; }

    /// <summary>
    /// Id of sex to match
    /// </summary>
    public System.Collections.Generic.ICollection<int> SexIds { get; set; }

    /// <summary>
    /// Only include hits with media associated
    /// </summary>
    public bool OnlyWithMedia { get; set; }

    /// <summary>
    /// Only include hits with notes attached to them
    /// </summary>
    public bool OnlyWithNotes { get; set; }

    public bool OnlyWithNotesOfInterest { get; set; }

    /// <summary>
    /// Only include hits that have user comments on them
    /// </summary>
    public bool OnlyWithUserComments { get; set; }

    public bool OnlyWithBarcode { get; set; }

    public System.DateTimeOffset? ReportedDateFrom { get; set; }

    public System.DateTimeOffset? ReportedDateTo { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingTypeFilterDto TypeFilter { get; set; }

    public bool UsePeriodForAllYears { get; set; }

    public System.Collections.Generic.ICollection<int> Months { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public DateFilterComparisonDto MonthsComparison { get; set; }

    public System.Collections.Generic.ICollection<int> DiscoveryMethodIds { get; set; }

    public System.Collections.Generic.ICollection<int> LifeStageIds { get; set; }

    public System.Collections.Generic.ICollection<int> ActivityIds { get; set; }

    public bool HasTriggeredVerificationRule { get; set; }

    public bool HasTriggeredVerificationRuleWithWarning { get; set; }

    public int? Length { get; set; }

    public string LengthOperator { get; set; }

    public int? Weight { get; set; }

    public string WeightOperator { get; set; }

    public int? Quantity { get; set; }

    public string QuantityOperator { get; set; }

    public System.Collections.Generic.ICollection<int> VerificationStatusIds { get; set; }

    public System.Collections.Generic.ICollection<int> ExcludeVerificationStatusIds { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingUnspontaneousFilterDto UnspontaneousFilter { get; set; }

    public string SpeciesCollectionLabel { get; set; }

    public string PublicCollection { get; set; }

    public string PrivateCollection { get; set; }

    public int? SubstrateSpeciesId { get; set; }

    public int? SubstrateId { get; set; }

    public int? BiotopeId { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingNotPresentFilterDto NotPresentFilter { get; set; }

    public bool OnlySecondHandInformation { get; set; }

    public System.Collections.Generic.ICollection<int> PublishTypeIdsFilter { get; set; }

    public System.Collections.Generic.ICollection<int> RegionalSightingStateIdsFilter { get; set; }

    public System.Collections.Generic.ICollection<int> TriggeredObservationRuleFrequencyIds { get; set; }

    public System.Collections.Generic.ICollection<int> TriggeredObservationRuleReproductionIds { get; set; }

    public System.Collections.Generic.ICollection<int> SiteIds { get; set; }

    public System.Collections.Generic.ICollection<int> SiteProjectIds { get; set; }

    public System.Collections.Generic.ICollection<int> SpeciesFactsIds { get; set; }

    public string InstitutionId { get; set; }

    public System.Collections.Generic.ICollection<int> DatasourceIds { get; set; }

    public System.Collections.Generic.ICollection<int> Years { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public DateFilterComparisonDto YearsComparison { get; set; }

    public System.Collections.Generic.ICollection<int> SightingTypeSearchGroupIds { get; set; }

}

/// <summary>
/// Measurement or facts for an observation or event.
/// </summary>
public class ExtendedMeasurementOrFact
{

    /// <summary>
    /// An identifier for the MeasurementOrFact (information pertaining
    /// <br/>to measurements, facts, characteristics, or assertions).
    /// <br/>May be a global unique identifier or an identifier specific to the data set.
    /// </summary>
    public string MeasurementID { get; set; }

    /// <summary>
    /// The identifier of the occurrence the measurement or fact refers to.
    /// <br/>If not applicable, it should be left empty.
    /// </summary>
    public string OccurrenceID { get; set; }

    /// <summary>
    /// The nature of the measurement, fact, characteristic, or assertion.
    /// <br/>Recommended best practice is to use a controlled vocabulary.
    /// </summary>
    public string MeasurementType { get; set; }

    /// <summary>
    /// An identifier for the measurementType (global unique identifier, URI).
    /// <br/>The identifier should reference the measurementType in a vocabulary.
    /// </summary>
    public string MeasurementTypeID { get; set; }

    /// <summary>
    /// The value of the measurement, fact, characteristic, or assertion.
    /// </summary>
    public string MeasurementValue { get; set; }

    /// <summary>
    /// An identifier for facts stored in the column measurementValue (global unique identifier, URI).
    /// <br/>This identifier can reference a controlled vocabulary (e.g. for sampling instrument names,
    /// <br/>methodologies, life stages) or reference a methodology paper with a DOI. When the measurementValue
    /// <br/>refers to a value and not to a fact, the measurementvalueID has no meaning and should remain empty.
    /// </summary>
    public string MeasurementValueID { get; set; }

    /// <summary>
    /// The description of the potential error associated with the measurementValue.
    /// </summary>
    public string MeasurementAccuracy { get; set; }

    /// <summary>
    /// The units associated with the measurementValue.
    /// <br/>Recommended best practice is to use the International System of Units (SI).
    /// </summary>
    public string MeasurementUnit { get; set; }

    /// <summary>
    /// An identifier for the measurementUnit (global unique identifier, URI).
    /// <br/>The identifier should reference the measurementUnit in a vocabulary.
    /// </summary>
    public string MeasurementUnitID { get; set; }

    /// <summary>
    /// The date on which the MeasurementOrFact was made. Recommended best
    /// <br/>practice is to use an encoding scheme, such as ISO 8601:2004(E).
    /// </summary>
    public string MeasurementDeterminedDate { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of names of people, groups, or organizations
    /// <br/>who determined the value of the MeasurementOrFact.
    /// </summary>
    public string MeasurementDeterminedBy { get; set; }

    /// <summary>
    /// A description of or reference to (publication, URI) the method or protocol used
    /// <br/>to determine the measurement, fact, characteristic, or assertion.
    /// </summary>
    public string MeasurementMethod { get; set; }

    /// <summary>
    /// Comments or notes accompanying the MeasurementOrFact.
    /// </summary>
    public string MeasurementRemarks { get; set; }

}


public enum ExternalSystemIdDto
{

    Artportalen = 0,

    DarwinCore = 1,

    SwedishSpeciesObservationService = 2,

}


public class ExternalSystemMappingDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public ExternalSystemIdDto Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public System.Collections.Generic.ICollection<ExternalSystemMappingFieldDto> Mappings { get; set; }

}


public class ExternalSystemMappingFieldDto
{

    /// <summary>
    /// The key in the external system.
    /// </summary>
    public string Key { get; set; }

    public string Description { get; set; }

    public System.Collections.Generic.ICollection<ExternalSystemMappingValueDto> Values { get; set; }

}


public class ExternalSystemMappingValueDto
{

    /// <summary>
    /// Value in data provider.
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Id in SOS (Species Observation System).
    /// </summary>
    public int SosId { get; set; }

}

/// <summary>
/// File object
/// </summary>
public class File
{

    /// <summary>
    /// Name Of file
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Date time when file was created
    /// </summary>
    public System.DateTimeOffset Created { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Download URL.
    /// </summary>
    public string DownloadUrl { get; set; }

}

/// <summary>
/// Generalization filter.
/// </summary>
public class GeneralizationFilterDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SensitiveGeneralizationFilterDto SensitiveGeneralizationFilter { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public PublicGeneralizationFilterDto PublicGeneralizationFilter { get; set; }

}


public class GeoCoordinate
{

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double? Z { get; set; }

}


public class GeoGridCellDto
{

    public long? ObservationsCount { get; set; }

    public long? TaxaCount { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Zoom { get; set; }

    public LatLonBoundingBoxDto BoundingBox { get; set; }

}


public class GeoGridMetricResultDto
{

    public LatLonBoundingBoxDto BoundingBox { get; set; }

    public System.Collections.Generic.ICollection<GridCellDto> GridCells { get; set; }

    public int GridCellCount { get; set; }

    public int GridCellSizeInMeters { get; set; }

    public XYBoundingBoxDto Sweref99TmBoundingBox { get; set; }

}


public class GeoGridResultDto
{

    public LatLonBoundingBoxDto BoundingBox { get; set; }

    public int Zoom { get; set; }

    public int GridCellCount { get; set; }

    public System.Collections.Generic.ICollection<GeoGridCellDto> GridCells { get; set; }

    public long TotalGridCellCount { get; set; }

}


public class GeoGridTileTaxaCellDto
{

    public LatLonBoundingBoxDto BoundingBox { get; set; }

    public string GeoTile { get; set; }

    public int Zoom { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public System.Collections.Generic.ICollection<GeoGridTileTaxonObservationCountDto> Taxa { get; set; }

}


public class GeoGridTileTaxonObservationCountDto
{

    public int TaxonId { get; set; }

    public int ObservationCount { get; set; }

}


public class GeoGridTileTaxonPageResultDto
{

    public bool HasMorePages { get; set; }

    /// <summary>
    /// The GeoTile page key.
    /// </summary>
    public string NextGeoTilePage { get; set; }

    /// <summary>
    /// The TaxonId page key.
    /// </summary>
    public int? NextTaxonIdPage { get; set; }

    public System.Collections.Generic.ICollection<GeoGridTileTaxaCellDto> GridCells { get; set; }

}


public class GeoLocation
{

    public double Latitude { get; set; }

    public double Longitude { get; set; }

}

/// <summary>
/// Geometry filter.
/// </summary>
public class GeographicsFilterDto
{

    /// <summary>
    /// Area filter
    /// </summary>
    public System.Collections.Generic.ICollection<AreaFilterDto> Areas { get; set; }

    public LatLonBoundingBoxDto BoundingBox { get; set; }

    /// <summary>
    /// If true, observations that are outside Geometries polygons
    /// <br/>but close enough when disturbance sensitivity of species
    /// <br/>are considered, will be included in the result.
    /// </summary>
    public bool ConsiderDisturbanceRadius { get; set; }

    /// <summary>
    /// If true, observations that are outside Geometries polygons
    /// <br/>but possibly inside when accuracy (coordinateUncertaintyInMeters)
    /// <br/>of observation is considered, will be included in the result.
    /// </summary>
    public bool ConsiderObservationAccuracy { get; set; }

    /// <summary>
    /// If Geometries is of point type, this property must be set to a value greater than 0.
    /// <br/>Observations inside circle (center=point, radius=MaxDistanceFromPoint) will be returned.
    /// </summary>
    public double? MaxDistanceFromPoint { get; set; }

    /// <summary>
    /// Point or polygon geometry used for search.
    /// <br/>If the geometry is a point, then MaxDistanceFromPoint is also used in search.
    /// </summary>
    public System.Collections.Generic.ICollection<GeoJsonGeometry> Geometries { get; set; }

    /// <summary>
    /// Filter on location id/s. Only observations with passed location id/s this will be returned
    /// </summary>
    public System.Collections.Generic.ICollection<string> LocationIds { get; set; }

    /// <summary>
    /// Location name wild card filter
    /// </summary>
    public string LocationNameFilter { get; set; }

    /// <summary>
    /// Limit observation accuracy. Only observations with accuracy less than this will be returned
    /// </summary>
    public int? MaxAccuracy { get; set; }

}

/// <summary>
/// Geological information, such as stratigraphy, that qualifies a region or place.
/// </summary>
public class GeologicalContext
{

    /// <summary>
    /// The full name of the lithostratigraphic bed from which
    /// <br/>the cataloged item was collected.
    /// </summary>
    public string Bed { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic
    /// <br/>age or lowest chronostratigraphic stage attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string EarliestAgeOrLowestStage { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic eon
    /// <br/>or lowest chrono-stratigraphic eonothem or the informal
    /// <br/>name ("Precambrian") attributable to the stratigraphic
    /// <br/>horizon from which the cataloged item was collected.
    /// </summary>
    public string EarliestEonOrLowestEonothem { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic
    /// <br/>epoch or lowest chronostratigraphic series attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string EarliestEpochOrLowestSeries { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic
    /// <br/>era or lowest chronostratigraphic erathem attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string EarliestEraOrLowestErathem { get; set; }

    /// <summary>
    /// Use to link a dwc:GeologicalContext instance to chronostratigraphic time
    /// <br/>periods at the lowest possible level in a standardized hierarchy. Use this
    /// <br/>property to point to the earliest possible geological time period from which
    /// <br/>the cataloged item was collected.
    /// </summary>
    public string EarliestGeochronologicalEra { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic
    /// <br/>period or lowest chronostratigraphic system attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string EarliestPeriodOrLowestSystem { get; set; }

    /// <summary>
    /// The full name of the lithostratigraphic formation from
    /// <br/>which the cataloged item was collected.
    /// </summary>
    public string Formation { get; set; }

    /// <summary>
    /// An identifier for the set of information associated
    /// <br/>with a GeologicalContext (the location within a geological
    /// <br/>context, such as stratigraphy). May be a global unique
    /// <br/>identifier or an identifier specific to the data set.
    /// </summary>
    public string GeologicalContextId { get; set; }

    /// <summary>
    /// The full name of the lithostratigraphic group from
    /// <br/>which the cataloged item was collected.
    /// </summary>
    public string Group { get; set; }

    /// <summary>
    /// The full name of the highest possible geological
    /// <br/>biostratigraphic zone of the stratigraphic horizon
    /// <br/>from which the cataloged item was collected.
    /// </summary>
    public string HighestBiostratigraphicZone { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic
    /// <br/>age or highest chronostratigraphic stage attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string LatestAgeOrHighestStage { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic eon
    /// <br/>or highest chrono-stratigraphic eonothem or the informal
    /// <br/>name ("Precambrian") attributable to the stratigraphic
    /// <br/>horizon from which the cataloged item was collected.
    /// </summary>
    public string LatestEonOrHighestEonothem { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic
    /// <br/>epoch or highest chronostratigraphic series attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string LatestEpochOrHighestSeries { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic
    /// <br/>era or highest chronostratigraphic erathem attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string LatestEraOrHighestErathem { get; set; }

    /// <summary>
    /// Use to link a dwc:GeologicalContext instance to chronostratigraphic time periods at the lowest possible
    /// <br/>level in a standardized hierarchy. Use this property to point to the latest possible geological time period
    /// <br/>from which the cataloged item was collected.
    /// </summary>
    public string LatestGeochronologicalEra { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic
    /// <br/>period or highest chronostratigraphic system attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string LatestPeriodOrHighestSystem { get; set; }

    /// <summary>
    /// The combination of all litho-stratigraphic names for
    /// <br/>the rock from which the cataloged item was collected.
    /// </summary>
    public string LithostratigraphicTerms { get; set; }

    /// <summary>
    /// The full name of the lowest possible geological
    /// <br/>biostratigraphic zone of the stratigraphic horizon
    /// <br/>from which the cataloged item was collected.
    /// </summary>
    public string LowestBiostratigraphicZone { get; set; }

    /// <summary>
    /// The full name of the lithostratigraphic member from
    /// <br/>which the cataloged item was collected.
    /// </summary>
    public string Member { get; set; }

}


public class GridCellDto
{

    /// <summary>
    /// Grid cell Id.
    /// </summary>
    public string Id { get; set; }

    public LatLonBoundingBoxDto BoundingBox { get; set; }

    /// <summary>
    /// Number of observations in cell
    /// </summary>
    public long? ObservationsCount { get; set; }

    public XYBoundingBoxDto MetricBoundingBox { get; set; }

    /// <summary>
    /// Count of different taxa
    /// </summary>
    public long? TaxaCount { get; set; }

}


public class IGeoShape
{

    public string Type { get; set; }
}

/// <summary>
/// Identification information about an species observation.
/// </summary>
public class Identification
{

    /// <summary>
    /// Confirmed by.
    /// </summary>
    public string ConfirmedBy { get; set; }

    /// <summary>
    /// Date of confirmation.
    /// </summary>
    public string ConfirmedDate { get; set; }

    /// <summary>
    /// The date on which the subject was identified as
    /// <br/>representing the Taxon. Recommended best practice is
    /// <br/>to use an encoding scheme, such as ISO 8601:2004(E).
    /// </summary>
    public string DateIdentified { get; set; }

    /// <summary>
    /// An identifier for the Identification (the body of
    /// <br/>information associated with the assignment of a scientific
    /// <br/>name). May be a global unique identifier or an identifier
    /// <br/>specific to the data set.
    /// </summary>
    public string IdentificationId { get; set; }

    /// <summary>
    /// A brief phrase or a standard term ("cf.", "aff.") to
    /// <br/>express the determiner's doubts about the Identification.
    /// </summary>
    public string IdentificationQualifier { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of references
    /// <br/>(publication, global unique identifier, URI) used in
    /// <br/>the Identification.
    /// </summary>
    public string IdentificationReferences { get; set; }

    /// <summary>
    /// Comments or notes about the Identification.
    /// <br/>Contains for example information about that
    /// <br/>the observer is uncertain about which species
    /// <br/>that has been observed.
    /// </summary>
    public string IdentificationRemarks { get; set; }

    /// <summary>
    ///     True if sighting is validated.
    /// <br/>This property is deprecated and replaced by the Verified property.
    /// </summary>
    [System.Obsolete]
    public bool Validated { get; set; }

    /// <summary>
    /// True if sighting is verified (validated).
    /// </summary>
    public bool Verified { get; set; }

    public VocabularyValue ValidationStatus { get; set; }

    public VocabularyValue VerificationStatus { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of names of people,
    /// <br/>groups, or organizations who assigned the Taxon to the
    /// <br/>subject.
    /// </summary>
    public string IdentifiedBy { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of nomenclatural
    /// <br/>types (type status, typified scientific name, publication)
    /// <br/>applied to the subject.
    /// </summary>
    public string TypeStatus { get; set; }

    /// <summary>
    /// True if determination is uncertain.
    /// </summary>
    public bool UncertainIdentification { get; set; }

    public VocabularyValue DeterminationMethod { get; set; }

    /// <summary>
    /// A list(concatenated and separated) of names of people,
    /// <br/>who verified the observation.
    /// </summary>
    public string VerifiedBy { get; set; }

}

/// <summary>
/// Id value dto
/// </summary>
public class Int32IdValueDto
{

    /// <summary>
    /// Id of item
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The value.
    /// </summary>
    public string Value { get; set; }

}


public class LatLonBoundingBox
{

    public LatLonCoordinate BottomRight { get; set; }

    public LatLonCoordinate TopLeft { get; set; }

}


public class LatLonBoundingBoxDto
{

    public LatLonCoordinateDto BottomRight { get; set; }

    public LatLonCoordinateDto TopLeft { get; set; }

}


public class LatLonCoordinate
{

    public double Latitude { get; set; }

    public double Longitude { get; set; }

}


public class LatLonCoordinateDto
{

    public double Latitude { get; set; }

    public double Longitude { get; set; }

}

/// <summary>
/// Location information for a species observation.
/// </summary>
public class Location
{

    public Area Atlas5x5 { get; set; }

    public Area Atlas10x10 { get; set; }

    public LocationAttributes Attributes { get; set; }

    public VocabularyValue Continent { get; set; }

    /// <summary>
    /// A decimal representation of the precision of the coordinates
    /// <br/>given in the DecimalLatitude and DecimalLongitude.
    /// </summary>
    public double? CoordinatePrecision { get; set; }

    /// <summary>
    /// The horizontal distance (in meters) from the given
    /// <br/>CoordinateX and CoordinateY describing the
    /// <br/>smallest circle containing the whole of the Location.
    /// <br/>Leave the value empty if the uncertainty is unknown, cannot
    /// <br/>be estimated, or is not applicable (because there are
    /// <br/>no coordinates). Zero is not a valid value for this term.
    /// </summary>
    public int? CoordinateUncertaintyInMeters { get; set; }

    public VocabularyValue Country { get; set; }

    /// <summary>
    /// The standard code for the country in which the
    /// <br/>Location occurs.
    /// <br/>Recommended best practice is to use ISO 3166-1-alpha-2
    /// <br/>country codes.
    /// </summary>
    public string CountryCode { get; set; }

    public Area CountryRegion { get; set; }

    public Area County { get; set; }

    public Area Municipality { get; set; }

    public Area Parish { get; set; }

    public Area Province { get; set; }

    /// <summary>
    /// The geographic latitude (in decimal degrees, using
    /// <br/>the spatial reference system given in geodeticDatum)
    /// <br/>of the geographic center of a Location. Positive values
    /// <br/>are north of the Equator, negative values are south of it.
    /// <br/>Legal values lie between -90 and 90, inclusive.
    /// </summary>
    public double? DecimalLatitude { get; set; }

    /// <summary>
    /// The geographic longitude (in decimal degrees, using
    /// <br/>the spatial reference system given in geodeticDatum)
    /// <br/>of the geographic center of a Location. Positive
    /// <br/>values are east of the Greenwich Meridian, negative
    /// <br/>values are west of it. Legal values lie between -180
    /// <br/>and 180, inclusive.
    /// </summary>
    public double? DecimalLongitude { get; set; }

    /// <summary>
    /// X coordinate in ETRS89.
    /// </summary>
    public double? Etrs89X { get; set; }

    /// <summary>
    /// Y coordinate in ETRS89.
    /// </summary>
    public double? Etrs89Y { get; set; }

    /// <summary>
    /// The ratio of the area of the footprint (footprintWKT)
    /// <br/>to the area of the true (original, or most specific)
    /// <br/>spatial representation of the Location. Legal values are
    /// <br/>0, greater than or equal to 1, or undefined. A value of
    /// <br/>1 is an exact match or 100% overlap. A value of 0 should
    /// <br/>be used if the given footprint does not completely contain
    /// <br/>the original representation. The footprintSpatialFit is
    /// <br/>undefined (and should be left blank) if the original
    /// <br/>representation is a point and the given georeference is
    /// <br/>not that same point. If both the original and the given
    /// <br/>georeference are the same point, the footprintSpatialFit
    /// <br/>is 1.
    /// </summary>
    public string FootprintSpatialFit { get; set; }

    /// <summary>
    /// A Well-Known Text (WKT) representation of the Spatial
    /// <br/>Reference System (SRS) for the footprintWKT of the
    /// <br/>Location. Do not use this term to describe the SRS of
    /// <br/>the decimalLatitude and decimalLongitude, even if it is
    /// <br/>the same as for the footprintWKT - use the geodeticDatum
    /// <br/>instead.
    /// </summary>
    public string FootprintSRS { get; set; }

    /// <summary>
    /// A Well-Known Text (WKT) representation of the shape
    /// <br/>(footprint, geometry) that defines the Location.
    /// <br/>A Location may have both a point-radius representation
    /// <br/>(see decimalLatitude) and a footprint representation,
    /// <br/>and they may differ from each other.
    /// </summary>
    public string FootprintWKT { get; set; }

    /// <summary>
    /// The ellipsoid, geodetic datum, or spatial reference
    /// <br/>system (SRS) upon which the geographic coordinates
    /// <br/>given in decimalLatitude and decimalLongitude as based.
    /// <br/>Recommended best practice is use the EPSG code as a
    /// <br/>controlled vocabulary to provide an SRS, if known.
    /// <br/>Otherwise use a controlled vocabulary for the name or
    /// <br/>code of the geodetic datum, if known. Otherwise use a
    /// <br/>controlled vocabulary for the name or code of the
    /// <br/>ellipsoid, if known. If none of these is known, use the
    /// <br/>value "unknown".
    /// </summary>
    public string GeodeticDatum { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of names of people,
    /// <br/>groups, or organizations who determined the georeference
    /// <br/>(spatial representation) the Location.
    /// </summary>
    public string GeoreferencedBy { get; set; }

    /// <summary>
    /// The date on which the Location was georeferenced.
    /// <br/>Recommended best practice is to use an encoding scheme,
    /// <br/>such as ISO 8601:2004(E).
    /// </summary>
    public string GeoreferencedDate { get; set; }

    /// <summary>
    /// A description or reference to the methods used to
    /// <br/>determine the spatial footprint, coordinates, and
    /// <br/>uncertainties.
    /// </summary>
    public string GeoreferenceProtocol { get; set; }

    /// <summary>
    /// Notes or comments about the spatial description
    /// <br/>determination, explaining assumptions made in addition
    /// <br/>or opposition to the those formalized in the method
    /// <br/>referred to in georeferenceProtocol.
    /// </summary>
    public string GeoreferenceRemarks { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of maps, gazetteers,
    /// <br/>or other resources used to georeference the Location,
    /// <br/>described specifically enough to allow anyone in the
    /// <br/>future to use the same resources.
    /// </summary>
    public string GeoreferenceSources { get; set; }

    /// <summary>
    /// A categorical description of the extent to which the
    /// <br/>georeference has been verified to represent the best
    /// <br/>possible spatial description. Recommended best practice
    /// <br/>is to use a controlled vocabulary.
    /// </summary>
    public string GeoreferenceVerificationStatus { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of geographic
    /// <br/>names less specific than the information captured
    /// <br/>in the locality term.
    /// </summary>
    public string HigherGeography { get; set; }

    /// <summary>
    /// An identifier for the geographic region within which
    /// <br/>the Location occurred.
    /// <br/>Recommended best practice is to use an
    /// <br/>persistent identifier from a controlled vocabulary
    /// <br/>such as the Getty Thesaurus of Geographic Names.
    /// </summary>
    public string HigherGeographyId { get; set; }

    /// <summary>
    /// The name of the island on or near which the Location occurs.
    /// <br/>Recommended best practice is to use a controlled
    /// <br/>vocabulary such as the Getty Thesaurus of Geographic Names.
    /// </summary>
    public string Island { get; set; }

    /// <summary>
    /// The name of the island group in which the Location occurs.
    /// <br/>Recommended best practice is to use a controlled
    /// <br/>vocabulary such as the Getty Thesaurus of Geographic Names.
    /// </summary>
    public string IslandGroup { get; set; }

    /// <summary>
    /// The specific description of the place. Less specific
    /// <br/>geographic information can be provided in other
    /// <br/>geographic terms (higherGeography, continent, country,
    /// <br/>stateProvince, county, municipality, waterBody, island,
    /// <br/>islandGroup). This term may contain information modified
    /// <br/>from the original to correct perceived errors or
    /// <br/>standardize the description.
    /// </summary>
    public string Locality { get; set; }

    /// <summary>
    /// Information about the source of this Location information.
    /// <br/>Could be a publication (gazetteer), institution,
    /// <br/>or team of individuals.
    /// </summary>
    public string LocationAccordingTo { get; set; }

    /// <summary>
    /// An identifier for the set of location information
    /// <br/>(data associated with dcterms:Location).
    /// <br/>May be a global unique identifier or an identifier
    /// <br/>specific to the data set.
    /// </summary>
    public string LocationId { get; set; }

    /// <summary>
    /// Comments or notes about the Location.
    /// </summary>
    public string LocationRemarks { get; set; }

    /// <summary>
    /// The greater depth of a range of depth below
    /// <br/>the local surface, in meters.
    /// </summary>
    public double? MaximumDepthInMeters { get; set; }

    /// <summary>
    /// The greater distance in a range of distance from a
    /// <br/>reference surface in the vertical direction, in meters.
    /// <br/>Use positive values for locations above the surface,
    /// <br/>negative values for locations below. If depth measures
    /// <br/>are given, the reference surface is the location given
    /// <br/>by the depth, otherwise the reference surface is the
    /// <br/>location given by the elevation.
    /// </summary>
    public double? MaximumDistanceAboveSurfaceInMeters { get; set; }

    /// <summary>
    /// The upper limit of the range of elevation (altitude,
    /// <br/>usually above sea level), in meters.
    /// </summary>
    public double? MaximumElevationInMeters { get; set; }

    /// <summary>
    /// The lesser depth of a range of depth below the
    /// <br/>local surface, in meters.
    /// </summary>
    public double? MinimumDepthInMeters { get; set; }

    /// <summary>
    /// The lesser distance in a range of distance from a
    /// <br/>reference surface in the vertical direction, in meters.
    /// <br/>Use positive values for locations above the surface,
    /// <br/>negative values for locations below.
    /// <br/>If depth measures are given, the reference surface is
    /// <br/>the location given by the depth, otherwise the reference
    /// <br/>surface is the location given by the elevation.
    /// </summary>
    public double? MinimumDistanceAboveSurfaceInMeters { get; set; }

    /// <summary>
    /// The lower limit of the range of elevation (altitude,
    /// <br/>usually above sea level), in meters.
    /// </summary>
    public double? MinimumElevationInMeters { get; set; }

    /// <summary>
    /// The ratio of the area of the point-radius
    /// <br/>(decimalLatitude, decimalLongitude,
    /// <br/>coordinateUncertaintyInMeters) to the area of the true
    /// <br/>(original, or most specific) spatial representation of
    /// <br/>the Location. Legal values are 0, greater than or equal
    /// <br/>to 1, or undefined. A value of 1 is an exact match or
    /// <br/>100% overlap. A value of 0 should be used if the given
    /// <br/>point-radius does not completely contain the original
    /// <br/>representation. The pointRadiusSpatialFit is undefined
    /// <br/>(and should be left blank) if the original representation
    /// <br/>is a point without uncertainty and the given georeference
    /// <br/>is not that same point (without uncertainty). If both the
    /// <br/>original and the given georeference are the same point,
    /// <br/>the pointRadiusSpatialFit is 1.
    /// </summary>
    public string PointRadiusSpatialFit { get; set; }

    /// <summary>
    /// X coordinate in SWEREF99 TM.
    /// </summary>
    public double? Sweref99TmX { get; set; }

    /// <summary>
    /// Y coordinate in SWEREF99 TM.
    /// </summary>
    public double? Sweref99TmY { get; set; }

    /// <summary>
    /// Location type. 1=Point, 2=Polygon.
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// The verbatim original spatial coordinates of the Location.
    /// <br/>The coordinate ellipsoid, geodeticDatum, or full
    /// <br/>Spatial Reference System (SRS) for these coordinates
    /// <br/>should be stored in verbatimSRS and the coordinate
    /// <br/>system should be stored in verbatimCoordinateSystem.
    /// </summary>
    public string VerbatimCoordinates { get; set; }

    /// <summary>
    /// The spatial coordinate system for the verbatimLatitude
    /// <br/>and verbatimLongitude or the verbatimCoordinates of the
    /// <br/>Location.
    /// <br/>Recommended best practice is to use a controlled vocabulary.
    /// </summary>
    public string VerbatimCoordinateSystem { get; set; }

    /// <summary>
    /// The original description of the
    /// <br/>depth below the local surface.
    /// </summary>
    public string VerbatimDepth { get; set; }

    /// <summary>
    /// The original description of the elevation (altitude,
    /// <br/>usually above sea level) of the Location.
    /// </summary>
    public string VerbatimElevation { get; set; }

    /// <summary>
    /// The verbatim original latitude of the Location.
    /// <br/>The coordinate ellipsoid, geodeticDatum, or full
    /// <br/>Spatial Reference System (SRS) for these coordinates
    /// <br/>should be stored in verbatimSRS and the coordinate
    /// <br/>system should be stored in verbatimCoordinateSystem.
    /// </summary>
    public string VerbatimLatitude { get; set; }

    /// <summary>
    /// The original textual description of the place.
    /// </summary>
    public string VerbatimLocality { get; set; }

    /// <summary>
    /// The verbatim original longitude of the Location.
    /// <br/>The coordinate ellipsoid, geodeticDatum, or full
    /// <br/>Spatial Reference System (SRS) for these coordinates
    /// <br/>should be stored in verbatimSRS and the coordinate
    /// <br/>system should be stored in verbatimCoordinateSystem.
    /// </summary>
    public string VerbatimLongitude { get; set; }

    /// <summary>
    /// The ellipsoid, geodetic datum, or spatial reference
    /// <br/>system (SRS) upon which coordinates given in
    /// <br/>verbatimLatitude and verbatimLongitude, or
    /// <br/>verbatimCoordinates are based.
    /// <br/>Recommended best practice is use the EPSG code as
    /// <br/>a controlled vocabulary to provide an SRS, if known.
    /// <br/>Otherwise use a controlled vocabulary for the name or
    /// <br/>code of the geodetic datum, if known.
    /// <br/>Otherwise use a controlled vocabulary for the name or
    /// <br/>code of the ellipsoid, if known. If none of these is
    /// <br/>known, use the value "unknown".
    /// </summary>
    public string VerbatimSRS { get; set; }

    /// <summary>
    /// The name of the water body in which the Location occurs.
    /// <br/>Recommended best practice is to use a controlled
    /// <br/>vocabulary such as the Getty Thesaurus of Geographic Names.
    /// </summary>
    public string WaterBody { get; set; }

}

/// <summary>
/// Location attributes.
/// </summary>
public class LocationAttributes
{

    /// <summary>
    /// Special handling of Kalmar/Öland.
    /// </summary>
    public string CountyPartIdByCoordinate { get; set; }

    /// <summary>
    /// External Id of site
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// Id of project
    /// </summary>
    public int? ProjectId { get; set; }

    /// <summary>
    /// Spacial handling of Lappland.
    /// </summary>
    public string ProvincePartIdByCoordinate { get; set; }

    /// <summary>
    /// The original municipality value from data provider.
    /// </summary>
    public string VerbatimMunicipality { get; set; }

    /// <summary>
    /// The original StateProvince value from data provider.
    /// </summary>
    public string VerbatimProvince { get; set; }

}


public class LocationDto
{

    public Int32IdValueDto Continent { get; set; }

    /// <summary>
    /// A decimal representation of the precision of the coordinates
    /// <br/>given in the DecimalLatitude and DecimalLongitude.
    /// </summary>
    public double? CoordinatePrecision { get; set; }

    /// <summary>
    /// The horizontal distance (in meters) from the given
    /// <br/>CoordinateX and CoordinateY describing the
    /// <br/>smallest circle containing the whole of the Location.
    /// <br/>Leave the value empty if the uncertainty is unknown, cannot
    /// <br/>be estimated, or is not applicable (because there are
    /// <br/>no coordinates). Zero is not a valid value for this term.
    /// </summary>
    public int? CoordinateUncertaintyInMeters { get; set; }

    public Int32IdValueDto Country { get; set; }

    public StringIdValueDto CountryRegion { get; set; }

    /// <summary>
    /// The standard code for the country in which the
    /// <br/>Location occurs.
    /// <br/>Recommended best practice is to use ISO 3166-1-alpha-2
    /// <br/>country codes.
    /// </summary>
    public string CountryCode { get; set; }

    public StringIdValueDto County { get; set; }

    public StringIdValueDto Municipality { get; set; }

    public StringIdValueDto Parish { get; set; }

    public StringIdValueDto Province { get; set; }

    /// <summary>
    /// The geographic latitude (in decimal degrees, using
    /// <br/>the spatial reference system given in geodeticDatum)
    /// <br/>of the geographic center of a Location. Positive values
    /// <br/>are north of the Equator, negative values are south of it.
    /// <br/>Legal values lie between -90 and 90, inclusive.
    /// </summary>
    public double? DecimalLatitude { get; set; }

    /// <summary>
    /// The geographic longitude (in decimal degrees, using
    /// <br/>the spatial reference system given in geodeticDatum)
    /// <br/>of the geographic center of a Location. Positive
    /// <br/>values are east of the Greenwich Meridian, negative
    /// <br/>values are west of it. Legal values lie between -180
    /// <br/>and 180, inclusive.
    /// </summary>
    public double? DecimalLongitude { get; set; }

    /// <summary>
    /// External Id of site
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// The specific description of the place. Less specific
    /// <br/>geographic information can be provided in other
    /// <br/>geographic terms (higherGeography, continent, country,
    /// <br/>stateProvince, county, municipality, waterBody, island,
    /// <br/>islandGroup). This term may contain information modified
    /// <br/>from the original to correct perceived errors or
    /// <br/>standardize the description.
    /// </summary>
    public string Locality { get; set; }

    /// <summary>
    /// Information about the source of this Location information.
    /// <br/>Could be a publication (gazetteer), institution,
    /// <br/>or team of individuals.
    /// </summary>
    public string LocationAccordingTo { get; set; }

    /// <summary>
    /// An identifier for the set of location information
    /// <br/>(data associated with dcterms:Location).
    /// <br/>May be a global unique identifier or an identifier
    /// <br/>specific to the data set.
    /// </summary>
    public string LocationId { get; set; }

    /// <summary>
    /// Comments or notes about the Location.
    /// </summary>
    public string LocationRemarks { get; set; }

    public PointGeoShape Point { get; set; }

    public PolygonGeoShape PointWithBuffer { get; set; }

    public PolygonGeoShape PointWithDisturbanceBuffer { get; set; }

    /// <summary>
    /// Id of project
    /// </summary>
    public int? ProjectId { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public LocationType Type { get; set; }

}


public class LocationSearchResultDto
{

    /// <summary>
    /// County
    /// </summary>
    public string County { get; set; }

    /// <summary>
    /// Id of location
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Location latitude in WGS 84
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Location longitude in WGS 84
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Municipality
    /// </summary>
    public string Municipality { get; set; }

    /// <summary>
    /// Name of location
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Parish
    /// </summary>
    public string Parish { get; set; }

}


public enum LocationType
{

    Unknown = 0,

    Point = 1,

    Polygon = 2,

}

/// <summary>
/// A physical result of a sampling (or subsampling) event. In biological collections, the material sample is typically
/// <br/>collected,
/// <br/>and either preserved or destructively processed.
/// </summary>
public class MaterialSample
{

    /// <summary>
    /// An identifier for the MaterialSample (as opposed to a particular digital record of the material sample).
    /// <br/>In the absence of a persistent global unique identifier, construct one from a combination of identifiers in the
    /// <br/>record
    /// <br/>that will most closely make the materialSampleID globally unique.
    /// </summary>
    public string MaterialSampleId { get; set; }

}


public class Measuring
{

    /// <summary>
    /// Value for measured weather variable.
    /// </summary>
    public double? Value { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Unit Unit { get; set; }

}


public enum MetricCoordinateSys
{

    SWEREF99_TM = 0,

    ETRS89 = 1,

}

/// <summary>
/// Filter for observation Modified date
/// </summary>
public class ModifiedDateFilterDto
{

    /// <summary>
    /// Changed from
    /// </summary>
    public System.DateTimeOffset? From { get; set; }

    /// <summary>
    /// Changed tp
    /// </summary>
    public System.DateTimeOffset? To { get; set; }

}

/// <summary>
/// Simple Multimedia extension
/// <br/>http://rs.gbif.org/extension/gbif/1.0/multimedia.xml
/// </summary>
public class Multimedia
{

    public System.Collections.Generic.ICollection<MultimediaComment> Comments { get; set; }

    public string Type { get; set; }

    public string Format { get; set; }

    public string Identifier { get; set; }

    public string References { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string Created { get; set; }

    public string Creator { get; set; }

    public string Contributor { get; set; }

    public string Publisher { get; set; }

    public string Audience { get; set; }

    public string Source { get; set; }

    public string License { get; set; }

    public string RightsHolder { get; set; }

    public string DatasetID { get; set; }

}

/// <summary>
/// Multimedia comment
/// </summary>
public class MultimediaComment
{

    /// <summary>
    /// Media comment
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// User making the comment
    /// </summary>
    public string CommentBy { get; set; }

    /// <summary>
    /// Media comment cration time
    /// </summary>
    public System.DateTimeOffset? Created { get; set; }

}

/// <summary>
/// Result returned by paged query
/// </summary>
public class PagedResultDto<T>
{

    /// <summary>
    /// Ignores the specified number of items and returns a sequence starting at the item after the last skipped item (if
    /// <br/>any)
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Returns a sequence containing up to the specified number of items. Anything after the count is ignored
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Total number of records matching the query
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Paged records
    /// </summary>
    public List<T> Records { get; set; }

}

/// <summary>
/// Information about a species observation.
/// </summary>
public class Observation
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public DiffusionStatus DiffusionStatus { get; set; }

    /// <summary>
    /// Indicates whether the location is generalized.
    /// </summary>
    public bool IsGeneralized { get; set; }

    /// <summary>
    /// Indicates whether there exists a generalized location in other index.
    /// </summary>
    public bool HasGeneralizedObservationInOtherIndex { get; set; }

    public Event Event { get; set; }

    public DataStewardshipInfo DataStewardship { get; set; }

    public GeologicalContext GeologicalContext { get; set; }

    public Identification Identification { get; set; }

    public Location Location { get; set; }

    public MaterialSample MaterialSample { get; set; }

    public Occurrence Occurrence { get; set; }

    public Organism Organism { get; set; }

    public Taxon Taxon { get; set; }

    public VocabularyValue AccessRights { get; set; }

    public VocabularyValue BasisOfRecord { get; set; }

    /// <summary>
    /// A bibliographic reference for the resource as a statement
    /// <br/>indicating how this record should be cited (attributed)
    /// <br/>when used.
    /// <br/>Recommended practice is to include sufficient
    /// <br/>bibliographic detail to identify the resource as
    /// <br/>unambiguously as possible.
    /// </summary>
    public string BibliographicCitation { get; set; }

    /// <summary>
    /// The name, acronym, coden, or initialism identifying the
    /// <br/>collection or data set from which the record was derived.
    /// </summary>
    public string CollectionCode { get; set; }

    /// <summary>
    /// An identifier for the collection or dataset from which
    /// <br/>the record was derived.
    /// <br/>For physical specimens, the recommended best practice is
    /// <br/>to use the identifier in a collections registry such as
    /// <br/>the Biodiversity Collections Index
    /// <br/>(http://www.biodiversitycollectionsindex.org/).
    /// </summary>
    public string CollectionId { get; set; }

    /// <summary>
    /// Actions taken to make the shared data less specific or
    /// <br/>complete than in its original form.
    /// <br/>Suggests that alternative data of higher quality
    /// <br/>may be available on request.
    /// </summary>
    public string DataGeneralizations { get; set; }

    /// <summary>
    /// Data provider id.
    /// </summary>
    public int DataProviderId { get; set; }

    /// <summary>
    /// An identifier for the set of data.
    /// <br/>May be a global unique identifier or an identifier
    /// <br/>specific to a collection or institution.
    /// </summary>
    public string DatasetId { get; set; }

    /// <summary>
    /// The name identifying the data set
    /// <br/>from which the record was derived.
    /// </summary>
    public string DatasetName { get; set; }

    /// <summary>
    /// A list of additional measurements, facts, characteristics, or assertions about the record.
    /// <br/>Meant to provide a mechanism for structured content.
    /// <br/>Recommended best practice is to use a key:value encoding schema for a data interchange format such as JSON.
    /// </summary>
    public string DynamicProperties { get; set; }

    /// <summary>
    /// Unique id.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Additional information that exists, but that has
    /// <br/>not been shared in the given record.
    /// </summary>
    public string InformationWithheld { get; set; }

    /// <summary>
    /// An identifier for the institution having custody of the object(s) or information referred to in the record.
    /// </summary>
    public string InstitutionId { get; set; }

    public VocabularyValue InstitutionCode { get; set; }

    /// <summary>
    /// A language of the resource.
    /// <br/>Recommended best practice is to use a controlled
    /// <br/>vocabulary such as RFC 4646 [RFC4646].
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// A legal document giving official permission to do something with the resource.
    /// </summary>
    public string License { get; set; }

    /// <summary>
    /// The most recent date-time on which the resource was changed (UTC).
    /// <br/>For Darwin Core, recommended best practice is to use an
    /// <br/>encoding scheme, such as ISO 8601:2004(E).
    /// </summary>
    public System.DateTimeOffset? Modified { get; set; }

    /// <summary>
    /// The name (or acronym) in use by the institution having
    /// <br/>ownership of the object(s) or information referred
    /// <br/>to in the record.
    /// </summary>
    public string OwnerInstitutionCode { get; set; }

    /// <summary>
    /// Private Collection.
    /// </summary>
    public string PrivateCollection { get; set; }

    /// <summary>
    /// Indicates whether the observation is protected.
    /// <br/>This property is deprecated and replaced by the Sensitive property.
    /// </summary>
    [System.Obsolete]
    public bool Protected { get; set; }

    /// <summary>
    /// Indicates whether the observation is sensitive and therefore protected.
    /// </summary>
    public bool Sensitive { get; set; }

    /// <summary>
    /// Public Collection.
    /// </summary>
    public string PublicCollection { get; set; }

    /// <summary>
    /// A related resource that is referenced, cited,
    /// <br/>or otherwise pointed to by the described resource.
    /// </summary>
    public string References { get; set; }

    /// <summary>
    /// A person or organization owning or
    /// <br/>managing rights over the resource.
    /// </summary>
    public string RightsHolder { get; set; }

    /// <summary>
    /// Species collection label.
    /// </summary>
    public string SpeciesCollectionLabel { get; set; }

    public VocabularyValue Type { get; set; }

    public ArtportalenInternal ArtportalenInternal { get; set; }

    /// <summary>
    /// Measurement or facts associated with the observation.
    /// </summary>
    public System.Collections.Generic.ICollection<ExtendedMeasurementOrFact> MeasurementOrFacts { get; set; }

    /// <summary>
    /// Projects from Artportalen associated with the observation.
    /// </summary>
    public System.Collections.Generic.ICollection<Project> Projects { get; set; }

    public ProjectsSummary ProjectsSummary { get; set; }

    /// <summary>
    /// The date the observation was created (UTC).
    /// </summary>
    public System.DateTimeOffset Created { get; set; }

    public DataQuality DataQuality { get; set; }

}

/// <summary>
/// Result returned by paged query. Can contain GeoJSON if requested.
/// </summary>
public class ObservationGeoPagedResultDto
{

    /// <summary>
    /// Ignores the specified number of items and returns a sequence starting at the item after the last skipped item (if
    /// <br/>any)
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Returns a sequence containing up to the specified number of items. Anything after the count is ignored
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Total number of records matching the query
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Paged records
    /// </summary>
    public System.Collections.Generic.ICollection<Observation> Records { get; set; }

}

/// <summary>
/// Result returned by paged query
/// </summary>
public class ObservationPagedResultDto
{

    /// <summary>
    /// Ignores the specified number of items and returns a sequence starting at the item after the last skipped item (if
    /// <br/>any)
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Returns a sequence containing up to the specified number of items. Anything after the count is ignored
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Total number of records matching the query
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Paged records
    /// </summary>
    public System.Collections.Generic.ICollection<Observation> Records { get; set; }

}

/// <summary>
/// Result returned by scroll query
/// </summary>
public class ObservationScrollResultDto
{

    /// <summary>
    /// True if more pages can be retrieved using the ScrollId; otherwise false.
    /// </summary>
    public bool HasMorePages { get; set; }

    /// <summary>
    /// The scroll id used for retrieving next page of records.
    /// </summary>
    public string ScrollId { get; set; }

    /// <summary>
    /// Returns a sequence containing up to the specified number of items.
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Total number of records matching the query
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Paged records
    /// </summary>
    public System.Collections.Generic.ICollection<Observation> Records { get; set; }

}

/// <summary>
/// Occurrence information about a species observation.
/// </summary>
public class Occurrence
{

    public VocabularyValue Activity { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers
    /// <br/>(publication, global unique identifier, URI) of
    /// <br/>media associated with the Occurrence.
    /// </summary>
    public string AssociatedMedia { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers of
    /// <br/>other Occurrence records and their associations to
    /// <br/>this Occurrence.
    /// </summary>
    public string AssociatedOccurrences { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers
    /// <br/>(publication, bibliographic reference, global unique
    /// <br/>identifier, URI) of literature associated with
    /// <br/>the Occurrence.
    /// </summary>
    public string AssociatedReferences { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers (publication, global unique identifier, URI) 
    /// <br/>of genetic sequence information associated with the Occurrence.
    /// </summary>
    public string AssociatedSequences { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers or
    /// <br/>names of taxa and their associations with the Occurrence.
    /// </summary>
    public string AssociatedTaxa { get; set; }

    public VocabularyValue Behavior { get; set; }

    public VocabularyValue Biotope { get; set; }

    /// <summary>
    /// Description of biotope.
    /// </summary>
    public string BiotopeDescription { get; set; }

    /// <summary>
    /// Bird nest activity.
    /// </summary>
    public int BirdNestActivityId { get; set; }

    /// <summary>
    /// An identifier (preferably unique) for the record
    /// <br/>within the data set or collection.
    /// </summary>
    public string CatalogNumber { get; set; }

    /// <summary>
    /// An int32 identifier (preferably unique) for the record within the data set or collection.
    /// </summary>
    public int CatalogId { get; set; }

    /// <summary>
    /// The current state of a specimen with respect to the
    /// <br/>collection identified in collectionCode or collectionID.
    /// <br/>Recommended best practice is to use a controlled vocabulary.
    /// </summary>
    public string Disposition { get; set; }

    public VocabularyValue EstablishmentMeans { get; set; }

    /// <summary>
    /// The number of individuals represented present
    /// <br/>at the time of the Occurrence.
    /// </summary>
    public string IndividualCount { get; set; }

    /// <summary>
    /// Indicates if this species occurrence is natural or
    /// <br/>if it is a result of human activity.
    /// </summary>
    public bool IsNaturalOccurrence { get; set; }

    /// <summary>
    /// Indicates if this observation is a never found observation.
    /// <br/>"Never found observation" is an observation that says
    /// <br/>that the specified species was not found in a location
    /// <br/>deemed appropriate for the species.
    /// </summary>
    public bool IsNeverFoundObservation { get; set; }

    /// <summary>
    /// Indicates if this observation is a
    /// <br/>not rediscovered observation.
    /// <br/>"Not rediscovered observation" is an observation that says
    /// <br/>that the specified species was not found in a location
    /// <br/>where it has previously been observed.
    /// </summary>
    public bool IsNotRediscoveredObservation { get; set; }

    /// <summary>
    /// Indicates if this observation is a positive observation.
    /// <br/>"Positive observation" is a normal observation indicating
    /// <br/>that a species has been seen at a specified location.
    /// </summary>
    public bool IsPositiveObservation { get; set; }

    public VocabularyValue LifeStage { get; set; }

    /// <summary>
    /// Media associated with the observation
    /// </summary>
    public System.Collections.Generic.ICollection<Multimedia> Media { get; set; }

    /// <summary>
    /// An identifier for the Occurrence (as opposed to a particular digital record of the occurrence).
    /// <br/>In the absence of a persistent global unique identifier, construct one from a combination of
    /// <br/>identifiers in the record that will most closely make the occurrenceID globally unique.
    /// </summary>
    public string OccurrenceId { get; set; }

    /// <summary>
    /// Comments or notes about the Occurrence.
    /// </summary>
    public string OccurrenceRemarks { get; set; }

    public VocabularyValue OccurrenceStatus { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of previous or
    /// <br/>alternate fully qualified catalog numbers or other
    /// <br/>human-used identifiers for the same Occurrence,
    /// <br/>whether in the current or any other data set or collection.
    /// </summary>
    public string OtherCatalogNumbers { get; set; }

    /// <summary>
    /// A number or enumeration value for the quantity of organisms.
    /// <br/>A dwc:organismQuantity must have a corresponding dwc:organismQuantityType.
    /// </summary>
    public string OrganismQuantity { get; set; }

    /// <summary>
    /// Organism quantity used in aggregations
    /// </summary>
    public int? OrganismQuantityAggregation { get; set; }

    /// <summary>
    /// The quantity of organisms as integer. This field is necessary because we want to be able to do range-querys against quantities.
    /// </summary>
    public int? OrganismQuantityInt { get; set; }

    public VocabularyValue OrganismQuantityUnit { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of preparations
    /// <br/>and preservation methods for a specimen.
    /// </summary>
    public string Preparations { get; set; }

    /// <summary>
    /// Information about how protected information about a species is in Sweden.
    /// <br/>This is a value between 1 to 5.
    /// <br/>1 indicates public access and 5 is the highest used security level.
    /// </summary>
    public int SensitivityCategory { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of names of people,
    /// <br/>groups, or organizations responsible for recording the
    /// <br/>original Occurrence. The primary collector or observer,
    /// <br/>especially one who applies a personal identifier
    /// <br/>(recordNumber), should be listed first.
    /// </summary>
    public string RecordedBy { get; set; }

    /// <summary>
    /// An identifier given to the Occurrence at the time it was
    /// <br/>recorded. Often serves as a link between field notes and
    /// <br/>an Occurrence record, such as a specimen collector's number.
    /// </summary>
    public string RecordNumber { get; set; }

    /// <summary>
    /// Name of the person that reported the species observation.
    /// </summary>
    public string ReportedBy { get; set; }

    /// <summary>
    /// Date and time when the species observation was reported (UTC).
    /// </summary>
    public System.DateTimeOffset? ReportedDate { get; set; }

    public VocabularyValue ReproductiveCondition { get; set; }

    public VocabularyValue Sex { get; set; }

    public Substrate Substrate { get; set; }

    /// <summary>
    /// URL to occurrence.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The reported length.
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// The reported weight.
    /// </summary>
    public int? Weight { get; set; }

}

/// <summary>
/// Possible values for the OccurrenceStatus filter.
/// </summary>
public enum OccurrenceStatusFilterValuesDto
{

    Present = 0,

    Absent = 1,

    BothPresentAndAbsent = 2,

}

/// <summary>
/// A particular organism or defined group of organisms considered to be taxonomically homogeneous.
/// </summary>
public class Organism
{

    /// <summary>
    /// An identifier for the Organism instance (as opposed to a particular digital record of the Organism).
    /// <br/>May be a globally unique identifier or an identifier specific to the data set.
    /// </summary>
    public string OrganismId { get; set; }

    /// <summary>
    /// A textual name or label assigned to an Organism instance.
    /// </summary>
    public string OrganismName { get; set; }

    /// <summary>
    /// A description of the kind of Organism instance. Can be used to indicate whether
    /// <br/>the Organism instance represents a discrete organism or if it represents
    /// <br/>a particular type of aggregation..
    /// </summary>
    public string OrganismScope { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of identifiers of other Organisms and their
    /// <br/>associations to this Organism.
    /// </summary>
    public string AssociatedOrganisms { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of previous assignments of names to the Organism.
    /// </summary>
    public string PreviousIdentifications { get; set; }

    /// <summary>
    /// Comments or notes about the Organism instance..
    /// </summary>
    public string OrganismRemarks { get; set; }

}

/// <summary>
/// Export property sets.
/// </summary>
public enum OutputFieldSet
{

    Minimum = 0,

    Extended = 1,

    AllWithValues = 2,

    All = 3,

    None = 4,

}

/// <summary>
/// Response output settings
/// </summary>
public class OutputFilterDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public OutputFieldSet FieldSet { get; set; }

    /// <summary>
    /// This parameter allows you to decide what fields should be returned, using a projection.
    /// <br/>Omit this parameter and you will receive the complete collection of fields.
    /// <br/>For example, to retrieve only basic observation data, specify:
    /// <br/>["event.startDate", "event.endDate", "location.decimalLatitude", "location.decimalLongitude", "location.municipality", "taxon.id", "taxon.scientificName", "occurrence.recordedBy", "occurrence.occurrenceStatus"].
    /// </summary>
    public System.Collections.Generic.ICollection<string> Fields { get; set; }

}

/// <summary>
/// Response output settings
/// </summary>
public class OutputFilterExtendedDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public OutputFieldSet FieldSet { get; set; }

    /// <summary>
    /// This parameter allows you to decide what fields should be returned, using a projection.
    /// <br/>Omit this parameter and you will receive the complete collection of fields.
    /// <br/>For example, to retrieve only basic observation data, specify:
    /// <br/>["event.startDate", "event.endDate", "location.decimalLatitude", "location.decimalLongitude", "location.municipality", "taxon.id", "taxon.scientificName", "occurrence.recordedBy", "occurrence.occurrenceStatus"].
    /// </summary>
    public System.Collections.Generic.ICollection<string> Fields { get; set; }

    /// <summary>
    /// Sort result
    /// </summary>
    public System.Collections.Generic.ICollection<SortOrderDto> SortOrders { get; set; }

}

/// <summary>
/// Output format
/// </summary>
public enum OutputFormatDto
{

    Json = 0,

    GeoJson = 1,

    GeoJsonFlat = 2,

}


public class PointGeoShape
{

    public string Type { get; set; }

    public GeoCoordinate Coordinates { get; set; }

}


public class PolygonGeoShape
{

    public string Type { get; set; }

    public System.Collections.Generic.ICollection<System.Collections.Generic.ICollection<GeoCoordinate>> Coordinates { get; set; }

}

/// <summary>
/// States the precipitation conditions during the survey event.
/// </summary>
public enum Precipitation
{

    DryWeather = 0,

    LightRain = 1,

    ModerateRain = 2,

    HeavyRain = 3,

    Showers = 4,

    LightSnowfall = 5,

    ModerateSnowfall = 6,

    HeavySnowfall = 7,

    Snowflurries = 8,

    HailShowers = 9,

}


public class ProblemDetails : System.Collections.Generic.Dictionary<string, object>
{

}

/// <summary>
/// Dto
/// </summary>
public class ProcessInfoDto
{

    /// <summary>
    /// Item processed
    /// </summary>
    public int PublicCount { get; set; }

    /// <summary>
    /// Protected observations count
    /// </summary>
    public int ProtectedCount { get; set; }

    /// <summary>
    /// Harvest end date and time
    /// </summary>
    public System.DateTimeOffset End { get; set; }

    /// <summary>
    /// Provider information about meta data
    /// </summary>
    public System.Collections.Generic.ICollection<ProcessInfoDto> MetadataInfo { get; set; }

    /// <summary>
    /// Information about providers
    /// </summary>
    public System.Collections.Generic.ICollection<ProviderInfoDto> ProvidersInfo { get; set; }

    /// <summary>
    /// Harvest start date and time
    /// </summary>
    public System.DateTimeOffset Start { get; set; }

    /// <summary>
    /// Running status
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Id, equals updated instance (0 or 1)
    /// </summary>
    public string Id { get; set; }

}

/// <summary>
/// Artportalen project information.
/// </summary>
public class Project
{

    /// <summary>
    /// Indicates if species observations that are reported in
    /// <br/>a project are publicly available or not.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Information about the type of project,
    /// <br/>for example 'Environmental monitoring'.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Information about the type of project in Swedish,
    /// <br/>for example 'Miljöövervakning'.
    /// </summary>
    public string CategorySwedish { get; set; }

    /// <summary>
    /// Description of a project.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Date when the project ends (UTC).
    /// </summary>
    public System.DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// An identifier for the project.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the project.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Name of person or organization that owns the project.
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// Project parameters
    /// </summary>
    public System.Collections.Generic.ICollection<ProjectParameter> ProjectParameters { get; set; }

    /// <summary>
    /// Web address that leads to more information about the
    /// <br/>project. The information should be accessible
    /// <br/>from the most commonly used web browsers.
    /// </summary>
    public string ProjectURL { get; set; }

    /// <summary>
    /// Date when the project starts (UTC).
    /// </summary>
    public System.DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// Survey method used in a project to
    /// <br/>retrieve species observations.
    /// </summary>
    public string SurveyMethod { get; set; }

    /// <summary>
    /// Survey method URL.
    /// </summary>
    public string SurveyMethodUrl { get; set; }

}

/// <summary>
/// Information about a project in Artportalen.
/// </summary>
public class ProjectDto
{

    /// <summary>
    /// Indicates if species observations that are reported in
    /// <br/>a project are publicly available or not.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Information about the type of project,
    /// <br/>for example 'Environmental monitoring'.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Information about the type of project in Swedish,
    /// <br/>for example 'Miljöövervakning'.
    /// </summary>
    public string CategorySwedish { get; set; }

    /// <summary>
    /// Description of a project.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Date when the project ends.
    /// </summary>
    public System.DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// An identifier for the project.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the project.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Name of person or organization that owns the project.
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// Web address that leads to more information about the project.
    /// </summary>
    public string ProjectURL { get; set; }

    /// <summary>
    /// Date when the project starts.
    /// </summary>
    public System.DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// Survey method used in a project to
    /// <br/>retrieve species observations.
    /// </summary>
    public string SurveyMethod { get; set; }

    /// <summary>
    /// Survey method URL.
    /// </summary>
    public string SurveyMethodUrl { get; set; }

}

/// <summary>
/// Artportalen project parameter.
/// </summary>
public class ProjectParameter
{

    /// <summary>
    /// Id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Project description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Unit for this species observation project parameter..
    /// </summary>
    public string Unit { get; set; }

    /// <summary>
    /// Data type.
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// Value of the data in string format.
    /// </summary>
    public string Value { get; set; }

}

/// <summary>
/// Artportalen projects summary.
/// </summary>
public class ProjectsSummary
{

    /// <summary>
    /// Project 1 Id.
    /// </summary>
    public int? Project1Id { get; set; }

    /// <summary>
    /// Project 1 name.
    /// </summary>
    public string Project1Name { get; set; }

    /// <summary>
    /// Project 1 category.
    /// </summary>
    public string Project1Category { get; set; }

    /// <summary>
    /// Project 1 URL.
    /// </summary>
    public string Project1Url { get; set; }

    /// <summary>
    /// Project 1 values.
    /// </summary>
    public string Project1Values { get; set; }

    /// <summary>
    /// Project 2 Id.
    /// </summary>
    public int? Project2Id { get; set; }

    /// <summary>
    /// Project 2 name.
    /// </summary>
    public string Project2Name { get; set; }

    /// <summary>
    /// Project 2 category.
    /// </summary>
    public string Project2Category { get; set; }

    /// <summary>
    /// Project 2 URL.
    /// </summary>
    public string Project2Url { get; set; }

    /// <summary>
    /// Project 2 values.
    /// </summary>
    public string Project2Values { get; set; }

}


public enum PropertyFieldDataType
{

    Boolean = 0,

    DateTime = 1,

    Double = 2,

    Int32 = 3,

    Int64 = 4,

    String = 5,

    TimeSpan = 6,

}

/// <summary>
/// Information about a property field.
/// </summary>
public class PropertyFieldDescriptionDto
{

    /// <summary>
    /// Property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Property path.
    /// </summary>
    public string PropertyPath { get; set; }

    /// <summary>
    /// Swedish title.
    /// </summary>
    public string SwedishTitle { get; set; }

    /// <summary>
    /// English title.
    /// </summary>
    public string EnglishTitle { get; set; }

    /// <summary>
    /// Darwin Core name.
    /// </summary>
    public string DwcName { get; set; }

    /// <summary>
    /// Darwin Core identifier.
    /// </summary>
    public string DwcIdentifier { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public PropertyFieldDataType DataType { get; set; }

    /// <summary>
    /// Indicates whether the data type is nullable.
    /// </summary>
    public bool DataTypeNullable { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public OutputFieldSet FieldSet { get; set; }

    /// <summary>
    /// The field sets this property is part of.
    /// </summary>
    // TODO(system.text.json): Add string enum item converter
    public System.Collections.Generic.ICollection<OutputFieldSet> PartOfFieldSets { get; set; }

}

/// <summary>
/// Property label type.
/// </summary>
public enum PropertyLabelType
{

    PropertyName = 0,

    PropertyPath = 1,

    Swedish = 2,

    English = 3,

}


public enum ProtectionFilterDto
{

    Public = 0,

    Sensitive = 1,

    BothPublicAndSensitive = 2,

}


public class ProviderInfoDto
{

    public int? DataProviderId { get; set; }

    public string DataProviderIdentifier { get; set; }

    /// <summary>
    /// Number of items harvested
    /// </summary>
    public int? HarvestCount { get; set; }

    /// <summary>
    /// Harvest end date and time
    /// </summary>
    public System.DateTimeOffset? HarvestEnd { get; set; }

    /// <summary>
    /// Harvest note
    /// </summary>
    public string HarvestNotes { get; set; }

    /// <summary>
    /// Harvest start date and time
    /// </summary>
    public System.DateTimeOffset? HarvestStart { get; set; }

    /// <summary>
    /// Status of harvest
    /// </summary>
    public string HarvestStatus { get; set; }

    /// <summary>
    /// Last incremental process count
    /// </summary>
    public int? LatestIncrementalPublicCount { get; set; }

    /// <summary>
    /// Last incremental process count
    /// </summary>
    public int? LatestIncrementalProtectedCount { get; set; }

    /// <summary>
    /// Last incremental process end
    /// </summary>
    public System.DateTimeOffset? LatestIncrementalEnd { get; set; }

    /// <summary>
    /// Last incremental process status
    /// </summary>
    public string LatestIncrementalStatus { get; set; }

    /// <summary>
    /// Last incremental process start
    /// </summary>
    public System.DateTimeOffset? LatestIncrementalStart { get; set; }

    /// <summary>
    /// Number of items processed
    /// </summary>
    public int? PublicProcessCount { get; set; }

    /// <summary>
    /// Number of items processed
    /// </summary>
    public int? ProtectedProcessCount { get; set; }

    /// <summary>
    /// Process end date and time
    /// </summary>
    public System.DateTimeOffset? ProcessEnd { get; set; }

    /// <summary>
    /// Process start date and time
    /// </summary>
    public System.DateTimeOffset ProcessStart { get; set; }

    /// <summary>
    /// Status of processing
    /// </summary>
    public string ProcessStatus { get; set; }

    /// <summary>
    /// Id of data provider
    /// </summary>
    public string DataProviderType { get; set; }

}

/// <summary>
/// Public observations generalization filter.
/// </summary>
public enum PublicGeneralizationFilterDto
{

    NoFilter = 0,

    OnlyGeneralized = 1,

    DontIncludeGeneralized = 2,

}

/// <summary>
/// Search filter for aggregations
/// </summary>
public class SearchFilterAggregationDto
{

    /// <summary>
    /// Limit returned observations based on bird nest activity level.
    /// <br/>Only bird observations in Artportalen are affected
    /// <br/>by this search criteria.
    /// <br/>Observation of other organism groups (not birds) are
    /// <br/>not affected by this search criteria.
    /// </summary>
    public int? BirdNestActivityLimit { get; set; }

    public DataProviderFilterDto DataProvider { get; set; }

    public DataStewardshipFilterDto DataStewardship { get; set; }

    public DateFilterDto Date { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingDeterminationFilterDto DeterminationFilter { get; set; }

    /// <summary>
    /// Filter by diffusion status.
    /// </summary>
    // TODO(system.text.json): Add string enum item converter
    public System.Collections.Generic.ICollection<DiffusionStatusDto> DiffusionStatuses { get; set; }

    public EventFilterDto Event { get; set; }

    public ExcludeFilterDto ExcludeFilter { get; set; }

    public GeographicsFilterDto Geographics { get; set; }

    public ModifiedDateFilterDto ModifiedDate { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingNotRecoveredFilterDto NotRecoveredFilter { get; set; }

    /// <summary>
    /// Only get observations observed by me
    /// </summary>
    public bool? ObservedByMe { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public OccurrenceStatusFilterValuesDto OccurrenceStatus { get; set; }

    /// <summary>
    /// Project id's to match.
    /// </summary>
    public System.Collections.Generic.ICollection<int> ProjectIds { get; set; }

    /// <summary>
    /// Only get observations reported by me
    /// </summary>
    public bool? ReportedByMe { get; set; }

    public TaxonFilterDto Taxon { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public StatusVerificationDto VerificationStatus { get; set; }

}

/// <summary>
/// Internal search filter.
/// </summary>
public class SearchFilterAggregationInternalDto
{

    /// <summary>
    /// Limit returned observations based on bird nest activity level.
    /// <br/>Only bird observations in Artportalen are affected
    /// <br/>by this search criteria.
    /// <br/>Observation of other organism groups (not birds) are
    /// <br/>not affected by this search criteria.
    /// </summary>
    public int? BirdNestActivityLimit { get; set; }

    public DataProviderFilterDto DataProvider { get; set; }

    public DataStewardshipFilterDto DataStewardship { get; set; }

    public DateFilterDto Date { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingDeterminationFilterDto DeterminationFilter { get; set; }

    /// <summary>
    /// Filter by diffusion status.
    /// </summary>
    // TODO(system.text.json): Add string enum item converter
    public System.Collections.Generic.ICollection<DiffusionStatusDto> DiffusionStatuses { get; set; }

    public EventFilterDto Event { get; set; }

    public ExcludeFilterDto ExcludeFilter { get; set; }

    public GeographicsFilterDto Geographics { get; set; }

    public ModifiedDateFilterDto ModifiedDate { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingNotRecoveredFilterDto NotRecoveredFilter { get; set; }

    /// <summary>
    /// Only get observations observed by me
    /// </summary>
    public bool? ObservedByMe { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public OccurrenceStatusFilterValuesDto OccurrenceStatus { get; set; }

    /// <summary>
    /// Project id's to match.
    /// </summary>
    public System.Collections.Generic.ICollection<int> ProjectIds { get; set; }

    /// <summary>
    /// Only get observations reported by me
    /// </summary>
    public bool? ReportedByMe { get; set; }

    public TaxonFilterDto Taxon { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public StatusVerificationDto VerificationStatus { get; set; }

    public ExtendedFilterDto ExtendedFilter { get; set; }

    public GeneralizationFilterDto GeneralizationFilter { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public ProtectionFilterDto ProtectionFilter { get; set; }

}

/// <summary>
/// Search filter.
/// </summary>
public class SearchFilterBaseDto
{

    /// <summary>
    /// Limit returned observations based on bird nest activity level.
    /// <br/>Only bird observations in Artportalen are affected
    /// <br/>by this search criteria.
    /// <br/>Observation of other organism groups (not birds) are
    /// <br/>not affected by this search criteria.
    /// </summary>
    public int? BirdNestActivityLimit { get; set; }

    public DataProviderFilterDto DataProvider { get; set; }

    public DataStewardshipFilterDto DataStewardship { get; set; }

    public DateFilterDto Date { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingDeterminationFilterDto DeterminationFilter { get; set; }

    /// <summary>
    /// Filter by diffusion status.
    /// </summary>
    // TODO(system.text.json): Add string enum item converter
    public System.Collections.Generic.ICollection<DiffusionStatusDto> DiffusionStatuses { get; set; }

    public EventFilterDto Event { get; set; }

    public ExcludeFilterDto ExcludeFilter { get; set; }

    public GeographicsFilterDto Geographics { get; set; }

    public ModifiedDateFilterDto ModifiedDate { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingNotRecoveredFilterDto NotRecoveredFilter { get; set; }

    /// <summary>
    /// Only get observations observed by me
    /// </summary>
    public bool? ObservedByMe { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public OccurrenceStatusFilterValuesDto OccurrenceStatus { get; set; }

    /// <summary>
    /// Project id's to match.
    /// </summary>
    public System.Collections.Generic.ICollection<int> ProjectIds { get; set; }

    /// <summary>
    /// Only get observations reported by me
    /// </summary>
    public bool? ReportedByMe { get; set; }

    public TaxonFilterDto Taxon { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public StatusVerificationDto VerificationStatus { get; set; }

}

/// <summary>
/// Search filter.
/// </summary>
public class SearchFilterDto
{

    /// <summary>
    /// Limit returned observations based on bird nest activity level.
    /// <br/>Only bird observations in Artportalen are affected
    /// <br/>by this search criteria.
    /// <br/>Observation of other organism groups (not birds) are
    /// <br/>not affected by this search criteria.
    /// </summary>
    public int? BirdNestActivityLimit { get; set; }

    public DataProviderFilterDto DataProvider { get; set; }

    public DataStewardshipFilterDto DataStewardship { get; set; }

    public DateFilterDto Date { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingDeterminationFilterDto DeterminationFilter { get; set; }

    /// <summary>
    /// Filter by diffusion status.
    /// </summary>
    // TODO(system.text.json): Add string enum item converter
    public System.Collections.Generic.ICollection<DiffusionStatusDto> DiffusionStatuses { get; set; }

    public EventFilterDto Event { get; set; }

    public ExcludeFilterDto ExcludeFilter { get; set; }

    public GeographicsFilterDto Geographics { get; set; }

    public ModifiedDateFilterDto ModifiedDate { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingNotRecoveredFilterDto NotRecoveredFilter { get; set; }

    /// <summary>
    /// Only get observations observed by me
    /// </summary>
    public bool? ObservedByMe { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public OccurrenceStatusFilterValuesDto OccurrenceStatus { get; set; }

    /// <summary>
    /// Project id's to match.
    /// </summary>
    public System.Collections.Generic.ICollection<int> ProjectIds { get; set; }

    /// <summary>
    /// Only get observations reported by me
    /// </summary>
    public bool? ReportedByMe { get; set; }

    public TaxonFilterDto Taxon { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public StatusVerificationDto VerificationStatus { get; set; }

    public OutputFilterDto Output { get; set; }

}

/// <summary>
/// Internal search filter.
/// </summary>
public class SearchFilterInternalBaseDto
{

    /// <summary>
    /// Limit returned observations based on bird nest activity level.
    /// <br/>Only bird observations in Artportalen are affected
    /// <br/>by this search criteria.
    /// <br/>Observation of other organism groups (not birds) are
    /// <br/>not affected by this search criteria.
    /// </summary>
    public int? BirdNestActivityLimit { get; set; }

    public DataProviderFilterDto DataProvider { get; set; }

    public DataStewardshipFilterDto DataStewardship { get; set; }

    public DateFilterDto Date { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingDeterminationFilterDto DeterminationFilter { get; set; }

    /// <summary>
    /// Filter by diffusion status.
    /// </summary>
    // TODO(system.text.json): Add string enum item converter
    public System.Collections.Generic.ICollection<DiffusionStatusDto> DiffusionStatuses { get; set; }

    public EventFilterDto Event { get; set; }

    public ExcludeFilterDto ExcludeFilter { get; set; }

    public GeographicsFilterDto Geographics { get; set; }

    public ModifiedDateFilterDto ModifiedDate { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingNotRecoveredFilterDto NotRecoveredFilter { get; set; }

    /// <summary>
    /// Only get observations observed by me
    /// </summary>
    public bool? ObservedByMe { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public OccurrenceStatusFilterValuesDto OccurrenceStatus { get; set; }

    /// <summary>
    /// Project id's to match.
    /// </summary>
    public System.Collections.Generic.ICollection<int> ProjectIds { get; set; }

    /// <summary>
    /// Only get observations reported by me
    /// </summary>
    public bool? ReportedByMe { get; set; }

    public TaxonFilterDto Taxon { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public StatusVerificationDto VerificationStatus { get; set; }

    public ExtendedFilterDto ExtendedFilter { get; set; }

    public GeneralizationFilterDto GeneralizationFilter { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public ProtectionFilterDto ProtectionFilter { get; set; }

}

/// <summary>
/// Internal search filter.
/// </summary>
public class SearchFilterInternalDto
{

    public static SearchFilterInternalDto CreateTestFilter()
    {
        return new SearchFilterInternalDto
        {
            Taxon = new TaxonFilterDto
            {
                Ids = [100077] // Utter
            },
            Output = new OutputFilterExtendedDto
            {
                FieldSet = OutputFieldSet.All
            }
        };
    }

    /// <summary>
    /// Limit returned observations based on bird nest activity level.
    /// <br/>Only bird observations in Artportalen are affected
    /// <br/>by this search criteria.
    /// <br/>Observation of other organism groups (not birds) are
    /// <br/>not affected by this search criteria.
    /// </summary>
    public int? BirdNestActivityLimit { get; set; }

    public DataProviderFilterDto DataProvider { get; set; }

    public DataStewardshipFilterDto DataStewardship { get; set; }

    public DateFilterDto Date { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingDeterminationFilterDto DeterminationFilter { get; set; }

    /// <summary>
    /// Filter by diffusion status.
    /// </summary>
    // TODO(system.text.json): Add string enum item converter
    public System.Collections.Generic.ICollection<DiffusionStatusDto> DiffusionStatuses { get; set; }

    public EventFilterDto Event { get; set; }

    public ExcludeFilterDto ExcludeFilter { get; set; }

    public GeographicsFilterDto Geographics { get; set; }

    public ModifiedDateFilterDto ModifiedDate { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SightingNotRecoveredFilterDto NotRecoveredFilter { get; set; }

    /// <summary>
    /// Only get observations observed by me
    /// </summary>
    public bool? ObservedByMe { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public OccurrenceStatusFilterValuesDto OccurrenceStatus { get; set; }

    /// <summary>
    /// Project id's to match.
    /// </summary>
    public System.Collections.Generic.ICollection<int> ProjectIds { get; set; }

    /// <summary>
    /// Only get observations reported by me
    /// </summary>
    public bool? ReportedByMe { get; set; }

    public TaxonFilterDto Taxon { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public StatusVerificationDto VerificationStatus { get; set; }

    public ExtendedFilterDto ExtendedFilter { get; set; }

    public GeneralizationFilterDto GeneralizationFilter { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public ProtectionFilterDto ProtectionFilter { get; set; }

    /// <summary>
    /// By default totalCount in search response will not exceed 10 000. If IncludeRealCount is true, totalCount will show the real number of hits. Even if it's more than 10 000 (performance cost)
    /// </summary>
    public bool? IncludeRealCount { get; set; }

    public OutputFilterExtendedDto Output { get; set; }

}

/// <summary>
/// Sort order enum
/// </summary>
public enum SearchSortOrder
{

    Asc = 0,

    Desc = 1,

}

/// <summary>
/// Sensitive observations generalization filter.
/// </summary>
public enum SensitiveGeneralizationFilterDto
{

    DontIncludeGeneralizedObservations = 0,

    IncludeGeneralizedObservations = 1,

    OnlyGeneralizedObservations = 2,

}


public enum SightingDeterminationFilterDto
{

    NoFilter = 0,

    NotUnsureDetermination = 1,

    OnlyUnsureDetermination = 2,

}


public enum SightingNotPresentFilterDto
{

    NoFilter = 0,

    DontIncludeNotPresent = 1,

    OnlyNotPresent = 2,

    IncludeNotPresent = 3,

}


public enum SightingNotRecoveredFilterDto
{

    NoFilter = 0,

    OnlyNotRecovered = 1,

    DontIncludeNotRecovered = 2,

    IncludeNotRecovered = 3,

}


public enum SightingTypeFilterDto
{

    DoNotShowMerged = 0,

    ShowOnlyMerged = 1,

    ShowBoth = 2,

    DoNotShowSightingsInMerged = 3,

    DoNotShowMergedIncludeReplacementChilds = 4,

}


public enum SightingUnspontaneousFilterDto
{

    NoFilter = 0,

    NotUnspontaneous = 1,

    Unspontaneous = 2,

}

/// <summary>
/// Search filter for signal search.
/// </summary>
public class SignalFilterDto
{

    /// <summary>
    /// Limit returned observations based on bird nest activity level.
    /// <br/>Only bird observations in Artportalen are affected
    /// <br/>by this search criteria.
    /// <br/>Observation of other organism groups (not birds) are
    /// <br/>not affected by this search criteria.
    /// </summary>
    public int? BirdNestActivityLimit { get; set; }

    public DataProviderFilterDto DataProvider { get; set; }

    public GeographicsFilterDto Geographics { get; set; }

    /// <summary>
    /// Observation start date specified in the ISO 8601 standard. If no timezone is specified, GMT+1 (CEST) is assumed
    /// </summary>
    public System.DateTimeOffset? StartDate { get; set; }

    public TaxonFilterBaseDto Taxon { get; set; }

}


public enum SnowCover
{

    SnowFreeGround = 0,

    SnowCoveredGround = 1,

    ThinOrPartialSnowCoveredGround = 2,

}


public class SortOrderDto
{

    /// <summary>
    /// Name of field to sort by
    /// </summary>
    public string SortBy { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SearchSortOrder SortOrder { get; set; }

}

/// <summary>
/// Species group (Artgrupp)
/// </summary>
public enum SpeciesGroup
{

    All = 0,

    VascularPlants = 1,

    Mosses = 2,

    Lichens = 3,

    Fungi = 4,

    AlgaeAndMicroOrganisms = 5,

    Invertebrates = 6,

    Birds = 7,

    AmphibiansAndReptiles = 8,

    OtherVertebrates = 9,

    Bats = 10,

    Fishes = 11,

}


public enum StatusVerificationDto
{

    BothVerifiedAndNotVerified = 0,

    Verified = 1,

    NotVerified = 2,

}

/// <summary>
/// Id value dto
/// </summary>
public class StringIdValueDto
{

    /// <summary>
    /// Id of item
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The value.
    /// </summary>
    public string Value { get; set; }

}

/// <summary>
/// Substrate info.
/// </summary>
public class Substrate
{

    /// <summary>
    /// Description of substrate.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Substrate id.
    /// </summary>
    public int? Id { get; set; }

    public VocabularyValue Name { get; set; }

    /// <summary>
    /// Quantity of substrate.
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// Description of substrate species.
    /// </summary>
    public string SpeciesDescription { get; set; }

    /// <summary>
    /// Substrate taxon id.
    /// </summary>
    public int? SpeciesId { get; set; }

    /// <summary>
    /// Scientific name of substrate species.
    /// </summary>
    public string SpeciesScientificName { get; set; }

    /// <summary>
    /// Vernacular name of substrate species.
    /// </summary>
    public string SpeciesVernacularName { get; set; }

    /// <summary>
    /// Description of substrate
    /// </summary>
    public string SubstrateDescription { get; set; }

}

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
    public string AcceptedNameUsage { get; set; }

    /// <summary>
    /// An identifier for the name usage (documented meaning of
    /// <br/>the name according to a source) of the currently valid
    /// <br/>(zoological) or accepted (botanical) taxon.
    /// </summary>
    public string AcceptedNameUsageId { get; set; }

    public TaxonAttributes Attributes { get; set; }

    /// <summary>
    /// Part of bird directive?
    /// </summary>
    public bool BirdDirective { get; set; }

    /// <summary>
    /// The full scientific name of the class in which
    /// <br/>the taxon is classified.
    /// </summary>
    public string Class { get; set; }

    /// <summary>
    /// Name used for display
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// The full scientific name of the family in which
    /// <br/>the taxon is classified.
    /// </summary>
    public string Family { get; set; }

    /// <summary>
    /// The full scientific name of the genus in which
    /// <br/>the taxon is classified.
    /// </summary>
    public string Genus { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of taxa names
    /// <br/>terminating at the rank immediately superior to the
    /// <br/>taxon referenced in the taxon record.
    /// <br/>Recommended best practice is to order the list
    /// <br/>starting with the highest rank and separating the names
    /// <br/>for each rank with a semi-colon (";").
    /// </summary>
    public string HigherClassification { get; set; }

    /// <summary>
    /// Object id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the lowest or terminal infraspecific epithet
    /// <br/>of the scientificName, excluding any rank designation.
    /// </summary>
    public string InfraspecificEpithet { get; set; }

    /// <summary>
    /// The full scientific name of the kingdom in which the
    /// <br/>taxon is classified.
    /// </summary>
    public string Kingdom { get; set; }

    /// <summary>
    /// The reference to the source in which the specific
    /// <br/>taxon concept circumscription is defined or implied -
    /// <br/>traditionally signified by the Latin "sensu" or "sec."
    /// <br/>(from secundum, meaning "according to").
    /// <br/>For taxa that result from identifications, a reference
    /// <br/>to the keys, monographs, experts and other sources should
    /// <br/>be given.
    /// </summary>
    public string NameAccordingTo { get; set; }

    /// <summary>
    /// An identifier for the source in which the specific
    /// <br/>taxon concept circumscription is defined or implied.
    /// <br/>See nameAccordingTo.
    /// </summary>
    public string NameAccordingToId { get; set; }

    /// <summary>
    /// A reference for the publication in which the
    /// <br/>scientificName was originally established under the rules
    /// <br/>of the associated nomenclaturalCode.
    /// </summary>
    public string NamePublishedIn { get; set; }

    /// <summary>
    /// An identifier for the publication in which the
    /// <br/>scientificName was originally established under the
    /// <br/>rules of the associated nomenclaturalCode.
    /// </summary>
    public string NamePublishedInId { get; set; }

    /// <summary>
    /// The four-digit year in which the scientificName
    /// <br/>was published.
    /// </summary>
    public string NamePublishedInYear { get; set; }

    /// <summary>
    /// The nomenclatural code (or codes in the case of an
    /// <br/>ambiregnal name) under which the scientificName is
    /// <br/>constructed.
    /// <br/>Recommended best practice is to use a controlled vocabulary.
    /// </summary>
    public string NomenclaturalCode { get; set; }

    /// <summary>
    /// The status related to the original publication of the
    /// <br/>name and its conformance to the relevant rules of
    /// <br/>nomenclature. It is based essentially on an algorithm
    /// <br/>according to the business rules of the code.
    /// <br/>It requires no taxonomic opinion.
    /// </summary>
    public string NomenclaturalStatus { get; set; }

    /// <summary>
    /// The full scientific name of the order in which
    /// <br/>the taxon is classified.
    /// </summary>
    public string Order { get; set; }

    /// <summary>
    /// The taxon name, with authorship and date information
    /// <br/>if known, as it originally appeared when first established
    /// <br/>under the rules of the associated nomenclaturalCode.
    /// <br/>The basionym (botany) or basonym (bacteriology) of the
    /// <br/>scientificName or the senior/earlier homonym for replaced
    /// <br/>names.
    /// </summary>
    public string OriginalNameUsage { get; set; }

    /// <summary>
    /// An identifier for the name usage (documented meaning of
    /// <br/>the name according to a source) in which the terminal
    /// <br/>element of the scientificName was originally established
    /// <br/>under the rules of the associated nomenclaturalCode.
    /// </summary>
    public string OriginalNameUsageId { get; set; }

    /// <summary>
    /// The full name, with authorship and date information
    /// <br/>if known, of the direct, most proximate higher-rank
    /// <br/>parent taxon (in a classification) of the most specific
    /// <br/>element of the scientificName.
    /// </summary>
    public string ParentNameUsage { get; set; }

    /// <summary>
    /// An identifier for the name usage (documented meaning
    /// <br/>of the name according to a source) of the direct,
    /// <br/>most proximate higher-rank parent taxon
    /// <br/>(in a classification) of the most specific
    /// <br/>element of the scientificName.
    /// </summary>
    public string ParentNameUsageId { get; set; }

    /// <summary>
    /// The full scientific name of the phylum or division
    /// <br/>in which the taxon is classified.
    /// </summary>
    public string Phylum { get; set; }

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
    public string ScientificName { get; set; }

    /// <summary>
    /// The authorship information for the scientificName
    /// <br/>formatted according to the conventions of the applicable
    /// <br/>nomenclaturalCode.
    /// </summary>
    public string ScientificNameAuthorship { get; set; }

    /// <summary>
    /// An identifier for the nomenclatural (not taxonomic)
    /// <br/>details of a scientific name.
    /// </summary>
    public string ScientificNameId { get; set; }

    /// <summary>
    /// Secondary parents dyntaxa taxon ids.
    /// </summary>
    public System.Collections.Generic.ICollection<int> SecondaryParentDyntaxaTaxonIds { get; set; }

    /// <summary>
    /// The name of the first or species epithet of
    /// <br/>the scientificName.
    /// </summary>
    public string SpecificEpithet { get; set; }

    /// <summary>
    /// The full scientific name of the subgenus in which
    /// <br/>the taxon is classified. Values should include the
    /// <br/>genus to avoid homonym confusion.
    /// </summary>
    public string Subgenus { get; set; }

    /// <summary>
    /// An identifier for the taxonomic concept to which the record
    /// <br/>refers - not for the nomenclatural details of a taxon.
    /// <br/>In SwedishSpeciesObservationSOAPService this property
    /// <br/>has the same value as property TaxonID.
    /// <br/>GUID in Dyntaxa is used as value for this property.
    /// </summary>
    public string TaxonConceptId { get; set; }

    /// <summary>
    /// An identifier for the set of taxon information
    /// <br/>(data associated with the Taxon class). May be a global
    /// <br/>unique identifier or an identifier specific to the data set.
    /// <br/>In SwedishSpeciesObservationSOAPService this property
    /// <br/>has the same value as property TaxonConceptID.
    /// <br/>GUID in Dyntaxa is used as value for this property.
    /// </summary>
    public string TaxonId { get; set; }

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
    public string TaxonomicStatus { get; set; }

    /// <summary>
    /// The taxonomic rank of the most specific name in the
    /// <br/>scientificName. Recommended best practice is to use
    /// <br/>a controlled vocabulary.
    /// </summary>
    public string TaxonRank { get; set; }

    /// <summary>
    /// Comments or notes about the taxon or name.
    /// </summary>
    public string TaxonRemarks { get; set; }

    /// <summary>
    /// The taxonomic rank of the most specific name in the
    /// <br/>scientificName as it appears in the original record.
    /// </summary>
    public string VerbatimTaxonRank { get; set; }

    /// <summary>
    /// A common or vernacular name.
    /// </summary>
    public string VernacularName { get; set; }

    /// <summary>
    /// Verbatim id
    /// </summary>
    public string VerbatimId { get; set; }

    /// <summary>
    /// Verbatim name
    /// </summary>
    public string VerbatimName { get; set; }

}


public class TaxonAggregationItemDto
{

    public System.DateTimeOffset? FirstSighting { get; set; }

    public System.DateTimeOffset? LastSighting { get; set; }

    public int TaxonId { get; set; }

    public int ObservationCount { get; set; }

}

/// <summary>
/// Result returned by paged query
/// </summary>
public class TaxonAggregationItemDtoPagedResultDto
{

    /// <summary>
    /// Ignores the specified number of items and returns a sequence starting at the item after the last skipped item (if
    /// <br/>any)
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Returns a sequence containing up to the specified number of items. Anything after the count is ignored
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Total number of records matching the query
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Paged records
    /// </summary>
    public System.Collections.Generic.ICollection<TaxonAggregationItemDto> Records { get; set; }

}

/// <summary>
/// Taxon attributes.
/// </summary>
public class TaxonAttributes
{

    /// <summary>
    /// Indicates whether the species is the subject
    /// <br/>of an action plan ('åtgärdsprogram' in swedish).
    /// </summary>
    public string ActionPlan { get; set; }

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
    public string InvasiveRiskAssessmentCategory { get; set; }

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
    public string OrganismGroup { get; set; }

    /// <summary>
    /// Parent Dyntaxa TaxonId.
    /// </summary>
    public int? ParentDyntaxaTaxonId { get; set; }

    /// <summary>
    /// Indicates whether the species 
    /// <br/>is protected by the law in Sweden.
    /// </summary>
    public bool ProtectedByLaw { get; set; }

    public VocabularyValue ProtectionLevel { get; set; }

    /// <summary>
    /// Redlist category for redlisted species. Possible redlist values
    /// <br/>are DD (Data Deficient), EX (Extinct),
    /// <br/>RE (Regionally Extinct), CR (Critically Endangered),
    /// <br/>EN (Endangered), VU (Vulnerable), NT (Near Threatened).
    /// <br/>Not redlisted species has no value in this property.
    /// </summary>
    public string RedlistCategory { get; set; }

    /// <summary>
    /// Derivied red list category from parent taxa
    /// </summary>
    public string RedlistCategoryDerived { get; set; }

    public VocabularyValue SensitivityCategory { get; set; }

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
    public string SwedishOccurrence { get; set; }

    /// <summary>
    /// This property contains information about the species
    /// <br/>immigration history.
    /// </summary>
    public string SwedishHistory { get; set; }

    /// <summary>
    /// Scientific synonym names.
    /// </summary>
    public System.Collections.Generic.ICollection<TaxonSynonymName> Synonyms { get; set; }

    public VocabularyValue TaxonCategory { get; set; }

    /// <summary>
    /// Vernacular names.
    /// </summary>
    public System.Collections.Generic.ICollection<TaxonVernacularName> VernacularNames { get; set; }

}

/// <summary>
/// Taxon filter.
/// </summary>
public class TaxonFilterBaseDto
{

    /// <summary>
    /// If true, also include the underlying hierarchical taxa in search.
    /// <br/>E.g. If ids=[4000104](Aves) and includeUnderlyingTaxa=true, then you search for all birds.
    /// </summary>
    public bool? IncludeUnderlyingTaxa { get; set; }

    /// <summary>
    /// Dyntaxa taxon id's to match.
    /// </summary>
    public System.Collections.Generic.ICollection<int> Ids { get; set; }

    /// <summary>
    /// Add (merge) or filter taxa by using taxon lists.
    /// </summary>
    public System.Collections.Generic.ICollection<int> TaxonListIds { get; set; }

}

/// <summary>
/// Taxon filter.
/// </summary>
public class TaxonFilterDto
{

    /// <summary>
    /// If true, also include the underlying hierarchical taxa in search.
    /// <br/>E.g. If ids=[4000104](Aves) and includeUnderlyingTaxa=true, then you search for all birds.
    /// </summary>
    public bool? IncludeUnderlyingTaxa { get; set; }

    /// <summary>
    /// Dyntaxa taxon id's to match.
    /// </summary>
    public System.Collections.Generic.ICollection<int> Ids { get; set; }

    /// <summary>
    /// Add (merge) or filter taxa by using taxon lists.
    /// </summary>
    public System.Collections.Generic.ICollection<int> TaxonListIds { get; set; }

    /// <summary>
    /// Redlist categories to match.
    /// <br/>Possible values are: "DD", "EX", "RE", "CR", "EN", "VU", "NT", "LC", "NA", "NE"
    /// </summary>
    public System.Collections.Generic.ICollection<string> RedListCategories { get; set; }

    /// <summary>
    /// Taxon categories to match.
    /// </summary>
    public System.Collections.Generic.ICollection<int> TaxonCategories { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public TaxonListOperatorDto TaxonListOperator { get; set; }

}

/// <summary>
/// Taxon list definition.
/// </summary>
public class TaxonListDefinitionDto
{

    /// <summary>
    /// Is the list allowed in signal search?
    /// </summary>
    public bool CanBeUsedInSignalSearch { get; set; }

    /// <summary>
    /// The Id of the taxon list.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The parent Id of the taxon list.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// The name of the taxon list.
    /// </summary>
    public string Name { get; set; }

}

/// <summary>
/// Operator to use when TaxonListIds is specified.
/// </summary>
public enum TaxonListOperatorDto
{

    Merge = 0,

    Filter = 1,

}

/// <summary>
/// Taxon information.
/// </summary>
public class TaxonListTaxonInformationDto
{

    /// <summary>
    /// Dyntaxa taxon id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Scientific name.
    /// </summary>
    public string ScientificName { get; set; }

    /// <summary>
    /// Swedish name.
    /// </summary>
    public string SwedishName { get; set; }

}

/// <summary>
/// Taxon aggregation item containing sum of underlying taxa values.
/// </summary>
public class TaxonSumAggregationItem
{

    /// <summary>
    /// Taxon id.
    /// </summary>
    public int TaxonId { get; set; }

    /// <summary>
    /// Observation count.
    /// </summary>
    public int ObservationCount { get; set; }

    /// <summary>
    /// Sum of observation count including underlying taxa observation count.
    /// </summary>
    public int SumObservationCount { get; set; }

    /// <summary>
    /// Number of provinces the taxon is observed.
    /// </summary>
    public int ProvinceCount { get; set; }

    /// <summary>
    /// Number of provinces the taxon is observed including underlying taxa.
    /// </summary>
    public int SumProvinceCount { get; set; }

    /// <summary>
    /// Sum of observation count including underlying taxa observation count, by Province id.
    /// </summary>
    public System.Collections.Generic.IDictionary<string, int?> SumObservationCountByProvinceId { get; set; }

}

/// <summary>
/// Result returned by paged query
/// </summary>
public class TaxonSumAggregationItemPagedResultDto
{

    /// <summary>
    /// Ignores the specified number of items and returns a sequence starting at the item after the last skipped item (if
    /// <br/>any)
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Returns a sequence containing up to the specified number of items. Anything after the count is ignored
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Total number of records matching the query
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Paged records
    /// </summary>
    public System.Collections.Generic.ICollection<TaxonSumAggregationItem> Records { get; set; }

}

/// <summary>
/// Taxon synonym.
/// </summary>
public class TaxonSynonymName
{

    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Author.
    /// </summary>
    public string Author { get; set; }

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
    public string TaxonomicStatus { get; set; }

    /// <summary>
    /// The status related to the original publication of the
    /// <br/>name and its conformance to the relevant rules of
    /// <br/>nomenclature. It is based essentially on an algorithm
    /// <br/>according to the business rules of the code.
    /// <br/>It requires no taxonomic opinion.
    /// </summary>
    public string NomenclaturalStatus { get; set; }

}

/// <summary>
/// Taxon vernacular name.
/// </summary>
public class TaxonVernacularName
{

    /// <summary>
    /// A common vernacular name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// ISO 639-1 language code used for the vernacular name value..
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// The standard code for the country in which the vernacular name is used.
    /// <br/>Recommended best practice is to use the ISO 3166-1-alpha-2 country codes available as a vocabulary
    /// <br/>at http://rs.gbif.org/vocabulary/iso/3166-1_alpha2.xml. For multiple countries separate values with a comma ","
    /// </summary>
    public string CountryCode { get; set; }

    /// <summary>
    /// This term is true if the source citing the use of this vernacular name indicates the usage has 
    /// <br/>some preference or specific standing over other possible vernacular names used for the species.
    /// </summary>
    public bool IsPreferredName { get; set; }

    /// <summary>
    /// Valid for sighting
    /// </summary>
    public bool ValidForSighting { get; set; }

}


public enum TimeRangeDto
{

    Morning = 0,

    Forenoon = 1,

    Afternoon = 2,

    Evening = 3,

    Night = 4,

}


/// <summary>
/// Unit for a certain amount of organisms (given in the attribute quantity) or
/// <br/>Unit for a reported measurement (given in the attribute "vädermått")..
/// </summary>
public enum Unit
{

    Percent = 0,

    Cm2 = 1,

    Cm3 = 2,

    Dm2 = 3,

    Kompassgrader = 4,

    Ms = 5,

    M2 = 6,

    Styck = 7,

    GraderCelsius = 8,

}

/// <summary>
/// User role authority area.
/// </summary>
public class UserAreaDto
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public AreaTypeDto AreaType { get; set; }

    /// <summary>
    /// Area FeatureId.
    /// </summary>
    public string FeatureId { get; set; }

    /// <summary>
    /// Area name.
    /// </summary>
    public string Name { get; set; }

}

/// <summary>
/// User role authority.
/// </summary>
public class UserAuthorityDto
{

    /// <summary>
    /// Authority id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Authority name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Authority areas.
    /// </summary>
    public System.Collections.Generic.ICollection<UserAreaDto> Areas { get; set; }

}

/// <summary>
/// User information.
/// </summary>
public class UserInformationDto
{

    /// <summary>
    /// User id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// First name.
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// Last name.
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// Email.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Indicates whether the user has any role with sensitive species observation authority.
    /// </summary>
    public bool HasSensitiveSpeciesAuthority { get; set; }

    /// <summary>
    /// Indicates whether the use user has any role with sighting indication authority.
    /// </summary>
    public bool HasSightingIndicationAuthority { get; set; }

    /// <summary>
    /// User roles.
    /// </summary>
    public System.Collections.Generic.ICollection<UserRoleDto> Roles { get; set; }

}


public class UserInternal
{

    /// <summary>
    /// User discovered the observation
    /// </summary>
    public bool Discover { get; set; }

    /// <summary>
    /// User Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Person Id
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// User Service id
    /// </summary>
    public int? UserServiceUserId { get; set; }

    /// <summary>
    /// User alias
    /// </summary>
    public string UserAlias { get; set; }

    /// <summary>
    /// User with sort &gt; 0 is authorized to view the observation
    /// </summary>
    public bool? ViewAccess { get; set; }

}

/// <summary>
/// User role.
/// </summary>
public class UserRoleDto
{

    /// <summary>
    /// Role id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Role name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Role short name.
    /// </summary>
    public string ShortName { get; set; }

    /// <summary>
    /// Role description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Indicates whether this role has sensitive species observation authority.
    /// </summary>
    public bool HasSensitiveSpeciesAuthority { get; set; }

    /// <summary>
    /// Indicates whether this role has sighting indication authority.
    /// </summary>
    public bool HasSightingIndicationAuthority { get; set; }

    public System.Collections.Generic.ICollection<UserAuthorityDto> Authorities { get; set; }

}

/// <summary>
/// States the visibility conditions during the survey event.
/// </summary>
public enum Visibility
{

    Fog1Km = 0,

    Haze1To4Km = 1,

    Moderate4To10Km = 2,

    Good10To20Km = 3,

    VeryGood20Km = 4,

}


public class VocabularyDto
{

    public int Id { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public VocabularyIdDto EnumId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool Localized { get; set; }

    public System.Collections.Generic.ICollection<VocabularyValueInfoDto> Values { get; set; }

    public System.Collections.Generic.ICollection<ExternalSystemMappingDto> ExternalSystemsMapping { get; set; }

}


public enum VocabularyIdDto
{

    Sex = 0,

    Activity = 1,

    LifeStage = 2,

    Biotope = 3,

    Substrate = 4,

    VerificationStatus = 5,

    Institution = 6,

    Unit = 7,

    BasisOfRecord = 8,

    Continent = 9,

    EstablishmentMeans = 10,

    OccurrenceStatus = 11,

    AccessRights = 12,

    Country = 13,

    Type = 14,

    AreaType = 15,

    DiscoveryMethod = 16,

    DeterminationMethod = 17,

    ReproductiveCondition = 18,

    Behavior = 19,

    SensitivityCategory = 20,

    BirdNestActivity = 21,

    TaxonCategory = 22,

}

/// <summary>
/// Describes a value associated with a vocabulary.
/// </summary>
public class VocabularyValue
{

    /// <summary>
    /// If the entry exist in the vocabulary, then Id is greater than or equal to 0.
    /// <br/>If the entry doesn't exist in the vocabulary, then Id is equal to -1.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The value.
    /// </summary>
    public string Value { get; set; }

}


public class VocabularyValueInfoCategoryDto
{

    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool Localized { get; set; }

    /// <summary>
    /// Translations.
    /// </summary>
    public System.Collections.Generic.ICollection<VocabularyValueTranslationDto> Translations { get; set; }

}


public class VocabularyValueInfoDto
{

    public int Id { get; set; }

    public string Value { get; set; }

    public string Description { get; set; }

    public bool Localized { get; set; }

    public VocabularyValueInfoCategoryDto Category { get; set; }

    /// <summary>
    /// Translations.
    /// </summary>
    public System.Collections.Generic.ICollection<VocabularyValueTranslationDto> Translations { get; set; }

}


public class VocabularyValueTranslationDto
{

    /// <summary>
    /// Culture code. I.e. en-GB, sv-SE
    /// </summary>
    public string CultureCode { get; set; }

    /// <summary>
    /// Translation
    /// </summary>
    public string Value { get; set; }

}


public class Weather
{

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public SnowCover SnowCover { get; set; }

    public Measuring Sunshine { get; set; }

    public Measuring AirTemperature { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public CompassDirection WindDirection { get; set; }

    public Measuring WindDirectionDegrees { get; set; }

    public Measuring WindSpeed { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public WindStrength WindStrength { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Precipitation Precipitation { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Visibility Visibility { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public Cloudiness Cloudiness { get; set; }

}

/// <summary>
/// States the strength of the wind during the survey event.
/// </summary>
public enum WindStrength
{

    _0Beaufort = 0,

    _1Beaufort = 1,

    _2Beaufort = 2,

    _3Beaufort = 3,

    _4Beaufort = 4,

    _5Beaufort = 5,

    _6Beaufort = 6,

    _7Beaufort = 7,

    _8Beaufort = 8,

    _9Beaufort = 9,

    _10Beaufort = 10,

    _11Beaufort = 11,

    _12Beaufort = 12,

    Calm1Ms = 13,

    LightBreezeUpTo3Ms = 14,

    ModerateBreeze4To7Ms = 15,

    FreshBreeze8Till13Ms = 16,

    NearGale14To19Ms = 17,

    StrongGale20To24Ms = 18,

    Storm25Till32Ms = 19,

    Hurricane33Ms = 20,

}


public class XYBoundingBoxDto
{

    public XYCoordinateDto BottomRight { get; set; }

    public XYCoordinateDto TopLeft { get; set; }

}


public class XYCoordinateDto
{

    public double X { get; set; }

    public double Y { get; set; }

}
