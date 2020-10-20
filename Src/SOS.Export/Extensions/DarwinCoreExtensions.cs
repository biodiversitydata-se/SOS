using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SOS.Export.Models.DarwinCore;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Export.Extensions
{
    /// <summary>
    ///     Extensions for Darwin Core
    /// </summary>
    public static class DarwinCoreExtensions
    {
        /// <summary>
        ///     Cast processed Darwin Core object to Darwin Core Archive
        /// </summary>
        /// <param name="processedDarwinCore"></param>
        /// <returns></returns>
        public static DwC ToDarwinCoreArchive(this DarwinCore processedDarwinCore)
        {
            if (processedDarwinCore == null)
            {
                return null;
            }

            return new DwC
            {
                AccessRights = processedDarwinCore.AccessRights,
                BasisOfRecord = processedDarwinCore.BasisOfRecord,
                BibliographicCitation = processedDarwinCore.BibliographicCitation,
                CollectionCode = processedDarwinCore.CollectionCode,
                CollectionID = processedDarwinCore.CollectionID,
                DataGeneralizations = processedDarwinCore.DataGeneralizations,
                DatasetID = processedDarwinCore.DatasetID,
                DatasetName = processedDarwinCore.DatasetName,
                DynamicProperties = JsonConvert.SerializeObject(processedDarwinCore.DynamicProperties),
                InformationWithheld = processedDarwinCore.InformationWithheld,
                InstitutionCode = processedDarwinCore.InstitutionCode,
                InstitutionID = processedDarwinCore.InstitutionID,
                Language = processedDarwinCore.Language,
                Modified = processedDarwinCore.Modified,
                References = processedDarwinCore.References,
                Rights = processedDarwinCore.Rights,
                RightsHolder = processedDarwinCore.RightsHolder,
                Type = processedDarwinCore.Type
            };
        }

        /// <summary>
        ///     Cast processed Darwin Core objects to Darwin Core
        /// </summary>
        /// <param name="processedDarwinCore"></param>
        /// <returns></returns>
        public static IEnumerable<DwC> ToDarwinCoreArchive(
            this IEnumerable<DarwinCore> processedDarwinCore)
        {
            return processedDarwinCore?.Select(m => m.ToDarwinCoreArchive());
        }
    }
}