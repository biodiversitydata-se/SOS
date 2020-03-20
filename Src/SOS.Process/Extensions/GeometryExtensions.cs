using MongoDB.Bson;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Extensions
{
    /// <summary>
    /// Extensions for geometries
    /// </summary>
    public static class GeometryExtensions
    {
        public static IFeature ToFeature(this Area area)
        {
            var reader = new NetTopologySuite.IO.GeoJsonReader();
            var attributes = new AttributesTable();
            attributes.Add("id", area.Id);
            attributes.Add("name", area.Name);
            attributes.Add("areaType", area.AreaType);
            attributes.Add("featureId", area.FeatureId);

            return new Feature { Geometry = reader.Read<Geometry>(area.Geometry.ToJson()), Attributes = attributes };
        }
    }
}
