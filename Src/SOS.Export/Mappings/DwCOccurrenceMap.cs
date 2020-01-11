using CsvHelper.Configuration;
using SOS.Export.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    /// Mapping of Darwin Core to csv
    /// </summary>
    public class DwCOccurrenceMap : ClassMap<DwCOccurrence>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DwCOccurrenceMap()
        {
            Map(m => m.CoreID).Index(0).Name("coreId");
            Map(m => m.OccurrenceID).Index(1).Name("occurrenceID");
            Map(m => m.CatalogNumber).Index(2).Name("catalogNumber");
            Map(m => m.RecordNumber).Index(3).Name("recordNumber");
            Map(m => m.RecordedBy).Index(4).Name("recordedBy");
            Map(m => m.IndividualCount).Index(5).Name("individualCount");
            Map(m => m.OrganismQuantity).Index(6).Name("organismQuantity");
            Map(m => m.OrganismQuantityType).Index(7).Name("organismQuantityType");
            Map(m => m.Sex).Index(8).Name("sex");
            Map(m => m.LifeStage).Index(9).Name("lifeStage");
            Map(m => m.ReproductiveCondition).Index(10).Name("reproductiveCondition");
            Map(m => m.Behavior).Index(11).Name("behavior");
            Map(m => m.EstablishmentMeans).Index(12).Name("establishmentMeans");
            Map(m => m.OccurrenceStatus).Index(13).Name("occurrenceStatus");
            Map(m => m.Preparations).Index(14).Name("preparations");
            Map(m => m.Disposition).Index(15).Name("disposition");
            Map(m => m.AssociatedMedia).Index(16).Name("associatedMedia");
            Map(m => m.AssociatedReferences).Index(17).Name("associatedReferences");
            Map(m => m.AssociatedSequences).Index(18).Name("associatedSequences");
            Map(m => m.AssociatedTaxa).Index(19).Name("associatedTaxa");
            Map(m => m.OtherCatalogNumbers).Index(20).Name("otherCatalogNumbers");
            Map(m => m.OccurrenceRemarks).Index(21).Name("occurrenceRemarks");
        }
    }
}