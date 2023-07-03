using System;
using System.Globalization;
using System.Reflection;

namespace SOS.Lib.Extensions
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Get version number
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetVersionNumber(this Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value.Substring(0, index);
                    return value;
                }
            }

            return default;
        }

        /// <summary>
        /// Get build date
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static DateTime GetBuildDate(this Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                }
            }

            return default;
        }
    }
}
