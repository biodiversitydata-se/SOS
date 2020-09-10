using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Import.Repositories.Destination.FieldMappings;
using SOS.Lib.Constants;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateFieldMappingEnumsTool : TestBase
    {
        private string CreateEnums(ICollection<FieldMapping> fieldMappings)
        {
            var sb = new StringBuilder();
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
            var sb = new StringBuilder();
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// Enumeration of {fieldMapping.Name}.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine($"public enum {fieldMapping.Name}Id");
            sb.AppendLine("{");
            foreach (var fieldMappingValue in fieldMapping.Values)
            {
                if (fieldMappingValue.Value == "")
                    continue;

                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"   /// {CapitalizeFirstChar(fieldMappingValue.Value)}.");
                if (fieldMappingValue.Localized)
                {
                    sb.AppendLine(
                        $"   /// ({CapitalizeFirstChar(fieldMappingValue.Translations.Single(m => m.CultureCode == Cultures.sv_SE).Value)})");
                }

                sb.AppendLine("    /// </summary>");
                sb.AppendLine($"    {TrimName(fieldMappingValue.Value)} = {fieldMappingValue.Id},");
                sb.AppendLine();
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private string TrimName(string name)
        {
            var textInfo = CultureInfo.InvariantCulture.TextInfo;
            var str = ReplaceInvalidChars(name);
            if (str.Split(" ").Length > 1)
            {
                str = textInfo.ToTitleCase(str);
            }

            str = RemoveWhitespace(str);
            str = CapitalizeFirstChar(str);
            return str;
        }

        private string CapitalizeFirstChar(string input)
        {
            try
            {
                return input.First().ToString().ToUpper() + input.Substring(1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public string ReplaceInvalidChars(string input)
        {
            var str = input;
            string[] invalidChars = {".", ",", "(", ")", "-"};
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

        /// <summary>
        ///     Reads field mappings from MongoDb and create enum file.
        /// </summary>
        [Fact]
        [Trait("Category", "Tool")]
        public async Task Create_field_mapping_enum_for_all_field_mapped_fields()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string filePath = @"c:\temp\FieldMappingEnums.cs";
            var verbatimDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient (
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize); 

            var fieldMappingRepository =
                new FieldMappingRepository(processClient, new NullLogger<FieldMappingRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var fieldMappings = (await fieldMappingRepository.GetAllAsync()).ToArray();
            var strEnums = CreateEnums(fieldMappings);
            File.WriteAllText(filePath, strEnums, Encoding.UTF8);
        }
    }
}