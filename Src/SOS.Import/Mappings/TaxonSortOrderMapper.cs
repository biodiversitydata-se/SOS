using CsvHelper.Configuration;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Import.Mappings
{
    public sealed class TaxonSortOrderMapper : ClassMap<TaxonSortOrder<string>>
    {
        public TaxonSortOrderMapper()
        {
            Map(v => v.TaxonId).Name("taxonId").Index(0);
            Map(v => v.SortOrder).Name("sortOrder").Index(1);            
        }
    }
}