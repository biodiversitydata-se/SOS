using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// Translation extensions
    /// </summary>
    public static class TranslationExtensions
    {
        /// <summary>
        /// Translate 
        /// </summary>
        /// <param name="translations"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        public static string Translate(this IEnumerable<VocabularyValueTranslation> translations, string cultureCode)
        {
            return translations
                ?.FirstOrDefault(t => t.CultureCode.Equals(cultureCode, StringComparison.CurrentCultureIgnoreCase))
                ?.Value ?? string.Empty;
        }
    }
}
