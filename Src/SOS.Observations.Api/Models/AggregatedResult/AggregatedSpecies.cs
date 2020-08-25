namespace SOS.Observations.Api.Models.AggregatedResult
{
    public class AggregatedSpecies
    {
        public int TaxonId { get; set; }
        public long? DocCount { get; set; }
        public string VernacularName { get; set; }
        public string ScientificNameAuthorship { get; set; }
        public string ScientificName { get; set; }
        public string RedlistCategory { get; set; }

    }

    public class AggregatedSpeciesInfo
    {
        public SpeciesAggregatedInfoTaxon Taxon { get; set; }
    }

    public class SpeciesAggregatedInfoTaxon
    {
        public int Id { get; set; }
        public string VernacularName { get; set; }
        public string ScientificNameAuthorship { get; set; }
        public string ScientificName { get; set; }
        public string RedlistCategory { get; set; }
    }
}