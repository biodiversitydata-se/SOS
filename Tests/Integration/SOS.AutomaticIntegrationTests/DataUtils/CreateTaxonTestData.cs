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
    }
}