using SOS.Lib.Models.Processed.Observation;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class TaxonExtensions
    {
        public static TaxonDto ToDto(this Taxon taxon)
        {
            return new TaxonDto
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
