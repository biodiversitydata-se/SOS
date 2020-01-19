using CsvHelper.Configuration;
using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Import.Mappings
{
    public sealed class TaxonRelationMapper : ClassMap<TaxonRelation<string>>
    {
        public TaxonRelationMapper()
        {
            Map(v => v.ParentTaxonId).Name("parentTaxonId").Index(0);
            Map(v => v.ChildTaxonId).Name("childTaxonId").Index(1);
            Map(v => v.IsMainRelation).Name("isMainRelation").Index(2);
        }
    }
}