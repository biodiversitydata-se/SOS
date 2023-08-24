using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SOS.Lib.Models.Shared;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateVocabularyMarkdownTool : TestBase
    {
        public class VocabularyFile
        {
            public string SourceFilePath { get; set; }
            public Vocabulary Vocabulary { get; set; }            
            public string Title { get; set; }
            public string Markdown { get; set; }
        }

        /// <summary>
        ///     Reads vocabularies from MongoDb and saves them as a JSON file.
        /// </summary>
        [Fact]
        [Trait("Category", "Tool")]
        public async Task CreateVocabularyMarkdown()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string outputPath = @"C:\temp\sos-vocabs";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            List<VocabularyFile> vocabularyFiles = GetVocabularyFiles();            
            foreach(var vocabularyFile in vocabularyFiles)
            {
                Debug.WriteLine($"Reading: {vocabularyFile.Title}");
                var str = File.ReadAllText(vocabularyFile.SourceFilePath, Encoding.UTF8);                
                vocabularyFile.Vocabulary = JsonConvert.DeserializeObject<Vocabulary>(str);
                vocabularyFile.Markdown = CreateMarkdown(vocabularyFile.Vocabulary);
            }

            // Create a markdown file for each vocabulary
            //foreach (var vocabularyFile in vocabularyFiles)
            //{
            //    string filePath = Path.Join(outputPath, $"{vocabularyFile.Title}.md");
            //    var sb = new StringBuilder();
            //    sb.AppendLine($"# {vocabularyFile.Title} vocabulary");
            //    sb.Append(vocabularyFile.Markdown);
            //    File.WriteAllText(filePath, sb.ToString());
            //}

            // Add all vocabularies into one file.
            var sbComposite = new StringBuilder();
            sbComposite.AppendLine("# Vocabularies");
            foreach (var vocabularyFile in vocabularyFiles)
            {
                sbComposite.AppendLine($"## {vocabularyFile.Title}");
                sbComposite.Append(vocabularyFile.Markdown);
                sbComposite.AppendLine();
            }
            File.WriteAllText(Path.Join(outputPath, "vocabularies.md"), sbComposite.ToString());            
        }

        private List<VocabularyFile> GetVocabularyFiles()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            List<VocabularyFile> vocabularyFiles = new List<VocabularyFile>()
            {
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/AccessRightsVocabulary.json"),
                    Title = "accessRights"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/ActivityVocabulary.json"),
                    Title = "activity"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/AreaTypeVocabulary.json"),
                    Title = "areaType"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/BasisOfRecordVocabulary.json"),
                    Title = "basisOfRecord"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/BehaviorVocabulary.json"),
                    Title = "behavior"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/BiotopeVocabulary.json"),
                    Title = "biotope"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/BirdNestActivityVocabulary.json"),
                    Title = "birdNestActivity"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/continentVocabulary.json"),
                    Title = "continent"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/countryVocabulary.json"),
                    Title = "country"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/determinationMethodVocabulary.json"),
                    Title = "determinationMethod"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/discoveryMethodVocabulary.json"),
                    Title = "discoveryMethod"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/establishmentMeansVocabulary.json"),
                    Title = "establishmentMeans"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/institutionVocabulary.json"),
                    Title = "institutionCode"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/lifeStageVocabulary.json"),
                    Title = "lifeStage"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/occurrenceStatusVocabulary.json"),
                    Title = "occurrenceStatus"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/reproductiveCondition.json"),
                    Title = "reproductiveCondition"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/sexVocabulary.json"),
                    Title = "sex"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/substrateVocabulary.json"),
                    Title = "substrate"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/sensitivityCategoryVocabulary.json"),
                    Title = "protectionLevel"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/typeVocabulary.json"),
                    Title = "type"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/unitVocabulary.json"),
                    Title = "unit"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/verificationStatusVocabulary.json"),
                    Title = "validationStatus"
                },
                new VocabularyFile
                {
                    SourceFilePath = Path.Combine(assemblyPath, @"Resources/Vocabularies/taxonCategoryVocabulary.json"),
                    Title = "taxonCategory"
                }
            };

            return vocabularyFiles;
        }
        private string CreateMarkdown(Vocabulary vocabulary)
        {
            bool useTranslation = vocabulary.Localized;
            bool useCategory = vocabulary.Values.First().Category != null;
            return CreateMarkdown(vocabulary, useTranslation, useCategory, true);
        }

        private string CreateMarkdown(Vocabulary vocabulary, bool useTranslation, bool useCategory, bool useCategoryTranslation)
        {
            var sb = new StringBuilder();
            if (useTranslation && useCategory && !useCategoryTranslation)
            {
                sb.AppendLine("| Id | Value (Swedish) | Value (English) | Category |");
                sb.AppendLine("|:---	|:---	|:---	|:---	|");
                foreach (var item in vocabulary.Values)
                {
                    sb.AppendLine($"| {item.Id} | {item.Translations.Single(m => m.CultureCode == "sv-SE").Value} | {item.Translations.Single(m => m.CultureCode == "en-GB").Value} | {item.Category.Name} |");
                }
            }
            else if (useTranslation && useCategory && useCategoryTranslation)
            {
                sb.AppendLine("| Id | Value (Swedish) | Value (English) | Category (Swedish) | Category (English) |");
                sb.AppendLine("|:---	|:---	|:---	|:---	|:---	|");
                foreach (var item in vocabulary.Values)
                {
                    sb.AppendLine($"| {item.Id} | {item.Translations.Single(m => m.CultureCode == "sv-SE").Value} | {item.Translations.Single(m => m.CultureCode == "en-GB").Value} | {item.Category.Translations.Single(m => m.CultureCode == "sv-SE").Value} | {item.Category.Translations.Single(m => m.CultureCode == "en-GB").Value}");
                }
            }
            else if (useTranslation && !useCategory)
            {
                sb.AppendLine("| Id | Value (Swedish) | Value (English) |");
                sb.AppendLine("|:---	|:---	|:---	|");
                foreach (var item in vocabulary.Values)
                {
                    sb.AppendLine($"| {item.Id} | {item.Translations.Single(m => m.CultureCode == "sv-SE").Value} | {item.Translations.Single(m => m.CultureCode == "en-GB").Value} |");
                }
            }
            else if (!useTranslation && !useCategory)
            {
                sb.AppendLine("| Id | Value |");
                sb.AppendLine("|:---	|:---	|");
                foreach (var item in vocabulary.Values)
                {
                    sb.AppendLine($"| {item.Id} | {item.Value} |");
                }
            }
            else if (!useTranslation && useCategory)
            {
                sb.AppendLine("| Id | Value | Category |");
                sb.AppendLine("|:---	|:---	|:---	|");
                foreach (var item in vocabulary.Values)
                {
                    sb.AppendLine($"| {item.Id} | {item.Value} | {item.Category.Name }");
                }
            }

            return sb.ToString();
        }
    }
}