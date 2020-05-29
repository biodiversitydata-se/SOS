using CsvHelper.Configuration;
using SOS.Export.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    ///     Mapping of Darwin Core to csv
    /// </summary>
    public class DwCTaxonMap : ClassMap<DwCTaxon>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public DwCTaxonMap()
        {
            Map(m => m.CoreID).Index(0).Name("coreId");
            Map(m => m.TaxonID).Index(1).Name("taxonID");
            Map(m => m.ScientificNameID).Index(2).Name("scientificNameID");
            Map(m => m.AcceptedNameUsageID).Index(3).Name("acceptedNameUsageID");
            Map(m => m.ParentNameUsageID).Index(4).Name("parentNameUsageID");
            Map(m => m.OriginalNameUsageID).Index(5).Name("originalNameUsageID");
            Map(m => m.NameAccordingToID).Index(6).Name("nameAccordingToID");
            Map(m => m.NamePublishedInID).Index(7).Name("namePublishedInID");
            Map(m => m.TaxonConceptID).Index(8).Name("taxonConceptID");
            Map(m => m.ScientificName).Index(9).Name("scientificName");
            Map(m => m.AcceptedNameUsage).Index(10).Name("acceptedNameUsage");
            Map(m => m.ParentNameUsage).Index(11).Name("parentNameUsage");
            Map(m => m.OriginalNameUsage).Index(12).Name("originalNameUsage");
            Map(m => m.NameAccordingTo).Index(13).Name("nameAccordingTo");
            Map(m => m.NamePublishedIn).Index(14).Name("namePublishedIn");
            Map(m => m.NamePublishedInYear).Index(15).Name("namePublishedInYear");
            Map(m => m.HigherClassification).Index(16).Name("higherClassification");
            Map(m => m.Kingdom).Index(17).Name("kingdom");
            Map(m => m.Phylum).Index(18).Name("phylum");
            Map(m => m.Class).Index(19).Name("class");
            Map(m => m.Order).Index(20).Name("order");
            Map(m => m.Family).Index(21).Name("family");
            Map(m => m.Genus).Index(22).Name("genus");
            Map(m => m.Subgenus).Index(23).Name("subgenus");
            Map(m => m.SpecificEpithet).Index(24).Name("specificEpithet");
            Map(m => m.InfraspecificEpithet).Index(25).Name("infraspecificEpithet");
            Map(m => m.TaxonRank).Index(26).Name("taxonRank");
            Map(m => m.VerbatimTaxonRank).Index(27).Name("verbatimTaxonRank");
            Map(m => m.ScientificNameAuthorship).Index(28).Name("scientificNameAuthorship");
            Map(m => m.VernacularName).Index(29).Name("vernacularName");
            Map(m => m.NomenclaturalCode).Index(30).Name("nomenclaturalCode");
            Map(m => m.TaxonomicStatus).Index(31).Name("taxonomicStatus");
            Map(m => m.NomenclaturalStatus).Index(32).Name("nomenclaturalStatus");
            Map(m => m.TaxonRemarks).Index(33).Name("taxonRemarks");
        }
    }
}