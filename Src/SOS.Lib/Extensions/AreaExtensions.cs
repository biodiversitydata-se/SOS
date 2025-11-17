using SOS.Lib.Enums;
using System.Linq;

namespace SOS.Lib.Extensions;

public static class AreaExtensions
{
    extension(AreaType type)
    {
        /// <summary>
        /// Check if this type has Grid Geometry
        /// </summary>
        /// <returns></returns>
        public bool HasGridGeometry() => new[] { AreaType.Atlas10x10, AreaType.Atlas5x5 }.Contains(type);

        /// <summary>
        /// Create string area id from type and feature id
        /// </summary>
        /// <param name="featureId"></param>
        /// <returns></returns>
        public string ToAreaId(string featureId)
        {
            return $"{type}:{featureId}";
        }
    }
}