using MongoDB.Driver.GeoJsonObjectModel;
using System.Text.Json.Serialization;

namespace SOS.Lib.Models.Processed.Sighting
{
    /// <summary>
    /// This class contains location information about a species sighting location
    /// </summary>
    public class ProcessedLocation 
    {
        /// <summary>
        /// The name of the continent in which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographi
        /// Names or the ISO 3166 Continent code.
        /// This property is currently not used.
        /// </summary>
        public string Continent { get; set; }

        /// <summary>
        /// A decimal representation of the precision of the coordinates
        /// given in the DecimalLatitude and DecimalLongitude.
        /// This property is currently not used.
        /// </summary>
        public int? CoordinatePrecision { get; set; }

        /// <summary>
        /// The horizontal distance (in meters) from the given
        /// CoordinateX and CoordinateY describing the
        /// smallest circle containing the whole of the Location.
        /// Leave the value empty if the uncertainty is unknown, cannot
        /// be estimated, or is not applicable (because there are
        /// no coordinates). Zero is not a valid value for this term.
        /// </summary>
        public int? CoordinateUncertaintyInMeters { get; set; }

        /// <summary>
        /// The name of the country or major administrative unit
        /// in which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Special handling of Kalmar/Öland
        /// </summary>
        public int? CountyPartIdByCoordinate { get; set; }

        /// <summary>
        /// The standard code for the country in which the
        /// Location occurs.
        /// Recommended best practice is to use ISO 3166-1-alpha-2
        /// country codes.
        /// This property is currently not used.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// The full, unabbreviated name of the next smaller
        /// administrative region than stateProvince(county, shire,
        /// department, etc.) in which the Location occurs
        /// ('län' in swedish).
        /// </summary>
        public ProcessedArea County { get; set; }

        /// <summary>
        /// Darwin Core term name: decimalLatitude.
        /// Definition in Darwin Core:
        /// The geographic latitude (in decimal degrees, using
        /// the spatial reference system given in geodeticDatum)
        /// of the geographic center of a Location. Positive values
        /// are north of the Equator, negative values are south of it.
        /// Legal values lie between -90 and 90, inclusive.
        /// </summary>
        public double DecimalLatitude { get; set; }

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
        public double DecimalLongitude { get; set; }

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
        // ReSharper disable once InconsistentNaming
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
        // ReSharper disable once InconsistentNaming
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
        // ReSharper disable once InconsistentNaming
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
        public string Id { get; set; }

        /// <summary>
        /// Darwin Core term name: locationRemarks.
        /// Comments or notes about the Location.
        /// This property is currently not used.
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// Darwin Core term name: maximumDepthInMeters.
        /// The greater depth of a range of depth below
        /// the local surface, in meters.
        /// This property is currently not used.
        /// </summary>
        public double? MaximumDepthInMeters { get; set; }

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
        public double? MaximumDistanceAboveSurfaceInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: maximumElevationInMeters.
        /// The upper limit of the range of elevation (altitude,
        /// usually above sea level), in meters.
        /// This property is currently not used.
        /// </summary>
        public double? MaximumElevationInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: minimumDepthInMeters.
        /// The lesser depth of a range of depth below the
        /// local surface, in meters.
        /// This property is currently not used.
        /// </summary>
        public double? MinimumDepthInMeters { get; set; }

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
        public double? MinimumDistanceAboveSurfaceInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: minimumElevationInMeters.
        /// The lower limit of the range of elevation (altitude,
        /// usually above sea level), in meters.
        /// This property is currently not used.
        /// </summary>
        public double? MinimumElevationInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: municipality.
        /// The full, unabbreviated name of the next smaller
        /// administrative region than county (city, municipality, etc.)
        /// in which the Location occurs.
        /// Do not use this term for a nearby named place
        /// that does not contain the actual location.
        /// </summary>
        public ProcessedArea Municipality { get; set; }

        /// <summary>
        /// Parish property
        /// </summary>
        public ProcessedArea Parish { get; set; }

        /// <summary>
        /// Point (WGS84)
        /// </summary>
        [JsonIgnore]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Point { get; set; }

        /// <summary>
        /// Point with accuracy buffer (WGS84)
        /// </summary>
        [JsonIgnore]
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> PointWithBuffer { get; set; }

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
        public ProcessedArea Province { get; set; }

        /// <summary>
        /// Spacial handling of Lappland
        /// </summary>
        public int? ProvincePartIdByCoordinate { get; set; }

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
        public int? VerbatimDepth { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimElevation.
        /// The original description of the elevation (altitude,
        /// usually above sea level) of the Location.
        /// This property is currently not used.
        /// </summary>
        public int? VerbatimElevation { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimLatitude.
        /// The verbatim original latitude of the Location.
        /// The coordinate ellipsoid, geodeticDatum, or full
        /// Spatial Reference System (SRS) for these coordinates
        /// should be stored in verbatimSRS and the coordinate
        /// system should be stored in verbatimCoordinateSystem.
        /// This property is currently not used.
        /// </summary>
        public double VerbatimLatitude { get; set; }

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
        public double VerbatimLongitude { get; set; }

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
        // ReSharper disable once InconsistentNaming
        public string VerbatimSRS { get; set; }

        /// <summary>
        /// Darwin Core term name: waterBody.
        /// The name of the water body in which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string WaterBody { get; set; }
    }
}
