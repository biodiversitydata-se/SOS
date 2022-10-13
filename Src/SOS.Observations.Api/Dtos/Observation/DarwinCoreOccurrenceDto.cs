using System;

namespace SOS.Observations.Api.Dtos.Observation
{
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
        /// May be a global unique identifier or an identifier specific to the data set.
        /// </summary>
        public string GeologicalContextID { get; set; }

        /// <summary>
        /// The full name of the earliest possible geochronologic eon or lowest chrono-stratigraphic eonothem or the informal name ("Precambrian") 
        /// attributable to the stratigraphic horizon from which the cataloged item was collected.
        /// </summary>
        public string EarliestEonOrLowestEonothem { get; set; }

        /// <summary>
        /// The full name of the latest possible geochronologic eon or highest chrono-stratigraphic eonothem or the informal name ("Precambrian") 
        /// attributable to the stratigraphic horizon from which the cataloged item was collected.
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
        /// May be a global unique identifier or an identifier specific to the data set.
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
        /// Do not use this term for a nearby named place that does not contain the actual location.
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
        /// Use positive values for locations above the surface, negative values for locations below. 
        /// If depth measures are given, the reference surface is the location given by the depth, otherwise the reference surface is the location given by the elevation. 
        /// </summary>
        public double? MinimumDistanceAboveSurfaceInMeters { get; set; }

        /// <summary>
        /// The greater distance in a range of distance from a reference surface in the vertical direction, in meters. 
        /// Use positive values for locations above the surface, negative values for locations below. 
        /// If depth measures are given, the reference surface is the location given by the depth, otherwise the reference surface is the location given by the elevation.
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
        /// Positive values are north of the Equator, negative values are south of it. Legal values lie between -90 and 90, inclusive.
        /// </summary>
        public double? DecimalLatitude { get; set; }

        /// <summary>
        /// The geographic longitude (in decimal degrees, using the spatial reference system given in geodeticDatum) of the geographic center of a Location. 
        /// Positive values are east of the Greenwich Meridian, negative values are west of it. Legal values lie between -180 and 180, inclusive.
        /// </summary>
        public double? DecimalLongitude { get; set; }

        /// <summary>
        /// The ellipsoid, geodetic datum, or spatial reference system (SRS) upon which the geographic coordinates given in decimalLatitude and decimalLongitude as based.
        /// </summary>
        public string GeodeticDatum { get; set; }

        /// <summary>
        /// The horizontal distance (in meters) from the given decimalLatitude and decimalLongitude describing the smallest circle containing the whole of the Location. 
        /// Leave the value empty if the uncertainty is unknown, cannot be estimated, or is not applicable (because there are no coordinates). Zero is not a valid value for this term.
        /// </summary>
        public int? CoordinateUncertaintyInMeters { get; set; }

        /// <summary>
        /// A decimal representation of the precision of the coordinates given in the decimalLatitude and decimalLongitude. 
        /// </summary>
        public double? CoordinatePrecision { get; set; }

        /// <summary>
        /// The ratio of the area of the point-radius (decimalLatitude, decimalLongitude, coordinateUncertaintyInMeters) to the area of the true (original, 
        /// or most specific) spatial representation of the Location. Legal values are 0, greater than or equal to 1, or undefined. A value of 1 is an exact match or 100% overlap. 
        /// A value of 0 should be used if the given point-radius does not completely contain the original representation. 
        /// The pointRadiusSpatialFit is undefined (and should be left empty) if the original representation is a point without uncertainty and the given georeference is not that same point (without uncertainty). 
        /// If both the original and the given georeference are the same point, the pointRadiusSpatialFit is 1.
        /// </summary>
        public string PointRadiusSpatialFit { get; set; }

        /// <summary>
        /// The verbatim original spatial coordinates of the Location. The coordinate ellipsoid, geodeticDatum, or full Spatial Reference System (SRS) 
        /// for these coordinates should be stored in verbatimSRS and the coordinate system should be stored in verbatimCoordinateSystem.
        /// </summary>
        public string VerbatimCoordinates { get; set; }

        /// <summary>
        /// The verbatim original latitude of the Location. The coordinate ellipsoid, geodeticDatum, or full Spatial Reference System (SRS) 
        /// for these coordinates should be stored in verbatimSRS and the coordinate system should be stored in verbatimCoordinateSystem. 
        /// </summary>
        public string VerbatimLatitude { get; set; }

        /// <summary>
        /// The verbatim original longitude of the Location. The coordinate ellipsoid, geodeticDatum, or full Spatial Reference System (SRS) 
        /// for these coordinates should be stored in verbatimSRS and the coordinate system should be stored in verbatimCoordinateSystem. 
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
        /// A Location may have both a point-radius representation (see decimalLatitude) and a footprint representation, and they may differ from each other.
        /// </summary>
        public string FootprintWKT { get; set; }

        /// <summary>
        /// The ellipsoid, geodetic datum, or spatial reference system (SRS) upon which the geometry given in footprintWKT is based.
        /// </summary>
        public string FootprintSRS { get; set; }

        /// <summary>
        /// he ratio of the area of the footprint (footprintWKT) to the area of the true (original, or most specific) spatial representation of the Location. 
        /// Legal values are 0, greater than or equal to 1, or undefined. A value of 1 is an exact match or 100% overlap. A value of 0 should be used if the given footprint does not 
        /// completely contain the original representation. The footprintSpatialFit is undefined (and should be left empty) if the original representation is a point without uncertainty 
        /// and the given georeference is not that same point (without uncertainty). If both the original and the given georeference are the same point, the footprintSpatialFit is 1.
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
        /// construct one from a combination of identifiers in the record that will most closely make the materialSampleID globally unique.
        /// </summary>
        public string MaterialSampleID { get; set; }

        /// <summary>
        /// An identifier for the Occurrence (as opposed to a particular digital record of the occurrence). In the absence of a persistent global unique identifier, 
        /// construct one from a combination of identifiers in the record that will most closely make the occurrenceID globally unique. 
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
        /// The primary collector or observer, especially one who applies a personal identifier (recordNumber), should be listed first.
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
        /// whether in the current or any other data set or collection.
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
        public DateTime? Modified { get; set; }

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
        ///  An identifier for the nomenclatural (not taxonomic) details of a scientific name. 
        /// </summary>
        public string ScientificNameID { get; set; }

        /// <summary>
        /// An identifier for the name usage (documented meaning of the name according to a source) of the currently valid (zoological) or accepted (botanical) taxon.
        /// </summary>
        public string AcceptedNameUsageID { get; set; }

        /// <summary>
        /// An identifier for the name usage (documented meaning of the name according to a source) of the direct, 
        /// most proximate higher-rank parent taxon (in a classification) of the most specific element of the scientificName. 
        /// </summary>
        public string ParentNameUsageID { get; set; }

        /// <summary>
        /// An identifier for the name usage (documented meaning of the name according to a source) in which the terminal element of the scientificName 
        /// was originally established under the rules of the associated nomenclaturalCode.
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
        /// This term should not contain identification qualifications, which should instead be supplied in the IdentificationQualifier term. 
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
        /// The basionym (botany) or basonym (bacteriology) of the scientificName or the senior/earlier homonym for replaced names.
        /// </summary>
        public string OriginalNameUsage { get; set; }

        /// <summary>
        /// The reference to the source in which the specific taxon concept circumscription is defined or implied - traditionally signified by the Latin "sensu" or "sec." (from secundum, meaning "according to"). 
        /// For taxa that result from identifications, a reference to the keys, monographs, experts and other sources should be given.
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
        /// Rules of priority then are used to define the taxonomic status of the nomenclature contained in that scope, combined with the experts opinion. 
        /// It must be linked to a specific taxonomic reference that defines the concept.
        /// </summary>
        public string TaxonomicStatus { get; set; }

        /// <summary>
        /// The status related to the original publication of the name and its conformance to the relevant rules of nomenclature. 
        /// It is based essentially on an algorithm according to the business rules of the code. It requires no taxonomic opinion.
        /// </summary>
        public string NomenclaturalStatus { get; set; }

        /// <summary>
        /// Comments or notes about the taxon or name.
        /// </summary>
        public string TaxonRemarks { get; set; }

    }
}
