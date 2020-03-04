using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.FieldMappings;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.TestHelpers.JsonConverters;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateFieldMappingEnumsTool : TestBase
    {
        /// <summary>
        /// Reads field mappings from MongoDb and create enum file.
        /// </summary>
        [Fact]
        [Trait("Category", "Tool")]
        public async Task Create_field_mapping_enum_for_all_field_mapped_fields()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string filePath = @"c:\temp\FieldMappingEnums.cs";
            const int batchSize = 50000;
            var importConfiguration = GetImportConfiguration();
            var importClient = new ImportClient(
                importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                importConfiguration.VerbatimDbConfiguration.DatabaseName,
                batchSize);

            var fieldMappingRepository =
                new FieldMappingRepository(importClient, new NullLogger<FieldMappingRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var fieldMappings = (await fieldMappingRepository.GetAllAsync()).ToArray();
            var strEnums = CreateEnums(fieldMappings);
            System.IO.File.WriteAllText(filePath, strEnums, Encoding.UTF8);
        }

        private string CreateEnums(ICollection<FieldMapping> fieldMappings)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var fieldMapping in fieldMappings)
            {
                if (fieldMapping.Id == FieldMappingFieldId.Municipality ||
                    fieldMapping.Id == FieldMappingFieldId.Parish)
                    continue;

                sb.Append(CreateEnum(fieldMapping));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string CreateEnum(FieldMapping fieldMapping)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// Enumeration of {fieldMapping.Name}.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine($"public enum {fieldMapping.Name}Id");
            sb.AppendLine("{");
            foreach (var fieldMappingValue in fieldMapping.Values)
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"   /// {CapitalizeFirstChar(fieldMappingValue.Name)}.");
                if (fieldMappingValue.Localized)
                {
                    sb.AppendLine($"   /// ({CapitalizeFirstChar(fieldMappingValue.Translations.Single(m => m.CultureCode == Cultures.sv_SE).Value)})");
                }
                sb.AppendLine("    /// </summary>");
                sb.AppendLine($"    {TrimName(fieldMappingValue.Name)} = {fieldMappingValue.Id},");
                sb.AppendLine();
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string TrimName(string name)
        {
            var textInfo = System.Globalization.CultureInfo.InvariantCulture.TextInfo;
            var str = ReplaceInvalidChars(name);
            str = textInfo.ToTitleCase(str);
            str = RemoveWhitespace(str);
            str = CapitalizeFirstChar(str);
            return str;
        }

        private string CapitalizeFirstChar(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }
        public string ReplaceInvalidChars(string input)
        {
            string str = input;
            string[] invalidChars = {".", ",", "(", ")","-"};
            foreach (var invalidChar in invalidChars)
            {
                str = str.Replace(invalidChar, "");
            }

            str = str.Replace("/", " Or ")
                .Replace("Å", "A")
                .Replace("å", "a")
                .Replace("Ä", "A")
                .Replace("ä", "a")
                .Replace("Ö", "O")
                .Replace("ö", "o");

            return str;
        }

        private string RemoveWhitespace(string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }
    }
}