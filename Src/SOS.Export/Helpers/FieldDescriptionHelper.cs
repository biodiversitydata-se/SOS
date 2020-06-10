using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using SOS.Export.Enums;
using SOS.Export.Models;

namespace SOS.Export.Helpers
{
    // todo - move to SOS.Lib
    /// <summary>
    ///     Field descriptions.
    /// </summary>
    public static class FieldDescriptionHelper
    {
        private static readonly List<FieldDescription> AllFields;
        private static readonly Dictionary<string, FieldDescription> AllFieldsByName;
        private static readonly Dictionary<int, FieldDescription> AllFieldsById;
        private static readonly Dictionary<FieldDescriptionId, FieldDescription> AllFieldsByFieldDescriptionId;

        private static readonly FieldDescriptionId[] MandatoryDwcFields =
        {
            FieldDescriptionId.OccurrenceID,
            FieldDescriptionId.BasisOfRecord,
            FieldDescriptionId.InstitutionCode,
            FieldDescriptionId.CollectionCode,
            FieldDescriptionId.CatalogNumber,
            FieldDescriptionId.ScientificName,
            FieldDescriptionId.DecimalLongitude,
            FieldDescriptionId.DecimalLatitude,
            FieldDescriptionId.GeodeticDatum,
            FieldDescriptionId.CoordinateUncertaintyInMeters,
            FieldDescriptionId.EventDate
        };

        static FieldDescriptionHelper()
        {
            AllFields = LoadFieldDescriptions().ToList();
            AllFieldsByName = AllFields.ToDictionary(x => x.Name, x => x);
            AllFieldsById = AllFields.ToDictionary(x => x.Id, x => x);
            AllFieldsByFieldDescriptionId = AllFields.ToDictionary(x => (FieldDescriptionId) x.Id, x => x);
        }

        private static List<FieldDescription> LoadFieldDescriptions()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, @"Resources\FieldDescriptions.json");
            using var fs = FileSystemHelper.WaitForFileAndThenOpenIt(filePath);
            var fields = JsonSerializer.DeserializeAsync<List<FieldDescription>>(fs).Result;
            return fields;
        }

        public static IEnumerable<FieldDescription> GetDefaultDwcExportFieldDescriptions()
        {
            var fieldIds = GetDefaultDwcExportFieldDescriptionIds();
            return GetFieldDescriptions(fieldIds);
        }

        public static IEnumerable<FieldDescriptionId> GetDefaultDwcExportFieldDescriptionIds()
        {
            return AllFields
                .Where(x => x.IncludedByDefaultInDwcExport)
                .Select(x => (FieldDescriptionId) x.Id)
                .AddMandatoryFieldDescriptionIdsFirst();
        }

        public static IEnumerable<FieldDescription> GetAllFieldDescriptions()
        {
            return AllFields;
        }

        public static IEnumerable<FieldDescription> AddMandatoryFieldDescriptions(
            IEnumerable<FieldDescriptionId> fieldDescriptions)
        {
            var fieldIds = AddMandatoryFieldDescriptionIds(fieldDescriptions);
            return AllFieldsByFieldDescriptionId
                .Where(x => fieldIds.Contains(x.Key))
                .Select(x => x.Value);
        }

        public static IEnumerable<FieldDescription> GetFieldDescriptions(
            IEnumerable<FieldDescriptionId> fieldDescriptionIds)
        {
            var list = fieldDescriptionIds.ToList();
            return AllFieldsByFieldDescriptionId
                .Where(x => list.Contains(x.Key))
                .Select(x => x.Value)
                .OrderBy(x => x.Id);
        }

        public static IEnumerable<FieldDescriptionId> AddMandatoryFieldDescriptionIds(
            IEnumerable<FieldDescriptionId> fieldDescriptions)
        {
            return MandatoryDwcFields.Union(fieldDescriptions);
        }

        public static IEnumerable<FieldDescriptionId> AddMandatoryFieldDescriptionIdsFirst(
            this IEnumerable<FieldDescriptionId> fieldDescriptions)
        {
            return MandatoryDwcFields.Union(fieldDescriptions);
        }

        public static FieldDescription GetFieldDescription(FieldDescriptionId fieldDescriptionId)
        {
            return AllFieldsByFieldDescriptionId[fieldDescriptionId];
        }

        public static IEnumerable<FieldDescriptionId> GetMissingFieldDescriptionIds(
            IEnumerable<FieldDescriptionId> fieldDescriptionIds)
        {
            return AllFields.Select(x => (FieldDescriptionId) x.Id).Except(fieldDescriptionIds);
        }

        /// <summary>
        /// Create a bool array that holds data about whether a field should be written to a DwC-A occurrence CSV file.
        /// </summary>
        /// <param name="fieldDescriptions"></param>
        /// <remarks>A bool array is much faster to use than using a Dictionary of {FieldDescriptionId, bool}</remarks>
        /// <returns></returns>
        public static bool[] CreateWriteFieldsArray(IEnumerable<FieldDescription> fieldDescriptions)
        {
            var fieldDescriptionIdsSet = new HashSet<FieldDescriptionId>();
            foreach (var fieldDescription in fieldDescriptions)
            {
                fieldDescriptionIdsSet.Add((FieldDescriptionId) fieldDescription.Id);
            }

            bool[] writeField = new bool[AllFields.Max(f => f.Id) +1];
            foreach (var field in AllFields.OrderBy(m => m.Id))
            {
                if (fieldDescriptionIdsSet.Contains(field.FieldDescriptionId))
                {
                    writeField[field.Id] = true;
                }
            }

            return writeField;
        }

        /// <summary>
        /// Get a subset of important DwC fields used for testing purpose. It's easier to debug data and the exports are faster.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<FieldDescription> GetDwcFieldDescriptionsForTestingPurpose()
        {
            return GetFieldDescriptions(DwcFieldDescriptionForTestingPurpose);
        }

        private static readonly FieldDescriptionId[] DwcFieldDescriptionForTestingPurpose = {
            FieldDescriptionId.OccurrenceID,
            FieldDescriptionId.BasisOfRecord,
            FieldDescriptionId.ScientificName,
            FieldDescriptionId.Kingdom,
            FieldDescriptionId.DecimalLongitude,
            FieldDescriptionId.DecimalLatitude,
            FieldDescriptionId.CoordinateUncertaintyInMeters,
            FieldDescriptionId.GeodeticDatum,
            FieldDescriptionId.EventDate,
            FieldDescriptionId.EventTime,
            FieldDescriptionId.CollectionCode,
            FieldDescriptionId.DatasetName,
            FieldDescriptionId.RecordedBy,
            FieldDescriptionId.LifeStage,
            FieldDescriptionId.IdentificationVerificationStatus,
            FieldDescriptionId.SamplingProtocol,
            FieldDescriptionId.Country,
            FieldDescriptionId.InstitutionCode,
            FieldDescriptionId.CatalogNumber
        };
    }
}