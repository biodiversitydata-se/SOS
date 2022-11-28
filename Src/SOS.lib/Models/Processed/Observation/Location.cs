using Nest;
using SOS.Lib.Swagger;
using System.Text.Json.Serialization;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Location information for a species observation.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Location()
        {
            Attributes = new LocationAttributes();
        }

        /// <summary>
        /// Location attributes.
        /// </summary>
        public LocationAttributes Attributes { get; set; }

        /// <summary>
        ///     The name of the continent in which the Location occurs.
        ///     Recommended best practice is to use a controlled
        ///     vocabulary such as the Getty Thesaurus of Geographi
        ///     Names or the ISO 3166 Continent code.
        ///     ('län' in swedish).
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        public VocabularyValue Continent { get; set; }

        /// <summary>
        ///     A decimal representation of the precision of the coordinates
        ///     given in the DecimalLatitude and DecimalLongitude.
        /// </summary>
        public double? CoordinatePrecision { get; set; }

        /// <summary>
        ///     The horizontal distance (in meters) from the given
        ///     CoordinateX and CoordinateY describing the
        ///     smallest circle containing the whole of the Location.
        ///     Leave the value empty if the uncertainty is unknown, cannot
        ///     be estimated, or is not applicable (because there are
        ///     no coordinates). Zero is not a valid value for this term.
        /// </summary>
        public int? CoordinateUncertaintyInMeters { get; set; }

        /// <summary>
        ///     The name of the country or major administrative unit
        ///     in which the Location occurs.
        ///     Recommended best practice is to use a controlled
        ///     vocabulary such as the Getty Thesaurus of Geographic Names.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        public VocabularyValue Country { get; set; }

        /// <summary>
        ///     The standard code for the country in which the
        ///     Location occurs.
        ///     Recommended best practice is to use ISO 3166-1-alpha-2
        ///     country codes.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Area CountryRegion { get; set; }

        /// <summary>
        ///     The full, unabbreviated name of the next smaller
        ///     administrative region than stateProvince(county, shire,
        ///     department, etc.) in which the Location occurs
        ///     ('län' in swedish).
        /// </summary>
        public Area County { get; set; }

        /// <summary>
        ///     The full, unabbreviated name of the next smaller
        ///     administrative region than county (city, municipality, etc.)
        ///     in which the Location occurs.
        /// </summary>
        public Area Municipality { get; set; }

        /// <summary>
        ///     Parish ('socken' in swedish).
        /// </summary>
        public Area Parish { get; set; }

        /// <summary>       
        ///     The name of the next smaller administrative region than
        ///     country (state, province, canton, department, region, etc.)
        ///     in which the Location occurs.
        ///     ('landskap' in swedish). Darwin Core term name: stateProvince.
        /// </summary>
        public Area Province { get; set; }

        /// <summary>
        ///     The geographic latitude (in decimal degrees, using
        ///     the spatial reference system given in geodeticDatum)
        ///     of the geographic center of a Location. Positive values
        ///     are north of the Equator, negative values are south of it.
        ///     Legal values lie between -90 and 90, inclusive.
        /// </summary>
        public double? DecimalLatitude { get; set; }

        /// <summary>
        ///     The geographic longitude (in decimal degrees, using
        ///     the spatial reference system given in geodeticDatum)
        ///     of the geographic center of a Location. Positive
        ///     values are east of the Greenwich Meridian, negative
        ///     values are west of it. Legal values lie between -180
        ///     and 180, inclusive.
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
        ///     The ratio of the area of the footprint (footprintWKT)
        ///     to the area of the true (original, or most specific)
        ///     spatial representation of the Location. Legal values are
        ///     0, greater than or equal to 1, or undefined. A value of
        ///     1 is an exact match or 100% overlap. A value of 0 should
        ///     be used if the given footprint does not completely contain
        ///     the original representation. The footprintSpatialFit is
        ///     undefined (and should be left blank) if the original
        ///     representation is a point and the given georeference is
        ///     not that same point. If both the original and the given
        ///     georeference are the same point, the footprintSpatialFit
        ///     is 1.
        /// </summary>
        public string FootprintSpatialFit { get; set; }

        /// <summary>
        ///     A Well-Known Text (WKT) representation of the Spatial
        ///     Reference System (SRS) for the footprintWKT of the
        ///     Location. Do not use this term to describe the SRS of
        ///     the decimalLatitude and decimalLongitude, even if it is
        ///     the same as for the footprintWKT - use the geodeticDatum
        ///     instead.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string FootprintSRS { get; set; }

        /// <summary>
        ///     A Well-Known Text (WKT) representation of the shape
        ///     (footprint, geometry) that defines the Location.
        ///     A Location may have both a point-radius representation
        ///     (see decimalLatitude) and a footprint representation,
        ///     and they may differ from each other.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string FootprintWKT { get; set; }

        /// <summary>
        ///     The ellipsoid, geodetic datum, or spatial reference
        ///     system (SRS) upon which the geographic coordinates
        ///     given in decimalLatitude and decimalLongitude as based.
        ///     Recommended best practice is use the EPSG code as a
        ///     controlled vocabulary to provide an SRS, if known.
        ///     Otherwise use a controlled vocabulary for the name or
        ///     code of the geodetic datum, if known. Otherwise use a
        ///     controlled vocabulary for the name or code of the
        ///     ellipsoid, if known. If none of these is known, use the
        ///     value "unknown".
        /// </summary>
        public string GeodeticDatum { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of names of people,
        ///     groups, or organizations who determined the georeference
        ///     (spatial representation) the Location.
        /// </summary>
        public string GeoreferencedBy { get; set; }

        /// <summary>
        ///     The date on which the Location was georeferenced.
        ///     Recommended best practice is to use an encoding scheme,
        ///     such as ISO 8601:2004(E).
        /// </summary>
        public string GeoreferencedDate { get; set; }

        /// <summary>
        ///     A description or reference to the methods used to
        ///     determine the spatial footprint, coordinates, and
        ///     uncertainties.
        /// </summary>
        public string GeoreferenceProtocol { get; set; }

        /// <summary>
        ///     Notes or comments about the spatial description
        ///     determination, explaining assumptions made in addition
        ///     or opposition to the those formalized in the method
        ///     referred to in georeferenceProtocol.
        /// </summary>
        public string GeoreferenceRemarks { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of maps, gazetteers,
        ///     or other resources used to georeference the Location,
        ///     described specifically enough to allow anyone in the
        ///     future to use the same resources.
        /// </summary>
        public string GeoreferenceSources { get; set; }

        /// <summary>
        ///     A categorical description of the extent to which the
        ///     georeference has been verified to represent the best
        ///     possible spatial description. Recommended best practice
        ///     is to use a controlled vocabulary.
        /// </summary>
        public string GeoreferenceVerificationStatus { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of geographic
        ///     names less specific than the information captured
        ///     in the locality term.
        /// </summary>
        public string HigherGeography { get; set; }

        /// <summary>
        ///     An identifier for the geographic region within which
        ///     the Location occurred.
        ///     Recommended best practice is to use an
        ///     persistent identifier from a controlled vocabulary
        ///     such as the Getty Thesaurus of Geographic Names.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string HigherGeographyId { get; set; }

        /// <summary>
        ///     Internal flag used in validation. must be true to be stored in processed data
        /// </summary>
        [JsonIgnore]
        [SwaggerExclude]
        public bool IsInEconomicZoneOfSweden { get; set; }

        /// <summary>
        ///     The name of the island on or near which the Location occurs.
        ///     Recommended best practice is to use a controlled
        ///     vocabulary such as the Getty Thesaurus of Geographic Names.
        /// </summary>
        public string Island { get; set; }

        /// <summary>
        ///     The name of the island group in which the Location occurs.
        ///     Recommended best practice is to use a controlled
        ///     vocabulary such as the Getty Thesaurus of Geographic Names.
        /// </summary>
        public string IslandGroup { get; set; }

        /// <summary>
        ///     The specific description of the place. Less specific
        ///     geographic information can be provided in other
        ///     geographic terms (higherGeography, continent, country,
        ///     stateProvince, county, municipality, waterBody, island,
        ///     islandGroup). This term may contain information modified
        ///     from the original to correct perceived errors or
        ///     standardize the description.
        /// </summary>
        public string Locality { get; set; }

        /// <summary>
        ///     Information about the source of this Location information.
        ///     Could be a publication (gazetteer), institution,
        ///     or team of individuals.
        /// </summary>
        public string LocationAccordingTo { get; set; }

        /// <summary>
        ///     An identifier for the set of location information
        ///     (data associated with dcterms:Location).
        ///     May be a global unique identifier or an identifier
        ///     specific to the data set.
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        ///     Comments or notes about the Location.
        /// </summary>
        public string LocationRemarks { get; set; }

        /// <summary>
        ///     The greater depth of a range of depth below
        ///     the local surface, in meters.
        /// </summary>
        public double? MaximumDepthInMeters { get; set; }

        /// <summary>
        ///     The greater distance in a range of distance from a
        ///     reference surface in the vertical direction, in meters.
        ///     Use positive values for locations above the surface,
        ///     negative values for locations below. If depth measures
        ///     are given, the reference surface is the location given
        ///     by the depth, otherwise the reference surface is the
        ///     location given by the elevation.
        /// </summary>
        public double? MaximumDistanceAboveSurfaceInMeters { get; set; }

        /// <summary>
        ///     The upper limit of the range of elevation (altitude,
        ///     usually above sea level), in meters.
        /// </summary>
        public double? MaximumElevationInMeters { get; set; }

        /// <summary>
        ///     The lesser depth of a range of depth below the
        ///     local surface, in meters.
        /// </summary>
        public double? MinimumDepthInMeters { get; set; }

        /// <summary>
        ///     The lesser distance in a range of distance from a
        ///     reference surface in the vertical direction, in meters.
        ///     Use positive values for locations above the surface,
        ///     negative values for locations below.
        ///     If depth measures are given, the reference surface is
        ///     the location given by the depth, otherwise the reference
        ///     surface is the location given by the elevation.
        /// </summary>
        public double? MinimumDistanceAboveSurfaceInMeters { get; set; }

        /// <summary>
        ///     The lower limit of the range of elevation (altitude,
        ///     usually above sea level), in meters.
        /// </summary>
        public double? MinimumElevationInMeters { get; set; }

        /// <summary>
        ///     Point (WGS84).
        /// </summary>
        [SwaggerExclude]
        public PointGeoShape Point { get; set; }

        /// <summary>
        ///     Point used in distance from point search.
        /// </summary>
        [SwaggerExclude]
        public GeoLocation Point10KGridCellCenter { get; set; }

        /// <summary>
        ///     Point used in distance from point search.
        /// </summary>
        [SwaggerExclude]
        public GeoLocation PointLocation { get; set; }

        /// <summary>
        ///     Point with accuracy buffer (WGS84).
        /// </summary>
        [SwaggerExclude]
        public PolygonGeoShape PointWithBuffer { get; set; }

        /// <summary>
        /// Point with disturbance buffer
        /// </summary>
        [SwaggerExclude]
        public PolygonGeoShape PointWithDisturbanceBuffer { get; set; }
        
        /// <summary>
        ///     The ratio of the area of the point-radius
        ///     (decimalLatitude, decimalLongitude,
        ///     coordinateUncertaintyInMeters) to the area of the true
        ///     (original, or most specific) spatial representation of
        ///     the Location. Legal values are 0, greater than or equal
        ///     to 1, or undefined. A value of 1 is an exact match or
        ///     100% overlap. A value of 0 should be used if the given
        ///     point-radius does not completely contain the original
        ///     representation. The pointRadiusSpatialFit is undefined
        ///     (and should be left blank) if the original representation
        ///     is a point without uncertainty and the given georeference
        ///     is not that same point (without uncertainty). If both the
        ///     original and the given georeference are the same point,
        ///     the pointRadiusSpatialFit is 1.
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
        ///     The verbatim original spatial coordinates of the Location.
        ///     The coordinate ellipsoid, geodeticDatum, or full
        ///     Spatial Reference System (SRS) for these coordinates
        ///     should be stored in verbatimSRS and the coordinate
        ///     system should be stored in verbatimCoordinateSystem.
        /// </summary>
        public string VerbatimCoordinates { get; set; }

        /// <summary>
        ///     The spatial coordinate system for the verbatimLatitude
        ///     and verbatimLongitude or the verbatimCoordinates of the
        ///     Location.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string VerbatimCoordinateSystem { get; set; }

        /// <summary>
        ///     The original description of the
        ///     depth below the local surface.
        /// </summary>
        public string VerbatimDepth { get; set; }

        /// <summary>
        ///     The original description of the elevation (altitude,
        ///     usually above sea level) of the Location.
        /// </summary>
        public string VerbatimElevation { get; set; }

        /// <summary>
        ///     The verbatim original latitude of the Location.
        ///     The coordinate ellipsoid, geodeticDatum, or full
        ///     Spatial Reference System (SRS) for these coordinates
        ///     should be stored in verbatimSRS and the coordinate
        ///     system should be stored in verbatimCoordinateSystem.
        /// </summary>
        public string VerbatimLatitude { get; set; }

        /// <summary>
        ///     The original textual description of the place.
        /// </summary>
        public string VerbatimLocality { get; set; }

        /// <summary>        
        ///     The verbatim original longitude of the Location.
        ///     The coordinate ellipsoid, geodeticDatum, or full
        ///     Spatial Reference System (SRS) for these coordinates
        ///     should be stored in verbatimSRS and the coordinate
        ///     system should be stored in verbatimCoordinateSystem.
        /// </summary>
        public string VerbatimLongitude { get; set; }

        /// <summary>
        ///     The ellipsoid, geodetic datum, or spatial reference
        ///     system (SRS) upon which coordinates given in
        ///     verbatimLatitude and verbatimLongitude, or
        ///     verbatimCoordinates are based.
        ///     Recommended best practice is use the EPSG code as
        ///     a controlled vocabulary to provide an SRS, if known.
        ///     Otherwise use a controlled vocabulary for the name or
        ///     code of the geodetic datum, if known.
        ///     Otherwise use a controlled vocabulary for the name or
        ///     code of the ellipsoid, if known. If none of these is
        ///     known, use the value "unknown".
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string VerbatimSRS { get; set; }

        /// <summary>
        ///     The name of the water body in which the Location occurs.
        ///     Recommended best practice is to use a controlled
        ///     vocabulary such as the Getty Thesaurus of Geographic Names.
        /// </summary>
        public string WaterBody { get; set; }
    }
}