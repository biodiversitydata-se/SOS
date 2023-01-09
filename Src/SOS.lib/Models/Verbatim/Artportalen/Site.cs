using System.Collections.Generic;

using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    /// <summary>
    ///     Site object
    /// </summary>
    public class Site
    {
        /// <summary>
        ///     Accuracy in meters
        /// </summary>
        public int Accuracy { get; set; }

        /// <summary>
        /// Bird validation areas
        /// </summary>
        public ICollection<string> BirdValidationAreaIds { get; set; }

        /// <summary>
        ///     Country Region
        /// </summary>
        public GeographicalArea CountryRegion { get; set; }

        /// <summary>
        ///     County of site
        /// </summary>
        public GeographicalArea County { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CountyPartIdByCoordinate { get; set; }

        /// <summary>
        ///     Diffused Point (WGS84)
        /// </summary>
        public GeoJsonGeometry DiffusedPoint { get; set; }

        /// <summary>
        ///   Diffused Point with accuracy buffer (WGS84)
        /// </summary>
        public GeoJsonGeometry DiffusedPointWithBuffer { get; set; }

        /// <summary>
        ///     Id of diffusion, 0 if no diffusion
        /// </summary>
        public int DiffusionFactor { get; set; }

        /// <summary>
        ///     External Id of site
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Tru if site has a geometry 
        /// </summary>
        public bool HasGeometry { get; set; }

        /// <summary>
        ///     Id of site
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Included by site id property
        /// </summary>
        public int? IncludedBySiteId { get; set; }

        /// <summary>
        /// Bird sites are public
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        ///     Name of site
        /// </summary>
        public GeographicalArea Municipality { get; set; }

        /// <summary>
        ///     Name of site
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Parish
        /// </summary>
        public GeographicalArea Parish { get; set; }

        /// <summary>
        ///     Presentation name Parish
        /// </summary>
        public string PresentationNameParishRegion { get; set; }

        /// <summary>
        ///     Point (WGS84)
        /// </summary>
        public GeoJsonGeometry Point { get; set; }

        /// <summary>
        ///     Point with accuracy buffer (WGS84)
        /// </summary>
        public GeoJsonGeometry PointWithBuffer { get; set; }

        /// <summary>
        ///     Protected Nature
        /// </summary>
        public GeographicalArea ProtectedNature { get; set; }

        /// <summary>
        ///     Province
        /// </summary>
        public GeographicalArea Province { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProvincePartIdByCoordinate { get; set; }
        
        /// <summary>
        /// Id of project
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        ///     Special Protection Area, Natura 2000, Birds Directive
        /// </summary>
        public GeographicalArea SpecialProtectionArea { get; set; }

        /// <summary>
        ///     X coordinate of site
        /// </summary>
        public int XCoord { get; set; }

        /// <summary>
        ///     Y coordinate of site
        /// </summary>
        public int YCoord { get; set; }

        public CoordinateSys VerbatimCoordinateSystem { get; set; }

        /// <summary>
        ///     Water Area
        /// </summary>
        public GeographicalArea WaterArea { get; set; }

        /// <summary>
        ///     The parent site id
        /// </summary>
        public int? ParentSiteId { get; set; }

        /// <summary>
        ///     Name of the parent site
        /// </summary>
        public string ParentSiteName { get; set; }
    }
}