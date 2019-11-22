using Dynamitey;
using Newtonsoft.Json.Linq;

namespace SOS.Lib.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Get value from json object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonObject"></param>
        /// <param name="propertyPath"></param>
        /// <returns></returns>
        public static T GetValue<T>(this object jsonObject, string propertyPath)
        {
            var propertyPathParts = propertyPath.Split('.');
            var property = jsonObject;

            // Traverse throw object hierarchy
            foreach (var part in propertyPathParts)
            {
                property = Dynamic.InvokeGet(property, part);
            }

            return (T)((JValue)property).Value;
        }

        /// <summary>
        /// Set property value of json object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonObject"></param>
        /// <param name="propertyPath"></param>
        /// <param name="value"></param>
        public static void SetValue<T>(this object jsonObject, string propertyPath, T value)
        {
            var propertyPathParts = propertyPath.Split('.');
            var property = jsonObject;
            foreach (var part in propertyPathParts)
            {
                property = Dynamic.InvokeGet(property, part);
            }

            ((JValue)property).Value = value;
        }
    }
}
