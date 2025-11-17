using SOS.Lib.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Extensions;

/// <summary>
/// Translation extensions
/// </summary>
public static class TranslationExtensions
{
    extension(IEnumerable<VocabularyValueTranslation> translations)
    {
        /// <summary>
        /// Translate 
        /// </summary>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        public string Translate(string cultureCode)
        {
            return translations
                ?.FirstOrDefault(t => t.CultureCode.Equals(cultureCode, StringComparison.CurrentCultureIgnoreCase))
                ?.Value ?? string.Empty;
        }
    }
}
