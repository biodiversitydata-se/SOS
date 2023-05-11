using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Helpers
{
    /// <summary>
    /// Helper class for mapping culture codes.
    /// </summary>
    public static class CultureCodeHelper
    {
        public const string DefaultCultureCode = "sv-SE";
        public const int SwedishCultureId = 175;
        public const int EnglishCultureId = 49;

        /// <summary>
        /// Resolve culture code.
        /// </summary>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        public static string GetCultureCode(string cultureCode)
        {
            if (cultureCode == null) return DefaultCultureCode;
            string code = cultureCode.ToLowerInvariant();
            if (CultureCodeMappings.TryGetValue(code, out var resolvedCultureCode))
            {
                return resolvedCultureCode;
            }

            return DefaultCultureCode;
        }

        /// <summary>
        /// Get culture id.
        /// </summary>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        public static int GetCultureId(string cultureCode)
        {
            string resolvedCultureCode = GetCultureCode(cultureCode);
            if (resolvedCultureCode == "en-GB") return EnglishCultureId;
            return SwedishCultureId;
        }

        private static readonly Dictionary<string, string> CultureCodeMappings = new()
        {
            { "sv-se", "sv-SE" },
            { "sv", "sv-SE" },
            { "swe", "sv-SE" },
            { "en-gb", "en-GB" },
            { "en-us", "en-GB" },
            { "en", "en-GB" },
            { "eng", "en-GB" }
        };
    }
}