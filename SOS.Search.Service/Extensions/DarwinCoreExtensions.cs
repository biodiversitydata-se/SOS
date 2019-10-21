using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Search.Service.Extensions
{
    /// <summary>
    /// Extensions for Darwin Core
    /// </summary>
    public static class DarwinCoreExtensions
    {
        /// <summary>
        /// Cast processed Darwin Core object to Darwin Core
        /// </summary>
        /// <param name="processedDarwinCore"></param>
        /// <returns></returns>
        public static DarwinCore<string> ToDarwinCore(this DarwinCore<DynamicProperties> processedDarwinCore)
        {
            if (processedDarwinCore == null)
            {
                return null;
            }

            return new DarwinCore<string>
            {
                AccessRights = processedDarwinCore.AccessRights,
                BasisOfRecord = processedDarwinCore.BasisOfRecord,
                BibliographicCitation = processedDarwinCore.BasisOfRecord,
                CollectionCode = processedDarwinCore.CollectionCode,
                CollectionID = processedDarwinCore.CollectionID,
                DataGeneralizations = processedDarwinCore.DataGeneralizations,
                DatasetID = processedDarwinCore.DatasetID,
                DatasetName = processedDarwinCore.DatasetName,
                DynamicProperties = JsonConvert.SerializeObject(processedDarwinCore.DynamicProperties),
                Event = processedDarwinCore.Event,
                GeologicalContext = processedDarwinCore.GeologicalContext,
                Identification = processedDarwinCore.Identification,
                InformationWithheld = processedDarwinCore.InformationWithheld,
                InstitutionCode = processedDarwinCore.InstitutionCode,
                InstitutionID = processedDarwinCore.InstitutionID,
                Language = processedDarwinCore.Language,
                Location = processedDarwinCore.Location,
                MeasurementOrFact = processedDarwinCore.MeasurementOrFact,
                Modified = processedDarwinCore.Modified,
                Occurrence = processedDarwinCore.Occurrence,
                OwnerInstitutionCode = processedDarwinCore.OwnerInstitutionCode,
                References = processedDarwinCore.References,
                ResourceRelationship = processedDarwinCore.ResourceRelationship,
                Rights = processedDarwinCore.Rights,
                RightsHolder = processedDarwinCore.RightsHolder,
                Taxon = processedDarwinCore.Taxon,
                Type = processedDarwinCore.Type
            };
        }

        /// <summary>
        ///  Cast processed Darwin Core objects to Darwin Core 
        /// </summary>
        /// <param name="processedDarwinCore"></param>
        /// <returns></returns>
        public static IEnumerable<DarwinCore<string>> ToDarwinCore(this IEnumerable<DarwinCore<DynamicProperties>> processedDarwinCore)
        {
            return processedDarwinCore?.Select(m => m.ToDarwinCore());
        }
    }
}
