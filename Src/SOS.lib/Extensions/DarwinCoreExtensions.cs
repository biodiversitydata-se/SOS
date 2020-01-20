using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// Extensions for Darwin Core
    /// </summary>
    public static class DarwinCoreExtensions
    {
        /// <summary>
        /// Cast DarwinCoreVernacularNames to TaxonVernacularNames.
        /// </summary>
        /// <param name="darwinCoreVernacularNames"></param>
        /// <returns></returns>
        public static IEnumerable<TaxonVernacularName> ToTaxonVernacularNames(this IEnumerable<DarwinCoreVernacularName> darwinCoreVernacularNames)
        {
            return darwinCoreVernacularNames?.Select(m => m.ToTaxonVernacularName());
        }

        /// <summary>
        /// Cast DarwinCoreVernacularName object to TaxonVernacularName.
        /// </summary>
        /// <param name="darwinCoreVernacularName"></param>
        /// <returns></returns>
        public static TaxonVernacularName ToTaxonVernacularName(this DarwinCoreVernacularName darwinCoreVernacularName)
        {
            return new TaxonVernacularName
            {
                CountryCode = darwinCoreVernacularName.CountryCode,
                IsPreferredName = darwinCoreVernacularName.IsPreferredName,
                Language = darwinCoreVernacularName.Language,
                Name = darwinCoreVernacularName.VernacularName
            };
        }
    }
}
