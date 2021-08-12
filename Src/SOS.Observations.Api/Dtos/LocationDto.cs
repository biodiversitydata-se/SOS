using Nest;

namespace SOS.Observations.Api.Dtos
{
    public class LocationDto
    {
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
        public IdValueDto<int> Continent { get; set; }

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
        public IdValueDto<int> Country { get; set; }

        /// <summary>
        ///     The standard code for the country in which the
        ///     Location occurs.
        ///     Recommended best practice is to use ISO 3166-1-alpha-2
        ///     country codes.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        ///     The full, unabbreviated name of the next smaller
        ///     administrative region than stateProvince(county, shire,
        ///     department, etc.) in which the Location occurs
        ///     ('län' in swedish).
        /// </summary>
        public IdValueDto<string> County { get; set; }

        /// <summary>
        ///     The full, unabbreviated name of the next smaller
        ///     administrative region than county (city, municipality, etc.)
        ///     in which the Location occurs.
        /// </summary>
        public IdValueDto<string> Municipality { get; set; }

        /// <summary>
        ///     Parish ('socken' in swedish).
        /// </summary>
        public IdValueDto<string> Parish { get; set; }

        /// <summary>
        ///     Darwin Core term name: stateProvince.
        ///     The name of the next smaller administrative region than
        ///     country (state, province, canton, department, region, etc.)
        ///     in which the Location occurs.
        ///     ('landskap' in swedish).
        /// </summary>
        public IdValueDto<string> Province { get; set; }

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
        ///     Point (WGS84).
        /// </summary>
        public PointGeoShape Point { get; set; }

        /// <summary>
        ///     Point with accuracy buffer (WGS84).
        /// </summary>
        public PolygonGeoShape PointWithBuffer { get; set; }

        /// <summary>
        /// Point with disturbance buffer
        /// </summary>
        public PolygonGeoShape PointWithDisturbanceBuffer { get; set; }
    }
}
