using SOS.Lib.Models.Processed.Observation;

namespace SOS.Shared.Api.Dtos.DataStewardship.Extensions;

public static class TaxonExtensions
{
    extension(Taxon taxon)
    {
        public DsTaxonDto ToDto()
        {
            return new DsTaxonDto
            {
                ScientificName = taxon.ScientificName,
                TaxonID = taxon.Id.ToString(),
                TaxonRank = taxon.TaxonRank,
                VernacularName = taxon.VernacularName,
                VerbatimTaxonID = taxon.VerbatimId,
                VerbatimName = taxon.VerbatimName
            };
        }
    }
}
