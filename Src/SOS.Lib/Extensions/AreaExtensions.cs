using SOS.Lib.Enums;
using System.Linq;

namespace SOS.Lib.Extensions
{
    public static class AreaExtensions
    {
        /// <summary>
        /// Check if this type has Grid Geometry
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HasGridGeometry(this AreaType type) => new[] { AreaType.Atlas10x10, AreaType.Atlas5x5 }.Contains(type);

        /// <summary>
        /// Create string area id from type and feature id
        /// </summary>
        /// <param name="type"></param>
        /// <param name="featureId"></param>
        /// <returns></returns>
        public static string ToAreaId(this AreaType type, string featureId)
        {
            return $"{type}:{featureId}";
        }
    }
}