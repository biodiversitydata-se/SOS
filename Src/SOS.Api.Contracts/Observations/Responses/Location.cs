using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Location information for a species observation.
/// </summary>
public class Location
{

    /// <summary>The 5 × 5 km atlas grid cell containing the location.</summary>
    public Area? Atlas5x5 { get; set; }

    /// <summary>The 10 × 10 km atlas grid cell containing the location.</summary>
    public Area? Atlas10x10 { get; set; }

    /// <summary>Additional location attributes supplied by the data provider.</summary>
    public LocationAttributes? Attributes { get; set; }

    /// <summary>The continent in which the location occurs.</summary>
    public VocabularyValue? Continent { get; set; }

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

    /// <summary>The country in which the location occurs.</summary>
    public VocabularyValue? Country { get; set; }

    /// <summary>
    /// The standard code for the country in which the
    /// <br/>Location occurs.
    /// <br/>Recommended best practice is to use ISO 3166-1-alpha-2
    /// <br/>country codes.
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>The region within the country in which the location occurs.</summary>
    public Area? CountryRegion { get; set; }

    /// <summary>The county (län) containing the location.</summary>
    public Area? County { get; set; }

    /// <summary>The municipality (kommun) containing the location.</summary>
    public Area? Municipality { get; set; }

    /// <summary>The parish (församling) containing the location.</summary>
    public Area? Parish { get; set; }

    /// <summary>The historical province (landskap) containing the location.</summary>
    public Area? Province { get; set; }

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
    public string? FootprintSpatialFit { get; set; }

    /// <summary>
    /// A Well-Known Text (WKT) representation of the Spatial
    /// <br/>Reference System (SRS) for the footprintWKT of the
    /// <br/>Location. Do not use this term to describe the SRS of
    /// <br/>the decimalLatitude and decimalLongitude, even if it is
    /// <br/>the same as for the footprintWKT - use the geodeticDatum
    /// <br/>instead.
    /// </summary>
    public string? FootprintSRS { get; set; }

    /// <summary>
    /// A Well-Known Text (WKT) representation of the shape
    /// <br/>(footprint, geometry) that defines the Location.
    /// <br/>A Location may have both a point-radius representation
    /// <br/>(see decimalLatitude) and a footprint representation,
    /// <br/>and they may differ from each other.
    /// </summary>
    public string? FootprintWKT { get; set; }

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
    public string? GeodeticDatum { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of names of people,
    /// <br/>groups, or organizations who determined the georeference
    /// <br/>(spatial representation) the Location.
    /// </summary>
    public string? GeoreferencedBy { get; set; }

    /// <summary>
    /// The date on which the Location was georeferenced.
    /// <br/>Recommended best practice is to use an encoding scheme,
    /// <br/>such as ISO 8601:2004(E).
    /// </summary>
    public string? GeoreferencedDate { get; set; }

    /// <summary>
    /// A description or reference to the methods used to
    /// <br/>determine the spatial footprint, coordinates, and
    /// <br/>uncertainties.
    /// </summary>
    public string? GeoreferenceProtocol { get; set; }

    /// <summary>
    /// Notes or comments about the spatial description
    /// <br/>determination, explaining assumptions made in addition
    /// <br/>or opposition to the those formalized in the method
    /// <br/>referred to in georeferenceProtocol.
    /// </summary>
    public string? GeoreferenceRemarks { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of maps, gazetteers,
    /// <br/>or other resources used to georeference the Location,
    /// <br/>described specifically enough to allow anyone in the
    /// <br/>future to use the same resources.
    /// </summary>
    public string? GeoreferenceSources { get; set; }

    /// <summary>
    /// A categorical description of the extent to which the
    /// <br/>georeference has been verified to represent the best
    /// <br/>possible spatial description. Recommended best practice
    /// <br/>is to use a controlled vocabulary.
    /// </summary>
    public string? GeoreferenceVerificationStatus { get; set; }

    /// <summary>
    /// A list (concatenated and separated) of geographic
    /// <br/>names less specific than the information captured
    /// <br/>in the locality term.
    /// </summary>
    public string? HigherGeography { get; set; }

    /// <summary>
    /// An identifier for the geographic region within which
    /// <br/>the Location occurred.
    /// <br/>Recommended best practice is to use an
    /// <br/>persistent identifier from a controlled vocabulary
    /// <br/>such as the Getty Thesaurus of Geographic Names.
    /// </summary>
    public string? HigherGeographyId { get; set; }

    /// <summary>
    /// The name of the island on or near which the Location occurs.
    /// <br/>Recommended best practice is to use a controlled
    /// <br/>vocabulary such as the Getty Thesaurus of Geographic Names.
    /// </summary>
    public string? Island { get; set; }

    /// <summary>
    /// The name of the island group in which the Location occurs.
    /// <br/>Recommended best practice is to use a controlled
    /// <br/>vocabulary such as the Getty Thesaurus of Geographic Names.
    /// </summary>
    public string? IslandGroup { get; set; }

    /// <summary>
    /// The specific description of the place. Less specific
    /// <br/>geographic information can be provided in other
    /// <br/>geographic terms (higherGeography, continent, country,
    /// <br/>stateProvince, county, municipality, waterBody, island,
    /// <br/>islandGroup). This term may contain information modified
    /// <br/>from the original to correct perceived errors or
    /// <br/>standardize the description.
    /// </summary>
    public string? Locality { get; set; }

    /// <summary>
    /// Information about the source of this Location information.
    /// <br/>Could be a publication (gazetteer), institution,
    /// <br/>or team of individuals.
    /// </summary>
    public string? LocationAccordingTo { get; set; }

    /// <summary>
    /// An identifier for the set of location information
    /// <br/>(data associated with dcterms:Location).
    /// <br/>May be a global unique identifier or an identifier
    /// <br/>specific to the data set.
    /// </summary>
    public string? LocationId { get; set; }

    /// <summary>
    /// Comments or notes about the Location.
    /// </summary>
    public string? LocationRemarks { get; set; }

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
    public string? PointRadiusSpatialFit { get; set; }

    /// <summary>
    /// X coordinate in SWEREF99 TM.
    /// </summary>
    public double? Sweref99TmX { get; set; }

    /// <summary>
    /// Y coordinate in SWEREF99 TM.
    /// </summary>
    public double? Sweref99TmY { get; set; }

    /// <summary>
    /// Location type.
    /// </summary>
    public LocationType Type { get; set; }

    /// <summary>
    /// The verbatim original spatial coordinates of the Location.
    /// <br/>The coordinate ellipsoid, geodeticDatum, or full
    /// <br/>Spatial Reference System (SRS) for these coordinates
    /// <br/>should be stored in verbatimSRS and the coordinate
    /// <br/>system should be stored in verbatimCoordinateSystem.
    /// </summary>
    public string? VerbatimCoordinates { get; set; }

    /// <summary>
    /// The spatial coordinate system for the verbatimLatitude
    /// <br/>and verbatimLongitude or the verbatimCoordinates of the
    /// <br/>Location.
    /// <br/>Recommended best practice is to use a controlled vocabulary.
    /// </summary>
    public string? VerbatimCoordinateSystem { get; set; }

    /// <summary>
    /// The original description of the
    /// <br/>depth below the local surface.
    /// </summary>
    public string? VerbatimDepth { get; set; }

    /// <summary>
    /// The original description of the elevation (altitude,
    /// <br/>usually above sea level) of the Location.
    /// </summary>
    public string? VerbatimElevation { get; set; }

    /// <summary>
    /// The verbatim original latitude of the Location.
    /// <br/>The coordinate ellipsoid, geodeticDatum, or full
    /// <br/>Spatial Reference System (SRS) for these coordinates
    /// <br/>should be stored in verbatimSRS and the coordinate
    /// <br/>system should be stored in verbatimCoordinateSystem.
    /// </summary>
    public string? VerbatimLatitude { get; set; }

    /// <summary>
    /// The original textual description of the place.
    /// </summary>
    public string? VerbatimLocality { get; set; }

    /// <summary>
    /// The verbatim original longitude of the Location.
    /// <br/>The coordinate ellipsoid, geodeticDatum, or full
    /// <br/>Spatial Reference System (SRS) for these coordinates
    /// <br/>should be stored in verbatimSRS and the coordinate
    /// <br/>system should be stored in verbatimCoordinateSystem.
    /// </summary>
    public string? VerbatimLongitude { get; set; }

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
    public string? VerbatimSRS { get; set; }

    /// <summary>
    /// The name of the water body in which the Location occurs.
    /// <br/>Recommended best practice is to use a controlled
    /// <br/>vocabulary such as the Getty Thesaurus of Geographic Names.
    /// </summary>
    public string? WaterBody { get; set; }

}


