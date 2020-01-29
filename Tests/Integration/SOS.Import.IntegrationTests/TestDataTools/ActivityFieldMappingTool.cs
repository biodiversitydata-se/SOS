using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using Newtonsoft.Json;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class ActivityFieldMappingTool
    {
        [Fact]
        [Trait("Category", "Tool")]
        public void Create_activity_field_mapping_json_file_from_csv_file()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string filename = @"Resources\SourceSystems\ArtportalenActivityValues.csv"; // This file is created by export to CSV from SQL Server.

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            List<IFieldMappingValue> fieldMappingValues = ReadActivityValuesCsvFile(filename).Cast<IFieldMappingValue>().ToList();
            FieldMapping fieldMapping = CreateFieldMapping(fieldMappingValues);
            WriteToFile(fieldMapping, @"c:\temp\ActivityFieldMapping.json");
        }

        private static FieldMapping CreateFieldMapping(List<IFieldMappingValue> fieldMappingValues)
        {
            FieldMapping fieldMapping = new FieldMapping
            {
                Id = 1,
                Name = "ActivityId",
                Values = fieldMappingValues,
                DataProviderTypeFieldMappings = new List<DataProviderTypeFieldMapping>()
            };
            DataProviderTypeFieldMapping artportalenMappings = new DataProviderTypeFieldMapping
            {
                Id = 0,
                Name = "Artportalen",
                Mappings = new List<DataProviderTypeFieldValueMapping>()
            };
            foreach (var fieldMappingValue in fieldMappingValues)
            {
                artportalenMappings.Mappings.Add(new DataProviderTypeFieldValueMapping
                {
                    SosId = fieldMappingValue.Id,
                    Value = fieldMappingValue.Id
                });
            }

            fieldMapping.DataProviderTypeFieldMappings.Add(artportalenMappings);
            return fieldMapping;
        }

        private void WriteToFile(object obj, string filePath)
        {
            var jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(filePath, jsonString, Encoding.UTF8);
        }

        private List<FieldMappingWithCategoryValue> ReadActivityValuesCsvFile(string filename)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, filename);
            List<dynamic> records = ReadRecordsFromCsvFile(filePath);
            var fieldMappings = new List<FieldMappingWithCategoryValue>();
            foreach (var group in records.GroupBy(m => m.Id))
            {
                var swedishRecord = group.Single(m => m.CultureCode == "sv-SE");
                var englishRecord = group.Single(m => m.CultureCode == "en-GB");

                var val = new FieldMappingWithCategoryValue();
                val.Id = int.Parse(group.Key);
                val.Description = string.IsNullOrWhiteSpace(englishRecord.Translation) ? "empty" : englishRecord.Translation;
                val.Translations = new List<FieldMappingTranslation>
                {
                    new FieldMappingTranslation()
                    {
                        CultureCode = swedishRecord.CultureCode, Value = swedishRecord.Translation
                    },
                    new FieldMappingTranslation()
                    {
                        CultureCode = englishRecord.CultureCode, Value = englishRecord.Translation
                    }
                };

                val.Category = new FieldMappingValue();
                val.Category.Id = int.Parse(swedishRecord.CategoryId);
                val.Category.Description = englishRecord.CategoryName;
                val.Category.Translations = new List<FieldMappingTranslation>
                {
                    new FieldMappingTranslation()
                    {
                        CultureCode = swedishRecord.CultureCode, Value = swedishRecord.CategoryName
                    },
                    new FieldMappingTranslation()
                    {
                        CultureCode = englishRecord.CultureCode, Value = englishRecord.CategoryName
                    }
                };

                fieldMappings.Add(val);
            }

            return fieldMappings;
        }

        private List<dynamic> ReadRecordsFromCsvFile(string filePath)
        {
            var csvFile = File.OpenRead(filePath);
            using var taxonReader = new StreamReader(csvFile, Encoding.UTF8);
            using var csvReader = new CsvReader(taxonReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                Encoding = Encoding.UTF8,
                HasHeaderRecord = true
            });

            List<dynamic> records = csvReader.GetRecords<dynamic>().ToList();
            return records;
        }

        private void SetCsvConfigurations(CsvReader csv)
        {
            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.Delimiter = "\t";
            csv.Configuration.Encoding = System.Text.Encoding.UTF8;
            csv.Configuration.BadDataFound = x => { Console.WriteLine($"Bad data: <{x.RawRecord}>"); };
        }

        private FieldMappingValue ToFieldMappingValue(FieldMappingWithCategoryValue val)
        {
            return new FieldMappingValue
            {
                Id = val.Id,

                Description = val.Description,
                Translations = val.Translations
            };
        }
    }
}
