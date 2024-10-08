﻿using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Constants;
using SOS.Lib.Database;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Import.LiveIntegrationTests.TestDataTools
{
    public class CreateVocabularyEnumsTool : TestBase
    {
        private string CreateEnums(ICollection<Vocabulary> processedVocabularies)
        {
            var sb = new StringBuilder();
            foreach (var vocabularity in processedVocabularies)
            {
                sb.Append(CreateEnum(vocabularity));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string CreateEnum(Vocabulary vocabulary)
        {
            var sb = new StringBuilder();
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// Enumeration of {vocabulary.Name}.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine($"public enum {vocabulary.Name}Id");
            sb.AppendLine("{");
            foreach (var vocabularyValue in vocabulary.Values)
            {
                if (vocabularyValue.Value == "")
                    continue;

                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"   /// {CapitalizeFirstChar(vocabularyValue.Value)}.");
                if (vocabularyValue.Localized)
                {
                    sb.AppendLine(
                        $"   /// ({CapitalizeFirstChar(vocabularyValue.Translations.Single(m => m.CultureCode == Cultures.sv_SE).Value)})");
                }

                sb.AppendLine("    /// </summary>");
                sb.AppendLine($"    {TrimName(vocabularyValue.Value)} = {vocabularyValue.Id},");
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
                if (input == "") return "";
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
            string[] invalidChars = { ".", ",", "(", ")", "-" };
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
        ///     Reads vocabulary mappings from MongoDb and create enum file.
        /// </summary>
        [Fact]
        [Trait("Category", "Tool")]
        public async Task Create_vocabulary_enum_for_all_field_mapped_fields()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string filePath = @"c:\temp\VocabularyEnums.cs";
            var verbatimDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);

            var vocabularyRepository =
                new VocabularyRepository(processClient, new NullLogger<VocabularyRepository>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var processedVocabularies = (await vocabularyRepository.GetAllAsync()).ToArray();
            var strEnums = CreateEnums(processedVocabularies);
            File.WriteAllText(filePath, strEnums, Encoding.UTF8);
        }
    }
}