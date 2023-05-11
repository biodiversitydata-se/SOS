using SOS.AutomaticIntegrationTests.TestFixtures;
using System.Linq;
using System.Text;
using SOS.Lib.Enums.VocabularyValues;
using Xunit;
using System.IO.Compression;
using System.IO;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;

namespace SOS.AutomaticIntegrationTests.DataUtils
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class CreateTaxonTestData
    {
        private readonly IntegrationTestFixture _fixture;

        public CreateTaxonTestData(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(Skip = "Intended to run on demand when needed")]
        [Trait("Category", "DataUtil")]
        public void CreateRedlistTestData()
        {
            var taxonIdByRedlistCategory = _fixture.Taxa
                .Where(m => m.Attributes.TaxonCategory.Id == (int)TaxonCategoryId.Species)
                .GroupBy(m => m.Attributes.RedlistCategory)
                .ToDictionary(g => g.Key ?? "", g => g.Select(v => v.Id).ToList());
            
            StringBuilder sb = new StringBuilder();
            foreach (var pair in taxonIdByRedlistCategory)
            {
                string key = $"\"{pair.Key ?? ""}\"";
                sb.AppendLine($"{{ {key}, new[] {{ {string.Join(", ", pair.Value.Take(50))} }} }},");
            }

            string str = sb.ToString(); // Use this string in ProtectedSpeciesHelper.RedlistedTaxonIdsByCategory
        }


        [Fact(Skip = "Intended to run on demand when needed")]
        [Trait("Category", "DataUtil")]
        public void CreateSensitiveSpeciesTestData()
        {
            var taxonIdsBySensitivityCategory = _fixture.Taxa
                .Where(m => m.Attributes.SensitivityCategory != null && m.Attributes.SensitivityCategory.Id > 1)
                .GroupBy(m => m.Attributes.SensitivityCategory.Id)
                .ToDictionary(g => g.Key, g => g.Select(v => v.Id).ToList());

            StringBuilder sb = new StringBuilder();
            foreach (var pair in taxonIdsBySensitivityCategory)
            {                
                sb.AppendLine($"{{ {pair.Key}, new[] {{ {string.Join(", ", pair.Value.Take(50))} }} }},");
            }
            string str = sb.ToString(); // Use this string in ProtectedSpeciesHelper.SensitiveSpeciesByCategory
        }

        [Fact(Skip = "Intended to run on demand when needed")]
        [Trait("Category", "DataUtil")]
        public void CreateSensitiveSpeciesTestDataVerbose()
        {
            var taxaBySensitivityCategory = _fixture.Taxa
                .Where(m => m.Attributes.SensitivityCategory != null && m.Attributes.SensitivityCategory.Id > 1)
                .GroupBy(m => m.Attributes.SensitivityCategory.Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            StringBuilder sb = new StringBuilder();
            foreach (var pair in taxaBySensitivityCategory)
            {
                sb.AppendLine($"{{ {pair.Key}, new[] {{ {string.Join(", ", pair.Value.Select(m => $"(TaxonId: {m.Id}, Name: \"{m.VernacularName}\")").Take(50))} }} }},");
            }
            string str = sb.ToString();
        }

        [Fact(Skip = "Intended to run on demand when needed")]        
        [Trait("Category", "DataUtil")]
        public void CreateTaxonZipFile()
        {            
            var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            var strJson = System.Text.Json.JsonSerializer.Serialize(_fixture.Taxa, jsonSerializerOptions);
            //System.IO.File.WriteAllText(@"C:\Temp\TaxonCollection.json", strJson2, Encoding.UTF8);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var taxonCollectionFile = archive.CreateEntry("TaxonCollection.json");

                    using (var entryStream = taxonCollectionFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        
                        streamWriter.Write(strJson);
                    }
                }

                using (var fileStream = new FileStream(@"C:\Temp\TaxonCollection.zip", FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }
        }

        [Fact(Skip = "Intended to run on demand when needed")]
        [Trait("Category", "DataUtil")]
        public void CreateTaxonCsvFile()
        {
            using var fileStream = File.Create(@"C:\Temp\TaxonCollection.csv");
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(fileStream, "\t");
            
            // Write header
            csvFileHelper.WriteField("TaxonId");
            csvFileHelper.WriteField("ParentId");
            csvFileHelper.WriteField("SecondaryParentIds");
            csvFileHelper.WriteField("ScientificName");
            csvFileHelper.WriteField("VernacularName");
            csvFileHelper.WriteField("TaxonCategory");
            csvFileHelper.WriteField("TaxonCategoryId");
            csvFileHelper.NextRecord();

            // Write Rows
            foreach (var taxon in _fixture.Taxa)
            {
                csvFileHelper.WriteField(taxon.Id.ToString());
                csvFileHelper.WriteField(taxon.Attributes.ParentDyntaxaTaxonId.HasValue ? taxon.Attributes.ParentDyntaxaTaxonId.ToString() : null);
                csvFileHelper.WriteField(taxon.SecondaryParentDyntaxaTaxonIds != null && taxon.SecondaryParentDyntaxaTaxonIds.Any() ? string.Join(", ", taxon.SecondaryParentDyntaxaTaxonIds) : null);
                csvFileHelper.WriteField(taxon.ScientificName.Clean());
                csvFileHelper.WriteField(taxon.VernacularName.Clean());
                csvFileHelper.WriteField(((TaxonCategoryId)taxon.Attributes.TaxonCategory.Id).ToString());
                csvFileHelper.WriteField(taxon.Attributes.TaxonCategory.Id.ToString());

                csvFileHelper.NextRecord();
            }

            csvFileHelper.FinishWrite();
        }
    }
}