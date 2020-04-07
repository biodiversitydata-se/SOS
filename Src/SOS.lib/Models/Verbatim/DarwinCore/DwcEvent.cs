using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.DarwinCore
{
    public class DwcEvent : IEntity<ObjectId>
    {
        /// <summary>
        /// MongoDb Id. // todo - should we use Id, RecordId or EventID?
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// The Record Id used in the DwC-A CSV file.
        /// </summary>
        [BsonIgnore]
        public string RecordId { get; set; }

        /// <summary>
        /// DwC-A file name.
        /// </summary>
        public string DwcArchiveFilename { get; set; }

        /// <summary>
        /// Data provider id.
        /// </summary>
        public int DataProviderId { get; set; }

        /// <summary>
        /// Data provider identifier.
        /// </summary>
        public string DataProviderIdentifier { get; set; }

        /// <summary>
        /// Audubon media linked to the event.
        /// </summary>
        public List<DwcAudubonMedia> AudubonMedia { get; set; }

        /// <summary>
        /// Multimedia linked to the event.
        /// </summary>
        public List<DwcMultimedia> Multimedia { get; set; }

        /// <summary>
        /// Measurement or fact linked to the event.
        /// </summary>
        public ICollection<DwcMeasurementOrFact> MeasurementOrFacts { get; set; }

        /// <summary>
        /// Extended measurement or fact linked to the event.
        /// </summary>
        public ICollection<DwcExtendedMeasurementOrFact> ExtendedMeasurementOrFacts { get; set; }

        #region RecordLevel
        /// <summary>
        /// Darwin Core term name: dcterms:accessRights.
        /// Information about who can access the resource or
        /// an indication of its security status.
        /// Access Rights may include information regarding
        /// access or restrictions based on privacy, security,
        /// or other policies.
        /// </summary>
        public string AccessRights { get; set; }

        /// <summary>
        /// Darwin Core term name: basisOfRecord.
        /// The specific nature of the data record -
        /// a subtype of the dcterms:type.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Darwin Core Type Vocabulary
        /// (http://rs.tdwg.org/dwc/terms/type-vocabulary/index.htm).
        /// In Species Gateway this property has the value
        /// HumanObservation.
        /// </summary>
        public string BasisOfRecord { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:bibliographicCitation.
        /// A bibliographic reference for the resource as a statement
        /// indicating how this record should be cited (attributed)
        /// when used.
        /// Recommended practice is to include sufficient
        /// bibliographic detail to identify the resource as
        /// unambiguously as possible.
        /// This property is currently not used.
        /// </summary>
        public string BibliographicCitation { get; set; }

        /// <summary>
        /// Darwin Core term name: collectionCode.
        /// The name, acronym, coden, or initialism identifying the 
        /// collection or data set from which the record was derived.
        /// </summary>
        public string CollectionCode { get; set; }

        /// <summary>
        /// Darwin Core term name: collectionID.
        /// An identifier for the collection or dataset from which
        /// the record was derived.
        /// For physical specimens, the recommended best practice is
        /// to use the identifier in a collections registry such as
        /// the Biodiversity Collections Index
        /// (http://www.biodiversitycollectionsindex.org/).
        /// </summary>
        public string CollectionID { get; set; }

        /// <summary>
        /// Darwin Core term name: dataGeneralizations.
        /// Actions taken to make the shared data less specific or
        /// complete than in its original form.
        /// Suggests that alternative data of higher quality
        /// may be available on request.
        /// This property is currently not used.
        /// </summary>
        public string DataGeneralizations { get; set; }

        /// <summary>
        /// Darwin Core term name: datasetID.
        /// An identifier for the set of data.
        /// May be a global unique identifier or an identifier
        /// specific to a collection or institution.
        /// </summary>
        public string DatasetID { get; set; }

        /// <summary>
        /// Darwin Core term name: datasetName.
        /// The name identifying the data set
        /// from which the record was derived.
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        /// Darwin Core term name: dynamicProperties.
        /// A list (concatenated and separated) of additional
        /// measurements, facts, characteristics, or assertions
        /// about the record. Meant to provide a mechanism for
        /// structured content such as key-value pairs.
        /// This property is currently not used.
        /// </summary>
        public string DynamicProperties { get; set; }

        /// <summary>
        /// Darwin Core term name: informationWithheld.
        /// Additional information that exists, but that has
        /// not been shared in the given record.
        /// This property is currently not used.
        /// </summary>
        public string InformationWithheld { get; set; }

        /// <summary>
        /// Darwin Core term name: institutionCode.
        /// The name (or acronym) in use by the institution
        /// having custody of the object(s) or information
        /// referred to in the record.
        /// Currently this property has the value ArtDatabanken.
        /// </summary>
        public string InstitutionCode { get; set; }

        /// <summary>
        /// Darwin Core term name: institutionID.
        /// An identifier for the institution having custody of 
        /// the object(s) or information referred to in the record.
        /// This property is currently not used.
        /// </summary>
        public string InstitutionID { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:language.
        /// A language of the resource.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as RFC 4646 [RFC4646].
        /// This property is currently not used.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// A legal document giving official permission to do something with the resource.
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:modified.
        /// The most recent date-time on which the resource was changed.
        /// For Darwin Core, recommended best practice is to use an
        /// encoding scheme, such as ISO 8601:2004(E).
        /// </summary>
        public string Modified { get; set; }

        /// <summary>
        /// Darwin Core term name: ownerInstitutionCode.
        /// The name (or acronym) in use by the institution having
        /// ownership of the object(s) or information referred
        /// to in the record.
        /// This property is currently not used.
        /// </summary>
        public string OwnerInstitutionCode { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:references.
        /// A related resource that is referenced, cited,
        /// or otherwise pointed to by the described resource.
        /// This property is currently not used.
        /// </summary>
        public string References { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:rightsHolder.
        /// A person or organization owning or
        /// managing rights over the resource.
        /// This property is currently not used.
        /// </summary>
        public string RightsHolder { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:type.
        /// The nature or genre of the resource.
        /// For Darwin Core, recommended best practice is
        /// to use the name of the class that defines the
        /// root of the record.
        /// This property is currently not used.
        /// </summary>
        public string Type { get; set; }
        #endregion

        #region Event
        /// <summary>
        /// Darwin Core term name: day.
        /// The integer day of the month on which the Event occurred
        /// (start date of observation).
        /// This property is currently not used.
        /// </summary>
        public string Day { get; set; }


        /// <summary>
        /// Darwin Core term name: endDayOfYear.
        /// The latest ordinal day of the year on which the Event
        /// occurred (1 for January 1, 365 for December 31,
        /// except in a leap year, in which case it is 366).
        /// This property is currently not used.
        /// </summary>
        public string EndDayOfYear { get; set; }

        /// <summary>
        /// Darwin Core term name: eventDate.
        /// The date-time or interval during which an Event occurred.
        /// For occurrences, this is the date-time when the event
        /// was recorded. Not suitable for a time in a geological
        /// context. Recommended best practice is to use an encoding
        /// scheme, such as ISO 8601:2004(E).
        /// For example: ”2007-03-01 13:00:00 - 2008-05-11 15:30:00”
        /// This property is currently not used.
        /// </summary>
        public string EventDate { get; set; }

        /// <summary>
        /// Darwin Core term name: eventID.
        /// A list (concatenated and separated) of identifiers
        /// (publication, global unique identifier, URI) of
        /// media associated with the Occurrence.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string EventID { get; set; }

        /// <summary>
        /// An identifier for the broader Event that groups this and potentially other Events.
        /// </summary>
        public string ParentEventID { get; set; }

        /// <summary>
        /// Darwin Core term name: eventRemarks.
        /// Comments or notes about the Event.
        /// This property is currently not used.
        /// </summary>
        public string EventRemarks { get; set; }

        /// <summary>
        /// Darwin Core term name: eventTime.
        /// The time or interval during which an Event occurred.
        /// Recommended best practice is to use an encoding scheme,
        /// such as ISO 8601:2004(E).
        /// For example: ”13:00:00 - 15:30:00”
        /// This property is currently not used.
        /// </summary>
        public string EventTime { get; set; }

        /// <summary>
        /// Darwin Core term name: fieldNotes.
        /// One of a) an indicator of the existence of, b) a
        /// reference to (publication, URI), or c) the text of
        /// notes taken in the field about the Event.
        /// This property is currently not used.
        /// </summary>
        public string FieldNotes { get; set; }

        /// <summary>
        /// Darwin Core term name: fieldNumber.
        /// An identifier given to the event in the field. Often 
        /// serves as a link between field notes and the Event.
        /// This property is currently not used.
        /// </summary>
        public string FieldNumber { get; set; }

        /// <summary>
        /// Darwin Core term name: habitat.
        /// A category or description of the habitat
        /// in which the Event occurred.
        /// This property is currently not used.
        /// </summary>
        public string Habitat { get; set; }

        /// <summary>
        /// Darwin Core term name: month.
        /// The ordinal month in which the Event occurred.
        /// This property is currently not used.
        /// </summary>
        public string Month { get; set; }

        /// <summary>
        /// The unit of measurement of the size (time duration, length, area, or volume) of a sample in a sampling event.
        /// A sampleSizeUnit must have a corresponding sampleSizeValue, e.g., 5 for sampleSizeValue with metre for sampleSizeUnit.
        /// </summary>
        /// <example>
        /// minute, hour, day, metre, square metre, cubic metre
        /// </example>
        public string SampleSizeUnit { get; set; }

        /// <summary>
        /// A numeric value for a measurement of the size (time duration, length, area, or volume) of a sample in a sampling event.
        /// </summary>
        /// <example>
        /// 5 for sampleSizeValue with metre for sampleSizeUnit.
        /// </example>
        public string SampleSizeValue { get; set; }

        /// <summary>
        /// Darwin Core term name: samplingEffort.
        /// The amount of effort expended during an Event.
        /// This property is currently not used.
        /// </summary>
        public string SamplingEffort { get; set; }

        /// <summary>
        /// Darwin Core term name: samplingProtocol.
        /// The name of, reference to, or description of the
        /// method or protocol used during an Event.
        /// This property is currently not used.
        /// </summary>
        public string SamplingProtocol { get; set; }

        /// <summary>
        /// Darwin Core term name: startDayOfYear.
        /// The earliest ordinal day of the year on which the
        /// Event occurred (1 for January 1, 365 for December 31,
        /// except in a leap year, in which case it is 366).
        /// This property is currently not used.
        /// </summary>
        public string StartDayOfYear { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimEventDate.
        /// The verbatim original representation of the date
        /// and time information for an Event.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimEventDate { get; set; }

        /// <summary>
        /// Darwin Core term name: year.
        /// The four-digit year in which the Event occurred,
        /// according to the Common Era Calendar.
        /// This property is currently not used.
        /// </summary>
        public string Year { get; set; }
        #endregion Event

        #region Location
        /// <summary>
        /// Darwin Core term name: continent.
        /// The name of the continent in which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographi
        /// Names or the ISO 3166 Continent code.
        /// This property is currently not used.
        /// </summary>
        public string Continent { get; set; }

        /// <summary>
        /// Darwin Core term name: CoordinatePrecision.
        /// A decimal representation of the precision of the coordinates
        /// given in the DecimalLatitude and DecimalLongitude.
        /// This property is currently not used.
        /// </summary>
        public string CoordinatePrecision { get; set; }

        /// <summary>
        /// Darwin Core term name: coordinateUncertaintyInMeters.
        /// The horizontal distance (in meters) from the given
        /// CoordinateX and CoordinateY describing the
        /// smallest circle containing the whole of the Location.
        /// Leave the value empty if the uncertainty is unknown, cannot
        /// be estimated, or is not applicable (because there are
        /// no coordinates). Zero is not a valid value for this term.
        /// </summary>
        public string CoordinateUncertaintyInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: country.
        /// The name of the country or major administrative unit
        /// in which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Darwin Core term name: countryCode.
        /// The standard code for the country in which the
        /// Location occurs.
        /// Recommended best practice is to use ISO 3166-1-alpha-2
        /// country codes.
        /// This property is currently not used.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Darwin Core term name: county.
        /// The full, unabbreviated name of the next smaller
        /// administrative region than stateProvince(county, shire,
        /// department, etc.) in which the Location occurs
        /// ('län' in swedish).
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// Darwin Core term name: decimalLatitude.
        /// Definition in Darwin Core:
        /// The geographic latitude (in decimal degrees, using
        /// the spatial reference system given in geodeticDatum)
        /// of the geographic center of a Location. Positive values
        /// are north of the Equator, negative values are south of it.
        /// Legal values lie between -90 and 90, inclusive.
        /// </summary>
        public string DecimalLatitude { get; set; }

        /// <summary>
        /// Darwin Core term name: decimalLongitude.
        /// Definition in Darwin Core:
        /// The geographic longitude (in decimal degrees, using
        /// the spatial reference system given in geodeticDatum)
        /// of the geographic center of a Location. Positive
        /// values are east of the Greenwich Meridian, negative
        /// values are west of it. Legal values lie between -180
        /// and 180, inclusive.
        /// </summary>
        public string DecimalLongitude { get; set; }

        /// <summary>
        /// Darwin Core term name: footprintSpatialFit.
        /// The ratio of the area of the footprint (footprintWKT)
        /// to the area of the true (original, or most specific)
        /// spatial representation of the Location. Legal values are
        /// 0, greater than or equal to 1, or undefined. A value of
        /// 1 is an exact match or 100% overlap. A value of 0 should
        /// be used if the given footprint does not completely contain
        /// the original representation. The footprintSpatialFit is
        /// undefined (and should be left blank) if the original
        /// representation is a point and the given georeference is
        /// not that same point. If both the original and the given
        /// georeference are the same point, the footprintSpatialFit
        /// is 1.
        /// This property is currently not used.
        /// </summary>
        public string FootprintSpatialFit { get; set; }

        /// <summary>
        /// Darwin Core term name: footprintSRS.
        /// A Well-Known Text (WKT) representation of the Spatial
        /// Reference System (SRS) for the footprintWKT of the
        /// Location. Do not use this term to describe the SRS of
        /// the decimalLatitude and decimalLongitude, even if it is
        /// the same as for the footprintWKT - use the geodeticDatum
        /// instead.
        /// This property is currently not used.
        /// </summary>
        public string FootprintSRS { get; set; }

        /// <summary>
        /// Darwin Core term name: footprintWKT.
        /// A Well-Known Text (WKT) representation of the shape
        /// (footprint, geometry) that defines the Location.
        /// A Location may have both a point-radius representation
        /// (see decimalLatitude) and a footprint representation,
        /// and they may differ from each other.
        /// This property is currently not used.
        /// </summary>
        public string FootprintWKT { get; set; }

        /// <summary>
        /// Darwin Core term name: geodeticDatum.
        /// The ellipsoid, geodetic datum, or spatial reference
        /// system (SRS) upon which the geographic coordinates
        /// given in decimalLatitude and decimalLongitude as based.
        /// Recommended best practice is use the EPSG code as a
        /// controlled vocabulary to provide an SRS, if known.
        /// Otherwise use a controlled vocabulary for the name or
        /// code of the geodetic datum, if known. Otherwise use a
        /// controlled vocabulary for the name or code of the
        /// ellipsoid, if known. If none of these is known, use the
        /// value "unknown".
        /// This property is currently not used.
        /// </summary>
        public string GeodeticDatum { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferencedBy.
        /// A list (concatenated and separated) of names of people,
        /// groups, or organizations who determined the georeference
        /// (spatial representation) the Location.
        /// This property is currently not used.
        /// </summary>
        public string GeoreferencedBy { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferencedDate.
        /// The date on which the Location was georeferenced.
        /// Recommended best practice is to use an encoding scheme,
        /// such as ISO 8601:2004(E).
        /// This property is currently not used.
        /// </summary>
        public string GeoreferencedDate { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferenceProtocol.
        /// A description or reference to the methods used to
        /// determine the spatial footprint, coordinates, and
        /// uncertainties.
        /// This property is currently not used.
        /// </summary>
        public string GeoreferenceProtocol { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferenceRemarks.
        /// Notes or comments about the spatial description
        /// determination, explaining assumptions made in addition
        /// or opposition to the those formalized in the method
        /// referred to in georeferenceProtocol.
        /// This property is currently not used.
        /// </summary>
        public string GeoreferenceRemarks { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferenceSources.
        /// A list (concatenated and separated) of maps, gazetteers,
        /// or other resources used to georeference the Location,
        /// described specifically enough to allow anyone in the
        /// future to use the same resources.
        /// This property is currently not used.
        /// </summary>
        public string GeoreferenceSources { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferenceVerificationStatus.
        /// A categorical description of the extent to which the
        /// georeference has been verified to represent the best
        /// possible spatial description. Recommended best practice
        /// is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string GeoreferenceVerificationStatus { get; set; }

        /// <summary>
        /// Darwin Core term name: higherGeography.
        /// A list (concatenated and separated) of geographic
        /// names less specific than the information captured
        /// in the locality term.
        /// This property is currently not used.
        /// </summary>
        public string HigherGeography { get; set; }

        /// <summary>
        /// Darwin Core term name: higherGeographyID.
        /// An identifier for the geographic region within which
        /// the Location occurred.
        /// Recommended best practice is to use an
        /// persistent identifier from a controlled vocabulary
        /// such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string HigherGeographyID { get; set; }

        /// <summary>
        /// Darwin Core term name: island.
        /// The name of the island on or near which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string Island { get; set; }

        /// <summary>
        /// Darwin Core term name: islandGroup.
        /// The name of the island group in which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string IslandGroup { get; set; }

        /// <summary>
        /// Darwin Core term name: locality.
        /// The specific description of the place. Less specific
        /// geographic information can be provided in other
        /// geographic terms (higherGeography, continent, country,
        /// stateProvince, county, municipality, waterBody, island,
        /// islandGroup). This term may contain information modified
        /// from the original to correct perceived errors or
        /// standardize the description.
        /// </summary>
        public string Locality { get; set; }

        /// <summary>
        /// Darwin Core term name: locationAccordingTo.
        /// Information about the source of this Location information.
        /// Could be a publication (gazetteer), institution,
        /// or team of individuals.
        /// This property is currently not used.
        /// </summary>
        public string LocationAccordingTo { get; set; }

        /// <summary>
        /// Darwin Core term name: locationID.
        /// An identifier for the set of location information
        /// (data associated with dcterms:Location).
        /// May be a global unique identifier or an identifier
        /// specific to the data set.
        /// This property is currently not used.
        /// </summary>
        public string LocationID { get; set; }

        /// <summary>
        /// Darwin Core term name: locationRemarks.
        /// Comments or notes about the Location.
        /// This property is currently not used.
        /// </summary>
        public string LocationRemarks { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Web address that leads to more information about the
        /// location. The information should be accessible
        /// from the most commonly used web browsers.
        /// </summary>
        //public string LocationURL { get; set; }

        /// <summary>
        /// Darwin Core term name: maximumDepthInMeters.
        /// The greater depth of a range of depth below
        /// the local surface, in meters.
        /// This property is currently not used.
        /// </summary>
        public string MaximumDepthInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: maximumDistanceAboveSurfaceInMeters.
        /// The greater distance in a range of distance from a
        /// reference surface in the vertical direction, in meters.
        /// Use positive values for locations above the surface,
        /// negative values for locations below. If depth measures
        /// are given, the reference surface is the location given
        /// by the depth, otherwise the reference surface is the
        /// location given by the elevation.
        /// This property is currently not used.
        /// </summary>
        public string MaximumDistanceAboveSurfaceInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: maximumElevationInMeters.
        /// The upper limit of the range of elevation (altitude,
        /// usually above sea level), in meters.
        /// This property is currently not used.
        /// </summary>
        public string MaximumElevationInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: minimumDepthInMeters.
        /// The lesser depth of a range of depth below the
        /// local surface, in meters.
        /// This property is currently not used.
        /// </summary>
        public string MinimumDepthInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: minimumDistanceAboveSurfaceInMeters.
        /// The lesser distance in a range of distance from a
        /// reference surface in the vertical direction, in meters.
        /// Use positive values for locations above the surface,
        /// negative values for locations below.
        /// If depth measures are given, the reference surface is
        /// the location given by the depth, otherwise the reference
        /// surface is the location given by the elevation.
        /// This property is currently not used.
        /// </summary>
        public string MinimumDistanceAboveSurfaceInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: minimumElevationInMeters.
        /// The lower limit of the range of elevation (altitude,
        /// usually above sea level), in meters.
        /// This property is currently not used.
        /// </summary>
        public string MinimumElevationInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: municipality.
        /// The full, unabbreviated name of the next smaller
        /// administrative region than county (city, municipality, etc.)
        /// in which the Location occurs.
        /// Do not use this term for a nearby named place
        /// that does not contain the actual location.
        /// </summary>
        public string Municipality { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Parish where the species observation where made.
        /// 'Socken/församling' in swedish.
        /// </summary>
        //public string Parish { get; set; }

        /// <summary>
        /// Darwin Core term name: pointRadiusSpatialFit.
        /// The ratio of the area of the point-radius
        /// (decimalLatitude, decimalLongitude,
        /// coordinateUncertaintyInMeters) to the area of the true
        /// (original, or most specific) spatial representation of
        /// the Location. Legal values are 0, greater than or equal
        /// to 1, or undefined. A value of 1 is an exact match or
        /// 100% overlap. A value of 0 should be used if the given
        /// point-radius does not completely contain the original
        /// representation. The pointRadiusSpatialFit is undefined
        /// (and should be left blank) if the original representation
        /// is a point without uncertainty and the given georeference
        /// is not that same point (without uncertainty). If both the
        /// original and the given georeference are the same point,
        /// the pointRadiusSpatialFit is 1.
        /// This property is currently not used.
        /// </summary>
        public string PointRadiusSpatialFit { get; set; }

        /// <summary>
        /// Darwin Core term name: stateProvince.
        /// The name of the next smaller administrative region than
        /// country (state, province, canton, department, region, etc.)
        /// in which the Location occurs.
        /// ('landskap' in swedish).
        /// </summary>
        public string StateProvince { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimCoordinates.
        /// The verbatim original spatial coordinates of the Location.
        /// The coordinate ellipsoid, geodeticDatum, or full
        /// Spatial Reference System (SRS) for these coordinates
        /// should be stored in verbatimSRS and the coordinate
        /// system should be stored in verbatimCoordinateSystem.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimCoordinates { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimCoordinateSystem.
        /// The spatial coordinate system for the verbatimLatitude
        /// and verbatimLongitude or the verbatimCoordinates of the
        /// Location.
        /// Recommended best practice is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimCoordinateSystem { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimDepth.
        /// The original description of the
        /// depth below the local surface.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimDepth { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimElevation.
        /// The original description of the elevation (altitude,
        /// usually above sea level) of the Location.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimElevation { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimLatitude.
        /// The verbatim original latitude of the Location.
        /// The coordinate ellipsoid, geodeticDatum, or full
        /// Spatial Reference System (SRS) for these coordinates
        /// should be stored in verbatimSRS and the coordinate
        /// system should be stored in verbatimCoordinateSystem.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimLatitude { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimLocality.
        /// The original textual description of the place.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimLocality { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimLongitude.
        /// The verbatim original longitude of the Location.
        /// The coordinate ellipsoid, geodeticDatum, or full
        /// Spatial Reference System (SRS) for these coordinates
        /// should be stored in verbatimSRS and the coordinate
        /// system should be stored in verbatimCoordinateSystem.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimLongitude { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimSRS.
        /// The ellipsoid, geodetic datum, or spatial reference
        /// system (SRS) upon which coordinates given in
        /// verbatimLatitude and verbatimLongitude, or
        /// verbatimCoordinates are based.
        /// Recommended best practice is use the EPSG code as
        /// a controlled vocabulary to provide an SRS, if known.
        /// Otherwise use a controlled vocabulary for the name or
        /// code of the geodetic datum, if known.
        /// Otherwise use a controlled vocabulary for the name or
        /// code of the ellipsoid, if known. If none of these is
        /// known, use the value "unknown".
        /// This property is currently not used.
        /// </summary>
        public string VerbatimSRS { get; set; }

        /// <summary>
        /// Darwin Core term name: waterBody.
        /// The name of the water body in which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string WaterBody { get; set; }
        #endregion

        #region GeologicalContext
        /// <summary>
        /// Darwin Core term name: bed.
        /// The full name of the lithostratigraphic bed from which
        /// the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string Bed { get; set; }

        /// <summary>
        /// Darwin Core term name: earliestAgeOrLowestStage.
        /// The full name of the earliest possible geochronologic
        /// age or lowest chronostratigraphic stage attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string EarliestAgeOrLowestStage { get; set; }

        /// <summary>
        /// Darwin Core term name: earliestEonOrLowestEonothem.
        /// The full name of the earliest possible geochronologic eon
        /// or lowest chrono-stratigraphic eonothem or the informal
        /// name ("Precambrian") attributable to the stratigraphic
        /// horizon from which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string EarliestEonOrLowestEonothem { get; set; }

        /// <summary>
        /// Darwin Core term name: earliestEpochOrLowestSeries.
        /// The full name of the earliest possible geochronologic
        /// epoch or lowest chronostratigraphic series attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string EarliestEpochOrLowestSeries { get; set; }

        /// <summary>
        /// Darwin Core term name: earliestEraOrLowestErathem.
        /// The full name of the earliest possible geochronologic
        /// era or lowest chronostratigraphic erathem attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string EarliestEraOrLowestErathem { get; set; }

        /// <summary>
        /// Use to link a dwc:GeologicalContext instance to chronostratigraphic time
        /// periods at the lowest possible level in a standardized hierarchy. Use this
        /// property to point to the earliest possible geological time period from which
        /// the cataloged item was collected.
        /// </summary>
        public string EarliestGeochronologicalEra { get; set; }

        /// <summary>
        /// Darwin Core term name: earliestPeriodOrLowestSystem.
        /// The full name of the earliest possible geochronologic
        /// period or lowest chronostratigraphic system attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string EarliestPeriodOrLowestSystem { get; set; }

        /// <summary>
        /// Darwin Core term name: formation.
        /// The full name of the lithostratigraphic formation from
        /// which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string Formation { get; set; }

        /// <summary>
        /// Darwin Core term name: geologicalContextID.
        /// An identifier for the set of information associated
        /// with a GeologicalContext (the location within a geological
        /// context, such as stratigraphy). May be a global unique
        /// identifier or an identifier specific to the data set.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string GeologicalContextID { get; set; }

        /// <summary>
        /// Darwin Core term name: group.
        /// The full name of the lithostratigraphic group from
        /// which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Darwin Core term name: highestBiostratigraphicZone.
        /// The full name of the highest possible geological
        /// biostratigraphic zone of the stratigraphic horizon
        /// from which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string HighestBiostratigraphicZone { get; set; }

        /// <summary>
        /// Darwin Core term name: latestAgeOrHighestStage.
        /// The full name of the latest possible geochronologic
        /// age or highest chronostratigraphic stage attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LatestAgeOrHighestStage { get; set; }

        /// <summary>
        /// Darwin Core term name: latestEonOrHighestEonothem.
        /// The full name of the latest possible geochronologic eon
        /// or highest chrono-stratigraphic eonothem or the informal
        /// name ("Precambrian") attributable to the stratigraphic
        /// horizon from which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LatestEonOrHighestEonothem { get; set; }

        /// <summary>
        /// Darwin Core term name: latestEpochOrHighestSeries.
        /// The full name of the latest possible geochronologic
        /// epoch or highest chronostratigraphic series attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LatestEpochOrHighestSeries { get; set; }

        /// <summary>
        /// Darwin Core term name: latestEraOrHighestErathem.
        /// The full name of the latest possible geochronologic
        /// era or highest chronostratigraphic erathem attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LatestEraOrHighestErathem { get; set; }

        /// <summary>
        /// Use to link a dwc:GeologicalContext instance to chronostratigraphic time periods at the lowest possible
        /// level in a standardized hierarchy. Use this property to point to the latest possible geological time period
        /// from which the cataloged item was collected.
        /// </summary>
        public string LatestGeochronologicalEra { get; set; }

        /// <summary>
        /// Darwin Core term name: latestPeriodOrHighestSystem.
        /// The full name of the latest possible geochronologic
        /// period or highest chronostratigraphic system attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LatestPeriodOrHighestSystem { get; set; }

        /// <summary>
        /// Darwin Core term name: lithostratigraphicTerms.
        /// The combination of all litho-stratigraphic names for
        /// the rock from which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LithostratigraphicTerms { get; set; }

        /// <summary>
        /// Darwin Core term name: lowestBiostratigraphicZone.
        /// The full name of the lowest possible geological
        /// biostratigraphic zone of the stratigraphic horizon
        /// from which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LowestBiostratigraphicZone { get; set; }

        /// <summary>
        /// Darwin Core term name: member.
        /// The full name of the lithostratigraphic member from
        /// which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string Member { get; set; }

        #endregion
    }
}
