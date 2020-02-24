using MongoDB.Driver.GeoJsonObjectModel;

namespace SOS.Lib.Models.Verbatim.SpeciesPortal
{
    /// <summary>
    /// Site object
    /// </summary>
    public class Site
    {
        /// <summary>
        /// Accuracy in meters
        /// </summary>
        public int Accuracy { get; set; }

        /// <summary>
        /// County of site
        /// </summary>
        public GeographicalArea County { get; set; }

        /// <summary>
        /// Countyry part of site
        /// </summary>
        public GeographicalArea CountryPart { get; set; }

        /// <summary>
        /// Id of site
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of site
        /// </summary>
        public GeographicalArea Municipality { get; set; }

        /// <summary>
        /// Parish
        /// </summary>
        public GeographicalArea Parish { get; set; }

        /// <summary>
        /// Point (WGS84)
        /// </summary>
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Point { get; set; }

        /// <summary>
        /// Point with accuracy buffer (WGS84)
        /// </summary>
        public GeoJsonGeometry<GeoJson2DGeographicCoordinates> PointWithBuffer { get; set; }

        /// <summary>
        /// Province
        /// </summary>
        public GeographicalArea Province { get; set; }

        /// <summary>
        /// Name of site
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// X coordinate of site
        /// </summary>
        public int XCoord { get; set; }


        /// <summary>
        /// Y coordinate of site
        /// </summary>
        public int YCoord { get; set; }
    }
}
