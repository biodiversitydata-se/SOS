using Newtonsoft.Json;
using SOS.Export.Models.DarwinCore;
using SOS.Lib.Models.DarwinCore;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Export.Extensions;

/// <summary>
///     Extensions for Darwin Core
/// </summary>
public static class DarwinCoreExtensions
{
    extension(DarwinCore processedDarwinCore)
    {
        /// <summary>
        ///     Cast processed Darwin Core object to Darwin Core Archive
        /// </summary>
        /// <param name="processedDarwinCore"></param>
        /// <returns></returns>
        public DwC ToDarwinCoreArchive()
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
    }

    extension(IEnumerable<DarwinCore> processedDarwinCore)
    {
        /// <summary>
        ///     Cast processed Darwin Core objects to Darwin Core
        /// </summary>
        /// <param name="processedDarwinCore"></param>
        /// <returns></returns>
        public IEnumerable<DwC> ToDarwinCoreArchive(
    )
        {
            return processedDarwinCore?.Select(m => m.ToDarwinCoreArchive());
        }
    }
}