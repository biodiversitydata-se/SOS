using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using SOS.Export.Enums;
using SOS.Export.Models;

namespace SOS.Export.Helpers
{
    /// <summary>
    /// Field descriptions.
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
            FieldDescriptionId.CoordinateUncertaintyInMeters,
            FieldDescriptionId.Year,
            FieldDescriptionId.Month,
            FieldDescriptionId.Day
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
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, @"Resources\FieldDescriptions.json");
            using FileStream fs = FileSystemHelper.WaitForFileAndThenOpenIt(filePath);
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
                .Select(x => (FieldDescriptionId)x.Id)
                .AddMandatoryFieldDescriptionIdsFirst();
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
                .OrderBy(x => list.IndexOf((FieldDescriptionId)x.Id));
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

        public static IEnumerable<FieldDescriptionId> GetMissingFieldDescriptionIds(IEnumerable<FieldDescriptionId> fieldDescriptionIds)
        {
            return AllFields.Select(x => (FieldDescriptionId) x.Id).Except(fieldDescriptionIds);
        }
    }
}