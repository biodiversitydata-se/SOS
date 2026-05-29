namespace SOS.Observations.Api.IntegrationTests.Models;

public class GeoJsonFlatLocation
{
    /// <summary>
    /// Decimal latitude WGS84 (WGS84 decimal (lat))
    /// </summary>
    public double? DecimalLatitude { get; set; }

    /// <summary>
    /// Decimal longitude WGS84 (WGS84 decimal (lon))
    /// </summary>
    public double? DecimalLongitude { get; set; }

    /// <summary>
    /// SWEREF99TM X (longitude) (SWEREF99TM X (öst))
    /// </summary>
    public double? Sweref99TmX { get; set; }

    /// <summary>
    /// SWEREF99TM Y (latitude) (SWEREF99TM Y (nord))
    /// </summary>
    public double? Sweref99TmY { get; set; }

    /// <summary>
    /// Geodetic datum (Geodetiskt datum)
    /// </summary>
    public string? GeodeticDatum { get; set; }

    /// <summary>
    /// Coordinate uncertainty (m) (Koordinatnoggrannhet (m))
    /// </summary>
    public double? CoordinateUncertaintyInMeters { get; set; }

    /// <summary>
    /// Location ID (Plats id)
    /// </summary>
    public string? LocationId { get; set; }

    /// <summary>
    /// Location external ID (Plats extern id)
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Locality (Lokal)
    /// </summary>
    public string? Locality { get; set; }

    /// <summary>
    /// Location type (Lokaltyp)
    /// </summary>
    public string? LocationType { get; set; }

    /// <summary>
    /// Location remarks (Lokalkommentar)
    /// </summary>
    public string? LocationRemarks { get; set; }

    /// <summary>
    /// Parish (Församling)
    /// </summary>
    public string? Parish { get; set; }

    /// <summary>
    /// Parish (Församling)
    /// </summary>
    public string? ParishName { get; set; }

    /// <summary>
    /// Parish ID (Församling id)
    /// </summary>
    public string? ParishId { get; set; }

    /// <summary>
    /// Municipality (Kommun)
    /// </summary>
    public string? Municipality { get; set; }

    /// <summary>
    /// Municipality (Kommun)
    /// </summary>
    public string? MunicipalityName { get; set; }

    /// <summary>
    /// Municipality ID (Kommun id)
    /// </summary>
    public string? MunicipalityId { get; set; }

    /// <summary>
    /// County (Län)
    /// </summary>
    public string? County { get; set; }

    /// <summary>
    /// County (Län)
    /// </summary>
    public string? CountyName { get; set; }

    /// <summary>
    /// County ID (Län id)
    /// </summary>
    public string? CountyId { get; set; }

    /// <summary>
    /// Province (Landskap)
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// Province (Landskap)
    /// </summary>
    public string? ProvinceName { get; set; }

    /// <summary>
    /// Province ID (Landskap id)
    /// </summary>
    public string? ProvinceId { get; set; }

    /// <summary>
    /// Country (Land)
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Country (Land)
    /// </summary>
    public string? CountryValue { get; set; }

    /// <summary>
    /// Country ID (Land id)
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// Country Code (Landskod)
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Continent (Kontinent)
    /// </summary>
    public string? Continent { get; set; }

    /// <summary>
    /// Continent ID (Kontinent id)
    /// </summary>
    public int? ContinentId { get; set; }

    /// <summary>
    /// Georeferenced by (Koordinatsatt av)
    /// </summary>
    public string? GeoreferencedBy { get; set; }

    /// <summary>
    /// Georeferenced Date (Koordinatsatt datum)
    /// </summary>
    public string? GeoreferencedDate { get; set; }

    /// <summary>
    /// Georeference remarks (Georeferenskommentar)
    /// </summary>
    public string? GeoreferenceRemarks { get; set; }

    /// <summary>
    /// Coordinate precision (Koordinatprecission)
    /// </summary>
    public double? CoordinatePrecision { get; set; }

    /// <summary>
    /// Water body (Vattenområde)
    /// </summary>
    public string? WaterBody { get; set; }

    /// <summary>
    /// Maximum depth (m) (Maxdjup (m))
    /// </summary>
    public double? MaximumDepthInMeters { get; set; }

    /// <summary>
    /// Minimum depth (m) (Mindjup (m))
    /// </summary>
    public double? MinimumDepthInMeters { get; set; }

    /// <summary>
    /// Maximum elevation (m) (Maxhöjd över havet (m))
    /// </summary>
    public double? MaximumElevationInMeters { get; set; }

    /// <summary>
    /// Minimum elevation (m) (Minhöjd över havet (m))
    /// </summary>
    public double? MinimumElevationInMeters { get; set; }

    /// <summary>
    /// Higher Geography (Överliggande geografi)
    /// </summary>
    public string? HigherGeography { get; set; }

    /// <summary>
    /// Island (Ö)
    /// </summary>
    public string? Island { get; set; }

    /// <summary>
    /// Verbatim coordinates (Ursprungliga koordinater)
    /// </summary>
    public string? VerbatimCoordinates { get; set; }

    /// <summary>
    /// Verbatim latitude (Ursprunglig latitud)
    /// </summary>
    public string? VerbatimLatitude { get; set; }

    /// <summary>
    /// Verbatim longitude (Ursprunglig longitud)
    /// </summary>
    public string? VerbatimLongitude { get; set; }

    /// <summary>
    /// Verbatim coordinate system (Ursprungligt koordinatsystem)
    /// </summary>
    public string? VerbatimCoordinateSystem { get; set; }

    /// <summary>
    /// Verbatim SRS (Ursprunglig SRS)
    /// </summary>
    public string? VerbatimSRS { get; set; }

    /// <summary>
    /// Verbatim locality (Ursprungligt lokalnamn)
    /// </summary>
    public string? VerbatimLocality { get; set; }

    /// <summary>
    /// Location project ID (Plats projekt-id)
    /// </summary>
    public int? ProjectId { get; set; }
}

