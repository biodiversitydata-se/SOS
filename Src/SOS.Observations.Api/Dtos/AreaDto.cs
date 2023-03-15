using MongoDB.Driver.GeoJsonObjectModel;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    ///     Area used external
    /// </summary>
    public class AreaDto : AreaBaseDto
    {
        /// <summary>
        ///     Area geometry
        /// </summary>
        public GeoJsonGeometry<GeoJson2DCoordinates> Geometry { get; set; }
    }
}