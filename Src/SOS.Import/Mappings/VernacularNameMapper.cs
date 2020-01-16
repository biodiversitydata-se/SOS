using CsvHelper.Configuration;
using  SOS.Lib.Models.DarwinCore;

namespace SOS.Import.Mappings
{
    public sealed class VernacularNameMapper : ClassMap<DarwinCoreVernacularName>
    {
        public VernacularNameMapper()
        {
            Map(v => v.TaxonID).Name("taxonId").Index(0);
            Map(v => v.VernacularName).Name("vernacularName").Index(1);
            Map(v => v.Language).Name("language").Index(2);
            Map(v => v.CountryCode).Name("countryCode").Index(3);
            Map(v => v.Source).Name("source").Index(4);
            Map(v => v.IsPreferredName).Name("isPreferredName").Index(5);
            Map(v => v.TaxonRemarks).Name("taxonRemarks").Index(6);
        }
    }
}
