namespace SOS.Lib.Models.Processed.AggregatedResult
{
    /// <summary>
    ///     Fields to return from aggregated result
    /// </summary>
    public class AggregatedSpecies
    {
        /// <summary>
        /// Taxon Id
        /// </summary>
        public int TaxonId { get; set; }
        /// <summary>
        /// No of documents
        /// </summary>
        public long? DocCount { get; set; }
        /// <summary>
        /// Vernacular Name
        /// </summary>
        public string VernacularName { get; set; }
        /// <summary>
        /// Authorship
        /// </summary>
        public string ScientificNameAuthorship { get; set; }
        /// <summary>
        /// Scientific Name
        /// </summary>
        public string ScientificName { get; set; }
        /// <summary>
        /// Redlist Category
        /// </summary>
        public string RedlistCategory { get; set; }

    }

    /// <summary>
    ///     Used to get hold of info from Document's section in aggregated result from Elastic
    /// </summary>
    public class AggregatedSpeciesInfo
    {
        /// <summary>
        /// Taxon-object
        /// </summary>
        public SpeciesAggregatedInfoTaxon Taxon { get; set; }
    }

    /// <summary>
    ///     Used to get hold of info from Document's section in aggregated result from Elastic
    /// </summary>
    public class SpeciesAggregatedInfoTaxon
    {
        /// <summary>
        ///     Taxon Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Vernacular Name
        /// </summary>
        public string VernacularName { get; set; }
        /// <summary>
        /// Authorship
        /// </summary>
        public string ScientificNameAuthorship { get; set; }
        /// <summary>
        /// Scientific Name
        /// </summary>
        public string ScientificName { get; set; }        

        /// <summary>
        /// Taxon attributes.
        /// </summary>
        public SpeciesAggregatedInfoTaxonAttributes Attributes { get; set; }
    }

    /// <summary>
    /// Taxon attributes in aggregated result.
    /// </summary>
    public class SpeciesAggregatedInfoTaxonAttributes
    {
        /// <summary>
        /// Redlist Category
        /// </summary>
        public string RedlistCategory { get; set; }
    }
}