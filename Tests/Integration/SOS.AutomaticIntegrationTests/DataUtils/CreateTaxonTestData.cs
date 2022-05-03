using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.TestHelpers.JsonConverters;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Enums.VocabularyValues;
using Xunit;
using System.IO.Compression;
using System.IO;

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
    }
}