using CsvHelper.Configuration;
using  SOS.Lib.Models.DarwinCore;

namespace SOS.Import.Mappings
{
    public sealed class TaxonMapper : ClassMap<DarwinCoreTaxon>
    {
        public TaxonMapper()
        {
            Map(t => t.TaxonID).Name("taxonId").Index(0);
            Map(x => x.AcceptedNameUsageID).Name("acceptedNameUsageID").Index(1);
            Map(x => x.ParentNameUsageID).Name("parentNameUsageID").Index(2);
            Map(x => x.ScientificName).Name("scientificName").Index(3);
            Map(x => x.TaxonRank).Name("taxonRank").Index(4);
            Map(x => x.ScientificNameAuthorship).Name("scientificNameAuthorship").Index(5);
            Map(x => x.TaxonomicStatus).Name("taxonomicStatus").Index(6);
            Map(x => x.NomenclaturalStatus).Name("nomenclaturalStatus").Index(7);
            Map(x => x.TaxonRemarks).Name("taxonRemarks").Index(8);
            Map(x => x.Kingdom).Name("kingdom").Index(9);
            Map(x => x.Phylum).Name("phylum").Index(10);
            Map(x => x.Class).Name("class").Index(11);
            Map(x => x.Order).Name("order").Index(12);
            Map(x => x.Family).Name("family").Index(13);
            Map(x => x.Genus).Name("genus").Index(14);
           // Map(x => x.DynamicProperties).Ignore(true);
        }
    }
}
