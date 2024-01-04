﻿using SOS.Lib.Models.Shared;
using System;
using System.Linq;

namespace SOS.Lib.Extensions
{
    public static class MetadataExtensions
    {
        /// <summary>
        ///     Try to get translation
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="cultures">In preferred order. Return value on first match</param>
        /// <returns></returns>
        public static string Translate<T>(this Metadata<T> metadata, params string[] cultures)
        {
            if (!(metadata?.Translations?.Any() ?? false) || !(cultures?.Any() ?? false))
            {
                return null;
            }

            foreach (var culture in cultures)
            {
                var translation = metadata.Translations.FirstOrDefault(t =>
                    t.Culture.Equals(culture, StringComparison.CurrentCultureIgnoreCase));

                if (translation != null)
                {
                    return translation.Value;
                }
            }

            return null;
        }
    }
}