using SOS.Lib.Enums;

namespace SOS.Lib.Extensions
{
    public static class AreaExtensions
    {
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